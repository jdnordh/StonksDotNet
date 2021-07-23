using Microsoft.AspNetCore.SignalR;
using Models.DataTransferObjects;
using Models.Game;
using StonkTrader.Hubs;
using StonkTrader.Models.Connection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SignalRSample.Hubs
{
    public class GameHub : Hub
    {
		private string CurrentUserId
		{ 
			get 
			{
				return Context.ConnectionId;
			}
		}

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

		#region Game Creation and Joining

		public async Task CreateGame()
		{
			if (GameInstance.OberserverConnectionId != null)
            {
				await Clients.Caller.SendAsync(ClientMethods.CreateGameUnavailable);
				return;
            }
			GameInstance.OberserverConnectionId = CurrentUserId;

			await Clients.Caller.SendAsync(ClientMethods.GameCreated);
		}

        public async Task JoinGame(string username)
		{
			// Add player to game
			var safeUsername = Regex.Replace(username, "[^0-9a-zA-Z]+", "");
			var inventory = GameInstance.Game.AddPlayer(CurrentUserId, safeUsername);
			inventory.Username = safeUsername;
			await Clients.Caller.SendAsync(ClientMethods.GameJoined, inventory);
		}

		public async Task StartGame()
		{
			GameInstance.Game.StartGame();
			await Clients.All.SendAsync(ClientMethods.GameStarted, GameInstance.Game.Stocks);
		}

		#endregion

		#region Transactions

		public async Task RequestTransaction(string stockName, bool isBuy, int amount)
		{
			bool transactionWasSuccessful = false;
			PlayerInventoryDto inventory = null;
			if (isBuy)
			{
				if (GameInstance.Game.IsBuyOkay(CurrentUserId, stockName, amount))
				{
					inventory = GameInstance.Game.BuyStock(CurrentUserId, stockName, amount);
					transactionWasSuccessful = true;
				}
			}
			else
			{
				if (GameInstance.Game.IsSellOkay(CurrentUserId, stockName, amount))
				{
					inventory = GameInstance.Game.SellStock(CurrentUserId, stockName, amount);
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
    }
}
