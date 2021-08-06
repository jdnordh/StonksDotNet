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
		private const string PlayerGroup = "Player";
		private const string GameThreadsGroup = "GameThreads";

		#region Connection
		public static class UserHandler
		{
			public static HashSet<string> ConnectedIds = new HashSet<string>();
		}

		public override Task OnConnectedAsync()
		{
			UserHandler.ConnectedIds.Add(CurrentUserConnectionId);
			return base.OnConnectedAsync();
		}

		public async override Task OnDisconnectedAsync(Exception exception)
		{
			UserHandler.ConnectedIds.Remove(CurrentUserConnectionId);
			if (UserHandler.ConnectedIds.Count == 0)
			{
				await Clients.Group(GameThreadsGroup).SendAsync(GameWorkerRequests.GameEndRequest);
			}
			await base.OnDisconnectedAsync(exception);
		}

		#endregion

		private string CurrentUserConnectionId
		{
			get 
			{
				return Context.ConnectionId;
			}
		}

		#region Communication With Game Threads

		/// <summary>
		/// Called when the game thread joins the hub.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>A completed task.</returns>
		public async Task GameThreadJoined(string key)
		{
			if (key != GameWorkerResponses.Key)
			{
				return;
			}
			WorkerManager.Instance.SetWorkerConnectionId(CurrentUserConnectionId);
			await Groups.AddToGroupAsync(CurrentUserConnectionId, GameThreadsGroup);
		}

		/// <summary>
		/// Called when a create game request was sent.
		/// </summary>
		/// <param name="creatorConnectionId">The creator connection id.</param>
		/// <param name="success">If the creation was successful.</param>
		/// <returns>A completed task.</returns>
		public async Task GameCreated(string creatorConnectionId, bool success)
		{
			if (success)
			{
				await Groups.AddToGroupAsync(creatorConnectionId, PlayerGroup);
				await Clients.Client(creatorConnectionId).SendAsync(ClientMethods.GameCreated);
			}
			else
			{
				await Clients.Client(creatorConnectionId).SendAsync(ClientMethods.CreateGameUnavailable);
			}
		}

		/// <summary>
		/// Called when a start game request was sent.
		/// </summary>
		/// <param name="creatorConnectionId">The creator connection id.</param>
		/// <returns>A completed task.</returns>
		public async Task GameStarted(string creatorConnectionId)
		{
			await Clients.Client(creatorConnectionId).SendAsync(ClientMethods.GameStarted);
		}

		/// <summary>
		/// Called when a game is joined by a player.
		/// </summary>
		/// <param name="connectionId">The connection id of the joined player.</param>
		/// <param name="inventory">The player inventory.</param>
		/// <returns>A completed task.</returns>
		public async Task GameJoinedPlayer(string connectionId, PlayerInventoryDto inventory)
		{
			await Groups.AddToGroupAsync(connectionId, PlayerGroup);
			await Clients.Client(connectionId).SendAsync(ClientMethods.GameJoined, inventory);
		}

		/// <summary>
		/// Called when a game is joined by an observer.
		/// </summary>
		/// <param name="connectionId">The connection id of the joined player.</param>
		/// <param name="marketDto">The market dto.</param>
		/// <returns>A completed task.</returns>
		public async Task GameJoinedObserver(string connectionId, MarketDto marketDto)
		{
			await Groups.AddToGroupAsync(connectionId, PlayerGroup);
			await Clients.Client(connectionId).SendAsync(ClientMethods.GameJoinedObserver, marketDto);
		}

		/// <summary>
		/// Called when a transaction was posted.
		/// </summary>
		/// <param name="connectionId">The connection Id.</param>
		/// <param name="inventory">The player's updated inventory.</param>
		/// <param name="success">If the transaction was successful.</param>
		/// <returns>A completed task.</returns>
		public async Task TransactionPosted(string connectionId, PlayerInventoryDto inventory, bool success)
		{
			if (success)
			{
				await Clients.Client(connectionId).SendAsync(ClientMethods.InventoryUpdated, inventory);
			}
			else
			{
				await Clients.Client(connectionId).SendAsync(ClientMethods.TransactionFailed);
			}
		}

		/// <summary>
		/// Called when the market is updated.
		/// </summary>
		/// <param name="marketDto">The market dto.</param>
		/// <returns>A completed task.</returns>
		public async Task MarketUpdated(MarketDto marketDto)
		{
			await Clients.Group(PlayerGroup).SendAsync(ClientMethods.MarketUpdated, marketDto);
		}

		/// <summary>
		/// Called when the market is updated for an individual client.
		/// </summary>
		/// <param name="connectionId">The connection Id.</param>
		/// <param name="marketDto">The market dto.</param>
		/// <returns>A completed task.</returns>
		public async Task MarketUpdatedIndividual(string connectionId, MarketDto marketDto)
		{
			await Clients.Client(connectionId).SendAsync(ClientMethods.MarketUpdated, marketDto);
		}

		/// <summary>
		/// Called when a roll happens.
		/// </summary>
		/// <param name="marketDto">The market dto.</param>
		/// <returns>A completed task.</returns>
		public async Task Rolled(MarketDto marketDto)
		{
			await Clients.Group(PlayerGroup).SendAsync(ClientMethods.Rolled, marketDto);
		}

		/// <summary>
		/// Called when a roll happens.
		/// </summary>
		/// <param name="marketDto">The market dto.</param>
		/// <returns>A completed task.</returns>
		public async Task InventoriesUpdated(PlayerInventoryCollectionDto playerInventoryCollectionDto)
		{
			foreach (KeyValuePair<string, PlayerInventoryDto> inventory in playerInventoryCollectionDto.Inventories)
			{
				await Clients.Client(inventory.Key).SendAsync(ClientMethods.InventoryUpdated, inventory.Value);
			}
		}

		/// <summary>
		/// Called when the game finishes.
		/// </summary>
		/// <returns>A completed task.</returns>
		public async Task GameOver(GameOverDto gameOverDto)
		{
			await Clients.Group(PlayerGroup).SendAsync(ClientMethods.GameOver, gameOverDto);
		}

		/// <summary>
		/// Called when the game is nullified.
		/// </summary>
		/// <returns>A completed task.</returns>
		public async Task GameEnded()
		{
			await Clients.Group(PlayerGroup).SendAsync(ClientMethods.GameEnded);
		}

		#endregion

		#region Communication with Clients

		#region Game Creation and Joining

		public async Task JoinGame(string username, bool isPlayer)
		{
			if (!WorkerManager.Instance.WorkerExists)
			{
				return;
			}

			await Clients.Group(GameThreadsGroup).SendAsync(GameWorkerRequests.JoinGameRequest, CurrentUserConnectionId, username, isPlayer);
		}

		public async Task CreateGame(GameInitializerDto parameters)
		{
			if (!WorkerManager.Instance.WorkerExists)
			{
				await Clients.Caller.SendAsync(ClientMethods.CreateGameUnavailable);
				return;
			}

			await Clients.Group(GameThreadsGroup).SendAsync(GameWorkerRequests.CreateGameRequest, parameters, CurrentUserConnectionId);
		}

		public async Task EndGame()
		{
			if (!WorkerManager.Instance.WorkerExists)
			{
				return;
			}
			await Clients.Group(GameThreadsGroup).SendAsync(GameWorkerRequests.GameEndRequest);
		}

		public async Task StartGame()
		{
			if (!WorkerManager.Instance.WorkerExists)
			{
				return;
			}
			await Clients.Group(GameThreadsGroup).SendAsync(GameWorkerRequests.StartGameRequest, CurrentUserConnectionId);
		}

		#endregion

		#region Transactions

		public async Task RequestTransaction(string stockName, bool isBuy, int amount)
		{
			if (!WorkerManager.Instance.WorkerExists)
			{
				return;
			}
			await Clients.Group(GameThreadsGroup).SendAsync(GameWorkerRequests.TransactionRequest, CurrentUserConnectionId, stockName, isBuy, amount);
		}

		#endregion

		#endregion

		#region Utilities

		private string GetSafeUsername(string submittedUsername)
		{
			return Regex.Replace(submittedUsername, "[^0-9a-zA-Z ]+", "");
		}

		#endregion

		public async Task Reset()
		{
			WorkerManager.Instance.SetWorkerConnectionId(CurrentUserConnectionId);
			await Clients.All.SendAsync(ClientMethods.GameEnded);
		}
	}
}
