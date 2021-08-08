using Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.DataTransferObjects;
using Models.Game;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StonkTrader.Models.Workers
{
	public class GameWorker : BackgroundService, IGameEventCommunicator
	{
		private readonly ILogger<GameWorker> m_logger;
		// TODO Use these ???
		private readonly string m_gameId;
		private readonly string m_gameToken;
		private StonkTraderGame m_game;
		private string m_creatorConnectionId;
		private HubConnection m_connection;

		public GameWorker(ILogger<GameWorker> logger)
		{
			m_logger = logger;
			m_game = null;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			//m_connection = new HubConnectionBuilder().WithUrl("https://localhost:44378/gamehub").Build();
			//m_connection = new HubConnectionBuilder().WithUrl("http://stonks.com/gamehub").Build();
			m_connection = new HubConnectionBuilder().WithUrl("http://localhost:5000/gamehub").Build();
			m_connection.ServerTimeout = TimeSpan.FromMilliseconds(1800000);

			m_connection.On<GameInitializerDto, string>(GameWorkerRequests.CreateGameRequest, CreateGame);

			m_connection.On<string, string, bool>(GameWorkerRequests.JoinGameRequest, JoinGame);

			m_connection.On<string>(GameWorkerRequests.StartGameRequest, StartGame);

			m_connection.On<string, string, bool, int>(GameWorkerRequests.TransactionRequest, DoTransaction);

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

		private async Task CreateGame(GameInitializerDto parameters, string creatorConnectionId)
		{
			var success = false;
			if (m_game == null)
			{
				m_logger.Log(LogLevel.Information, "Creating game.");
				GameInitializerDto initializer = parameters.IsPrototype ? GetPrototypeGameInitializer() : GetDefaultGameInitializer();
				initializer.MarketOpenTimeInSeconds = parameters.MarketOpenTimeInSeconds;
				initializer.StartingMoney = parameters.StartingMoney;
				initializer.RollsPerRound = parameters.RollsPerRound;
				initializer.NumberOfRounds = parameters.NumberOfRounds;
				initializer.RollTimeInSeconds = parameters.RollTimeInSeconds;
				initializer.TimeBetweenRollsInSeconds = parameters.TimeBetweenRollsInSeconds;

				m_game = new StonkTraderGame(initializer, this);
				m_creatorConnectionId = creatorConnectionId;
				success = true;
			}

			await m_connection.InvokeAsync(GameWorkerResponses.GameCreatedResponse, creatorConnectionId, success);
		}

		private async Task JoinGame(string connectionId, string username, bool isPlayer)
		{
			if (isPlayer)
			{
				m_logger.Log(LogLevel.Information, $"{username} is joining game.");
				PlayerInventoryDto inventory = m_game.AddPlayer(connectionId, username);
				if (inventory == null)
				{
					return;
				}
				await m_connection.InvokeAsync(GameWorkerResponses.PlayerJoinedGameResponse, connectionId, inventory);
				if (m_game.IsStarted)
				{
					// If game is started, update the player with the market
					await m_connection.InvokeAsync(GameWorkerResponses.MarketUpdated, m_game.GetMarketDto());
				}
			}
			else
			{
				m_logger.Log(LogLevel.Information, "Observer is joining game.");
				await m_connection.InvokeAsync(GameWorkerResponses.ObserverJoinedGameResponse, connectionId, m_game.GetMarketDto());
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
			var transactionWasSuccessful = false;
			PlayerInventoryDto inventory = null;
			if (isBuy)
			{
				if (m_game.IsBuyOkay(connectionId, stockName, amount))
				{
					inventory = m_game.BuyStock(connectionId, stockName, amount);
					transactionWasSuccessful = true;
				}
			}
			else
			{
				if (m_game.IsSellOkay(connectionId, stockName, amount))
				{
					inventory = m_game.SellStock(connectionId, stockName, amount);
					transactionWasSuccessful = true;
				}
			}

			await m_connection.InvokeAsync(GameWorkerResponses.TransactionPosted, connectionId, inventory, transactionWasSuccessful);
			await m_connection.InvokeAsync(GameWorkerResponses.PlayerInventoriesUpdated, m_game.GetMarketDto());
		}

		public async Task EndGame()
		{
			m_logger.Log(LogLevel.Information, "Ending game.");
			m_game = null;
			m_creatorConnectionId = null;
			await m_connection.InvokeAsync(GameWorkerResponses.GameEnded);
		}

		#endregion

		#region Implementation of IGameEventCommunicator

		/// <inheritdoc/>
		public async Task PlayerInventoriesUpdated(PlayerInventoryCollectionDto playerInventoryCollectionDto)
		{
			m_logger.Log(LogLevel.Information, "Player inventories updated.");
			await m_connection.InvokeAsync(GameWorkerResponses.InventoriesUpdated, playerInventoryCollectionDto);
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
		public async Task GameOver(GameOverDto gameOverDto)
		{
			m_logger.Log(LogLevel.Information, "Game over.");
			await m_connection.InvokeAsync(GameWorkerResponses.GameOver, gameOverDto);
		}

		#endregion

		#region Game Defaults

		public static GameInitializerDto GetDefaultGameInitializer()
		{
			return new GameInitializerDto()
			{
				MarketOpenTimeInSeconds = 60,
				RollTimeInSeconds = 2,
				TimeBetweenRollsInSeconds = 2,
				NumberOfRounds = 7,
				RollsPerRound = 12,
				StartingMoney = 5000,
				IsPrototype = false,
				Stocks = new[]
				{
					new StockDto("Gold", "#FFD700"),
					new StockDto("Silver", "#C0C0C0"),
					new StockDto("Oil", "#4682B4"),
					new StockDto("Bonds", "#228B22"),
					new StockDto("Industrial", "#DA70D6"),
					new StockDto("Grain", "#F0E68C"),
				}
			};
		}

		public static GameInitializerDto GetPrototypeGameInitializer()
		{
			// Della config
			
			return new GameInitializerDto()
			{
				MarketOpenTimeInSeconds = 90,
				RollTimeInSeconds = 2,
				TimeBetweenRollsInSeconds = 2,
				NumberOfRounds = 7,
				RollsPerRound = 12,
				StartingMoney = 7500,
				IsPrototype = true,
				Stocks = new []
				{
					new StockDto("Dogecoin", "#5cc3f7"),
					new StockDto("Crayola", "#ff33cc"),
					new StockDto("Twitch", "#6441a5"),
					new StockDto("Reddit", "#ff471a"),
					new StockDto("Memes", "#98FB98"),
					new StockDto("YouTube", "#e60000"),
				}
			};
			/*
			return new GameInitializerDto()
			{
				MarketOpenTimeInSeconds = 90,
				RollTimeInSeconds = 2,
				TimeBetweenRollsInSeconds = 2,
				NumberOfRounds = 7,
				RollsPerRound = 12,
				StartingMoney = 7500,
				IsPrototype = true,
				Stocks = new[]
				{
					new StockDto("Tech", "#5cc3f7"),
					new StockDto("Crypto", "#0df20d"),
					//new StockDto("Oil", "#005cb3"),
					new StockDto("Retail", "#800000"),
					new StockDto("Art", "#98FB98"),
					//new StockDto("Industrial", "#8B008B"),

					//new StockDto("Power", "#e61919", true),
					new StockDto("Gold", "#FFD700", true),
					new StockDto("Silver", "#C0C0C0", true),
					new StockDto("Bonds", "#4aad18", true),
					new StockDto("Transport", "#66ffff", true),
					//new StockDto("Grain", "#5cc3f7", true),
				}
			};
			*/
		}

		#endregion
	}
}
