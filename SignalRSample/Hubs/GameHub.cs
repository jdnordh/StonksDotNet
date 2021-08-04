using Microsoft.AspNetCore.SignalR;
using Models.DataTransferObjects;
using Models.Game;
using Newtonsoft.Json;
using StonkTrader.Models.Connection;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hubs
{
	public class GameHub : Hub
	{
		public static class UserHandler
		{
			public static HashSet<string> ConnectedIds = new HashSet<string>();
		}

		private string CurrentUserConnectionId
		{
			get {
				return Context.ConnectionId;
			}
		}

		#region Connection

		public override Task OnConnectedAsync()
		{
			UserHandler.ConnectedIds.Add(CurrentUserConnectionId);
			return base.OnConnectedAsync();
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			UserHandler.ConnectedIds.Remove(CurrentUserConnectionId);
			if (UserHandler.ConnectedIds.Count == 0)
			{
				GameManager.Instance.EndGame();
			}
			return base.OnDisconnectedAsync(exception);
		}

		#endregion

		#region Game Creation and Joining

		[Serializable]
		public class CreateGameParams
		{
			[JsonInclude]
			[JsonProperty("marketTime")]
			public int marketTime { get; set; }

			[JsonInclude]
			[JsonProperty("startingMoney")]
			public int startingMoney { get; set; }

			[JsonInclude]
			[JsonProperty("rollsPerRound")]
			public int rollsPerRound { get; set; }

			[JsonInclude]
			[JsonProperty("rounds")]
			public int rounds { get; set; }

			[JsonInclude]
			[JsonProperty("rollTime")]
			public int rollTime { get; set; }

			[JsonInclude]
			[JsonProperty("timeBetweenRolls")]
			public int timeBetweenRolls { get; set; }

			[JsonInclude]
			[JsonProperty("usePrototype")]
			public bool usePrototype { get; set; }
		}

		public async Task CreateGame(CreateGameParams obj)
		{
			CreateGameParams parameters = obj;

			if (parameters == null)
			{
				return;
			}
			if (GameManager.Instance.Game != null)
			{
				await Clients.Caller.SendAsync(ClientMethods.CreateGameUnavailable);
				return;
			}
			GameInitializer initializer = parameters.usePrototype ? GameManager.GetPrototypeGameInitializer() : GameManager.GetDefaultGameInitializer();
			initializer.MarketOpenTimeInSeconds = parameters.marketTime;
			initializer.StartingMoney = parameters.startingMoney;
			initializer.RollsPerRound = parameters.rollsPerRound;
			initializer.NumberOfRounds = parameters.rounds;
			initializer.RollTimeInSeconds = parameters.rollTime;
			initializer.TimeBetweenRollsInSeconds = parameters.timeBetweenRolls;

			GameManager.Instance.CreateNewGame(initializer);
			await Clients.Caller.SendAsync(ClientMethods.GameCreated);
		}

		public async Task EndGame()
		{
			if (GameManager.Instance.Game != null && !GameManager.Instance.Game.IsStarted)
			{
				GameManager.Instance.EndGame();
				await Clients.All.SendAsync(ClientMethods.GameEnded);
				return;
			}
		}

		public async Task JoinGame(string username, bool isPlayer)
		{
			if (GameManager.Instance.Game == null)
			{
				return;
			}

			if (isPlayer)
			{
				// Add player to game
				var safeUsername = GetSafeUsername(username);
				PlayerInventoryDto inventory = GameManager.Instance.Game.AddPlayer(CurrentUserConnectionId, safeUsername);
				await Clients.Caller.SendAsync(ClientMethods.GameJoined, inventory);
			}
			else
			{
				// Client is observer
				await Clients.Caller.SendAsync(ClientMethods.GameJoinedObserver, GameManager.Instance.Game.GetMarketDto());
			}

			// If game is already started, notify caller.
			if (GameManager.Instance.Game.IsStarted)
			{
				await Clients.Caller.SendAsync(ClientMethods.MarketUpdated, GameManager.Instance.Game.GetMarketDto());
			}
		}

		public async Task StartGame()
		{
			if (GameManager.Instance.Game == null || GameManager.Instance.Game.IsStarted)
			{
				return;
			}

			// Make sure observer gets the market before starting to present
			await Clients.Caller.SendAsync(ClientMethods.MarketUpdated, GameManager.Instance.Game.GetMarketDto());
			await Clients.Caller.SendAsync(ClientMethods.GameStarted);

			await GameManager.Instance.Game.RunGame();
		}

		#endregion

		#region Transactions

		public async Task RequestTransaction(string stockName, bool isBuy, int amount)
		{
			if (GameManager.Instance.Game == null || !GameManager.Instance.Game.IsStarted || !GameManager.Instance.Game.IsMarketOpen)
			{
				return;
			}
			var transactionWasSuccessful = false;
			PlayerInventoryDto inventory = null;
			if (isBuy)
			{
				if (GameManager.Instance.Game.IsBuyOkay(CurrentUserConnectionId, stockName, amount))
				{
					inventory = GameManager.Instance.Game.BuyStock(CurrentUserConnectionId, stockName, amount);
					transactionWasSuccessful = true;
				}
			}
			else
			{
				if (GameManager.Instance.Game.IsSellOkay(CurrentUserConnectionId, stockName, amount))
				{
					inventory = GameManager.Instance.Game.SellStock(CurrentUserConnectionId, stockName, amount);
					transactionWasSuccessful = true;
				}
			}

			if (transactionWasSuccessful)
			{
				await Clients.Caller.SendAsync(ClientMethods.InventoryUpdated, inventory);
			}
			else
			{
				await Clients.Caller.SendAsync(ClientMethods.TransactionFailed);
			}
		}

		#endregion

		#region Utilities

		private string GetSafeUsername(string submittedUsername)
		{
			return Regex.Replace(submittedUsername, "[^0-9a-zA-Z ]+", "");
		}

		#endregion

		public async Task Reset()
		{
			GameManager.Instance.EndGame();
			await Clients.All.SendAsync(ClientMethods.GameEnded);
		}
	}
}
