using Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.DataTransferObjects;
using Models.Game;
using StonkTrader.Models.Game.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StonkTrader.Models.Workers
{
	public class GameWorker : BackgroundService, IGameEventCommunicator
	{
		private readonly ILogger<GameWorker> m_logger;

		//  TODO Make a dictionary of games instead of one.
		private StonkTraderGame m_game;
		private string m_creatorConnectionId;
		private HubConnection m_connection;

		private Dictionary<string, string> m_connectionIdToPlayerIdMap = new Dictionary<string, string>();
		private Dictionary<string, string> m_playerIdToConnectionIdMap = new Dictionary<string, string>();

		public GameWorker(ILogger<GameWorker> logger)
		{
			m_logger = logger;
			m_game = null;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			m_connection = new HubConnectionBuilder().WithUrl("http://localhost:5000/gamehub").Build();
			m_connection.ServerTimeout = TimeSpan.FromMilliseconds(1800000);

			m_connection.On<GameInitializerDto, string>(GameWorkerRequests.CreateGameRequest, CreateGame);

			m_connection.On<string, string, bool, int>(GameWorkerRequests.JoinGameRequest, JoinGame);

			m_connection.On<string, string>(GameWorkerRequests.ReJoinGameRequest, ReJoinGame);

			m_connection.On<string>(GameWorkerRequests.StartGameRequest, StartGame);

			m_connection.On<string, string, bool, int>(GameWorkerRequests.TransactionRequest, DoTransaction);

			m_connection.On<string>(GameWorkerRequests.RollPreviewRequest, RollPreview);

			m_connection.On<string>(GameWorkerRequests.TrendPreviewRequest, TrendPreview);

			m_connection.On<string, string>(GameWorkerRequests.StockPushDownRequest, StockPushDown);

			m_connection.On(GameWorkerRequests.GameEndRequest, EndGame);

			await Task.Run(async () => {
				m_logger.Log(LogLevel.Information, "Starting a connection with the game hub from worker...");
				await m_connection.StartAsync();
			}).ContinueWith(async (t) => {
				m_logger.Log(LogLevel.Information, "Invoking GameThreadJoined.");
				await m_connection.InvokeAsync(GameWorkerResponses.GameThreadJoined, GameWorkerResponses.Key);
			});

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					// todo: Add repeating code here
					// add a delay to not run in a tight loop
					await Task.Delay(1000, stoppingToken);
				}
				catch (OperationCanceledException)
				{
					// catch the cancellation exception
					// to stop execution
					await m_connection.StopAsync();
					return;
				}
			}
			await m_connection.StopAsync();
		}

		#region Request Handlers

		private void StockPushDown(string connectionId, string stockName)
		{
			if (!m_connectionIdToPlayerIdMap.TryGetValue(connectionId, out var playerId))
			{
				return;
			}
			m_game.RequestStockPushDown(playerId, stockName);
		}

		private async Task TrendPreview(string connectionId)
		{
			if (!m_connectionIdToPlayerIdMap.TryGetValue(connectionId, out var playerId))
			{
				return;
			}
			var trendDto = m_game.PreviewRoundTrend(playerId);
			if (trendDto != null)
			{
				await m_connection.InvokeAsync(GameWorkerResponses.TrendPreviewResponse, connectionId, trendDto);
			}
		}

		private async Task RollPreview(string connectionId)
		{
			if (!m_connectionIdToPlayerIdMap.TryGetValue(connectionId, out var playerId))
			{
				return;
			}
			var rollDto = m_game.PreviewFirstRoll(playerId);
			if (rollDto != null)
			{
				await m_connection.InvokeAsync(GameWorkerResponses.RollPreviewResponse, connectionId, rollDto);
			}
		}

		private async Task CreateGame(GameInitializerDto parameters, string creatorConnectionId)
		{
			if (m_game == null)
			{
				m_logger.Log(LogLevel.Information, "Creating game.");
				parameters.Stocks = StockPresetProvider.GetPreset(parameters.StockPreset).Stocks;

				m_game = new StonkTraderGame(parameters, this);
				m_creatorConnectionId = creatorConnectionId;
			}
			else
			{
				await m_connection.InvokeAsync(GameWorkerResponses.GameCreatedResponse, creatorConnectionId, false);
				return;
			}

			await m_connection.InvokeAsync(GameWorkerResponses.GameCreatedResponse, creatorConnectionId, true);
		}

		private async Task JoinGame(string connectionId, string username, bool isPlayer, int characterId)
		{
			if (m_game == null)
			{
				await m_connection.SendAsync(GameWorkerResponses.JoinGameFailed, connectionId);
				return;
			}
			if(m_connectionIdToPlayerIdMap.ContainsKey(connectionId))
			{
				// Connection id is already registered.
				return;
			}
			if (isPlayer)
			{
				m_logger.Log(LogLevel.Information, $"{username} is joining game.");
				PlayerInventoryDto inventory = m_game.AddPlayer(username, characterId);
				AddNewPlayer(connectionId, inventory.PlayerId);
				await m_connection.InvokeAsync(GameWorkerResponses.PlayerJoinedGameResponse, connectionId, inventory);
				if (m_game.IsStarted)
				{
					// If game is started, update the player with the market
					await m_connection.InvokeAsync(GameWorkerResponses.MarketUpdatedIndividual, connectionId, m_game.GetMarketDto());
				}
			}
			else
			{
				m_logger.Log(LogLevel.Information, "Observer is joining game.");
				await m_connection.InvokeAsync(GameWorkerResponses.ObserverJoinedGameResponse, connectionId, m_game.GetMarketDto());
			}
		}

		private async Task ReJoinGame(string connectionId, string playerId)
		{
			if (m_game == null)
			{
				await m_connection.SendAsync(GameWorkerResponses.JoinGameFailed, connectionId);
				return;
			}
			if (!m_playerIdToConnectionIdMap.ContainsKey(playerId))
			{
				await m_connection.SendAsync(GameWorkerResponses.JoinGameFailed, connectionId);
				return;
			}
			if (!m_game.Players.ContainsKey(playerId))
			{
				m_logger.Log(LogLevel.Error, "Player Id was not present in game.");
				await m_connection.SendAsync(GameWorkerResponses.JoinGameFailed, connectionId);
				return;
			}

			Player player = m_game.Players[playerId];
			UpdateConnectionId(connectionId, playerId);
			await m_connection.InvokeAsync(GameWorkerResponses.PlayerJoinedGameResponse, connectionId, player.GetPlayerInvetory());

			m_logger.Log(LogLevel.Information, $"{player.Username} is re-joining game.");

			// Update the player with the market if the game is open
			if (m_game.IsStarted)
			{
				await m_connection.InvokeAsync(GameWorkerResponses.MarketUpdatedIndividual, connectionId, m_game.GetMarketDto());
			}
		}

		private async Task StartGame(string creatorConnectionId)
		{
			if (m_game.IsStarted)
			{
				return;
			}
			m_logger.Log(LogLevel.Information, "Starting game.");
			await m_connection.InvokeAsync(GameWorkerResponses.MarketUpdatedIndividual, creatorConnectionId, m_game.GetMarketDto());
			await m_connection.InvokeAsync(GameWorkerResponses.GameStarted, creatorConnectionId);
			await m_game.StartGame();
			// Verify we return after starting the game
			m_logger.Log(LogLevel.Warning, "Started game.");

		}

		private async Task DoTransaction(string connectionId, string stockName, bool isBuy, int amount)
		{
			m_logger.Log(LogLevel.Information, $"Transaction posted: {(isBuy ? "Buy" : "Sell")} {amount} {stockName}");
			if (!m_game.IsMarketOpen)
			{
				return;
			}
			string playerId = m_connectionIdToPlayerIdMap[connectionId];

			var transactionWasSuccessful = false;
			PlayerInventoryDto inventory = null;
			if (isBuy)
			{
				if (m_game.IsBuyOkay(playerId, stockName, amount))
				{
					inventory = m_game.BuyStock(playerId, stockName, amount);
					transactionWasSuccessful = true;
				}
			}
			else
			{
				if (m_game.IsSellOkay(playerId, stockName, amount))
				{
					inventory = m_game.SellStock(playerId, stockName, amount);
					transactionWasSuccessful = true;
				}
			}

			await m_connection.InvokeAsync(GameWorkerResponses.TransactionPosted, connectionId, inventory, transactionWasSuccessful);
			await m_connection.InvokeAsync(GameWorkerResponses.PlayerInventoriesUpdated, m_game.GetMarketDto());
		}

		public async Task EndGame()
		{
			m_logger.Log(LogLevel.Information, "Ending game.");
			await m_connection.InvokeAsync(GameWorkerResponses.GameEnded);
		}

		#endregion

		#region Player Id

		private void ClearPlayerIdMaps()
		{
			m_connectionIdToPlayerIdMap.Clear();
			m_playerIdToConnectionIdMap.Clear();
		}

		private void UpdateConnectionId(string connectionId, string playerId)
		{
			m_playerIdToConnectionIdMap[playerId] = connectionId;
			m_connectionIdToPlayerIdMap.Remove(connectionId);
			m_connectionIdToPlayerIdMap.Add(connectionId, playerId);
		}

		/// <summary>
		/// Adds a new player.
		/// </summary>
		/// <param name="connectionId">The connection id.</param>
		/// <param name="playerId">The player id.</param>
		private void AddNewPlayer(string connectionId, string playerId)
		{
			if (!m_connectionIdToPlayerIdMap.ContainsKey(connectionId))
			{
				m_connectionIdToPlayerIdMap.Add(connectionId, playerId);
			}
			else
			{
				m_logger.Log(LogLevel.Error, "Tried to add duplicate connection ID to dictionary.");
			}

			if (!m_playerIdToConnectionIdMap.ContainsKey(playerId))
			{
				m_playerIdToConnectionIdMap.Add(playerId, connectionId);
			}
			else
			{
				m_logger.Log(LogLevel.Error, "Tried to add duplicate GUID to dictionary.");
			}
		}

		#endregion

		#region Implementation of IGameEventCommunicator

		/// <inheritdoc/>
		public async Task PlayerInventoriesUpdated(PlayerInventoryCollectionDto playerInventoryCollectionDto)
		{
			m_logger.Log(LogLevel.Information, "Player inventories updated.");

			// Transform player ids into connection ids.
			var inventories = new Dictionary<string, PlayerInventoryDto>();
			foreach(KeyValuePair<string, PlayerInventoryDto> kvp in playerInventoryCollectionDto.Inventories)
			{
				inventories.Add(m_playerIdToConnectionIdMap[kvp.Key], kvp.Value);
			}

			var updatedConnectionIdDto = new PlayerInventoryCollectionDto(inventories);
			await m_connection.InvokeAsync(GameWorkerResponses.InventoriesUpdated, updatedConnectionIdDto);
		}

		/// <inheritdoc/>
		public async Task GameMarketChanged(MarketDto marketDto)
		{
			m_logger.Log(LogLevel.Information, "Market updated.");
			await m_connection.InvokeAsync(GameWorkerResponses.MarketUpdated, marketDto);
		}

		/// <inheritdoc/>
		public async Task GameRolled(MarketDto marketDto)
		{
			m_logger.Log(LogLevel.Information, "Rolled.");
			await m_connection.InvokeAsync(GameWorkerResponses.Rolled, marketDto);
		}

		/// <inheritdoc/>
		public async Task GameOver(PlayerInventoryCollectionDto inventoryCollectionDto)
		{
			m_logger.Log(LogLevel.Information, "Game over.");
			ClearPlayerIdMaps();
			m_game = null;
			m_creatorConnectionId = null;
			await m_connection.InvokeAsync(GameWorkerResponses.GameOver, inventoryCollectionDto);
		}

		#endregion
	}
}
