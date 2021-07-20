using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Models.StockTicker.GameClasses;
using StockTickerDotNetCore.Models.Connection;
using StockTickerDotNetCore.Models.DataTransferObjects;
using StockTickerDotNetCore.Models.StockTicker;
using StockTickerDotNetCore.Models.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockTickerDotNetCore.Controllers
{
	[Authorize]
	public class GameHub : Hub
	{
		private readonly static ConnectionMapping<string> m_connections = new ConnectionMapping<string>();
		private readonly static GameCollection<GameConnection> m_gameConnections = new GameCollection<GameConnection>();

		#region Client Calls

		private static class ClientMethods
		{
			public const string GameCreated = "gameCreated";
			public const string GameJoined = "gameJoined";
			public const string InventoryUpdated = "inventoryUpdated";
			public const string MarketUpdated = "marketUpdated";
			public const string TransactionFailed = "transactionFailed";
		}

		#region Game Event Handlers

		private void GameDividendsPaid(StonkTraderGame senderGame)
		{
			foreach(var kvp in senderGame.Players)
			{
				Clients.Client(kvp.Key).SendAsync(ClientMethods.InventoryUpdated, kvp.Value.GetPlayerInvetory());
			}
		}

		private void GameMarketChanged(StonkTraderGame senderGame)
		{
			foreach (var kvp in senderGame.Players)
			{
				Clients.Client(kvp.Key).SendAsync(ClientMethods.MarketUpdated, senderGame.GetMarketDto());
			}
		}

		#endregion

		#region Game Creation and Joining

		public async Task CreateGame(GameInitializer initializer)
		{
			var gameConnection = new GameConnection(new StonkTraderGame(initializer), Context.User.Identity.Name);
			string gameId = m_gameConnections.AddGame(gameConnection);

			gameConnection.Game.DividendsPaid += GameDividendsPaid;
			gameConnection.Game.MarketOpened += GameMarketChanged;
			gameConnection.Game.MarketClosed += GameMarketChanged;

			await Clients.Caller.SendAsync("gameCreated", gameId);
		}

		public async Task JoinGame(string gameId, string username)
		{
			bool gameJoined = false;
			if (m_gameConnections.GameExists(gameId))
			{
				var userId = Context.User.Identity.Name;
				var user = m_connections.GetUser(userId);
				gameJoined = true;

				// Add player to game connection
				var gameConnection = m_gameConnections.GetGame(gameId);
				gameConnection.PlayerNames.Add(userId);
				user.GameId = gameId;

				// Add player to game
				gameConnection.Game.AddPlayer(userId, new Player(username, gameConnection.Game.Stocks.Values.Select(s => s.Name).ToList()));
			}
			await Clients.Caller.SendAsync("gameJoined", gameJoined);
		}

		#endregion

		#region Gameplay

		private bool UserIsRegisteredInGame(string userId)
		{
			var user = m_connections.GetUser(userId);
			return user.GameId != null;
		}

		public void Roll()
		{
			var user = m_connections.GetUser(Context.User.Identity.Name);
			if (!UserIsRegisteredInGame(user.Id))
			{
				return;
			}
			var gameConnection = m_gameConnections.GetGame(user.GameId);
			if (gameConnection.Game.UserPlayingTurn == user.Id)
			{
				gameConnection.Game.Roll(user.Id);
			}
		}

		public async Task RequestTransaction(PlayerTransaction transaction)
		{
			var user = m_connections.GetUser(Context.User.Identity.Name);
			if (!UserIsRegisteredInGame(user.Id))
			{
				return;
			}

			var gameConnection = m_gameConnections.GetGame(user.GameId);
			bool transactionWasSuccessful = false;
			PlayerInventoryDto inventory = null;
			if (transaction.IsBuyTransaction)
			{
				if (gameConnection.Game.IsBuyOkay(user.Id, transaction.StockName, transaction.StockAmount))
				{
					inventory = gameConnection.Game.BuyStock(user.Id, transaction.StockName, transaction.StockAmount);
					transactionWasSuccessful = true;
				}
			}
			else
			{
				if (gameConnection.Game.IsSellOkay(user.Id, transaction.StockName, transaction.StockAmount))
				{
					inventory = gameConnection.Game.SellStock(user.Id, transaction.StockName, transaction.StockAmount);
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

		#endregion

		#region Connection

		public override Task OnConnectedAsync()
		{
			string name = Context.User.Identity.Name;

			m_connections.Add(name, Context.ConnectionId);

			return base.OnConnectedAsync();
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			string name = Context.User.Identity.Name;
			m_connections.Remove(name, Context.ConnectionId);

			return base.OnDisconnectedAsync(exception);
		}

		#endregion
	}
}
