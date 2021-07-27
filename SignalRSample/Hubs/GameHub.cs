using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.SignalR;
using Models.DataTransferObjects;
using StonkTrader.Models.Connection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hubs
{
	public class GameHub : Hub
	{
		private string CurrentUserConnectionId
		{
			get
			{
				return Context.ConnectionId;
			}
		}

		#region Game Creation and Joining

		public async Task CreateGame()
		{
			if (GameManager.Instance.Game != null)
			{
				await Clients.Caller.SendAsync(ClientMethods.CreateGameUnavailable);
				return;
			}
			GameManager.Instance.CreateNewGame();
			await Clients.Caller.SendAsync(ClientMethods.GameCreated);
		}

		public async Task JoinGame(string username)
		{
			// Add player to game
			var safeUsername = GetSafeUsername(username);
			var inventory = GameManager.Instance.Game.AddPlayer(CurrentUserConnectionId, safeUsername);
			await Clients.Caller.SendAsync(ClientMethods.GameJoined, inventory);

			// If game is already started, notify caller.
			if (GameManager.Instance.Game.IsStarted)
			{
				await Clients.Caller.SendAsync(ClientMethods.MarketUpdated, GameManager.Instance.Game.GetMarketDto());
			}
		}

		public async Task StartGame()
		{
			if (GameManager.Instance.Game.IsStarted)
			{
				return;
			}
			GameManager.Instance.Game.StartGame();
			await Clients.All.SendAsync(ClientMethods.GameStarted);
		}

		#endregion

		#region Transactions

		public async Task RequestTransaction(string stockName, bool isBuy, int amount)
		{
			bool transactionWasSuccessful = false;
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
			return Regex.Replace(submittedUsername, "[^0-9a-zA-Z]+", "");
		}

		#endregion
	}
}
