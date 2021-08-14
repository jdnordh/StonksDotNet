using Models.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Models.Game
{
	/// <summary>
	/// The game.
	/// </summary>
	public class StonkTraderGame
	{
		#region Fields

		// Init fields
		private readonly int m_numberOfRounds;
		private readonly int m_numberOfRollsPerRound;
		private readonly int m_startingMoney;
		private readonly int m_marketOpenTimeInSeconds;
		private readonly int m_rollTimeInSeconds;
		private readonly int m_timeBetweenRollsInSeconds;
		private readonly IGameEventCommunicator m_gameEventCommunicator;
		private readonly bool m_isPrototype;

		// Rolling
		private readonly Die<string> m_stockDie;
		private readonly Die<decimal> m_amountDie;
		private readonly Die<Func<string, decimal, StockFunc>> m_funcDie;

		// Timers
		private Timer m_marketTimer;
		private Timer m_rollTimer;
		private Timer m_rollPlayerDelayTimer;
		private StockFunc m_currentStockFunc;

		// Trackers
		private int m_currentRound;
		private int m_currentRoll;

		#endregion

		#region Properties

		public Dictionary<string, Stock> Stocks;

		public bool IsStarted { get; private set; }

		public bool IsMarketOpen { get; private set; }

		public Dictionary<string, Player> Players;

		#endregion

		#region Constructor 

		public StonkTraderGame(GameInitializerDto initializer, IGameEventCommunicator gameEventCommunicator)
		{
			Players = new Dictionary<string, Player>();
			m_numberOfRounds = initializer.NumberOfRounds;
			m_numberOfRollsPerRound = initializer.RollsPerRound;
			m_startingMoney = initializer.StartingMoney;
			m_marketOpenTimeInSeconds = initializer.MarketOpenTimeInSeconds;
			m_rollTimeInSeconds = initializer.RollTimeInSeconds;
			m_timeBetweenRollsInSeconds = initializer.TimeBetweenRollsInSeconds;
			m_gameEventCommunicator = gameEventCommunicator;
			m_isPrototype = initializer.IsPrototype;

			m_currentRound = 0;
			m_currentRoll = 0;

			// Intialize timers
			m_marketTimer = new Timer(m_marketOpenTimeInSeconds * 1000);
			m_marketTimer.Elapsed += MarketTimerElapsed;
			m_marketTimer.AutoReset = false;

			m_rollTimer = new Timer((m_rollTimeInSeconds + m_timeBetweenRollsInSeconds) * 1000);
			m_rollTimer.Elapsed += RollTimerElapsed;
			m_rollTimer.AutoReset = false;

			m_rollPlayerDelayTimer = new Timer(m_rollTimeInSeconds * 1000);
			m_rollPlayerDelayTimer.Elapsed += RollPlayerDelayTimerElapsed;
			m_rollPlayerDelayTimer.AutoReset = false;

			m_stockDie = new Die<string>() { Results = initializer.Stocks.Select(stock => stock.Name).ToList() };
			m_amountDie = new Die<decimal>
			{
				Results = new List<decimal>
				{
					// TODO CHange this
					//0.05M,
					.1M,
					.2M,
					.3M,
				}
			};
			m_funcDie = new Die<Func<string, decimal, StockFunc>>
			{
				Results = new List<Func<string, decimal, StockFunc>>
				{
					(stock, percentAmount) =>
					{
						return new StockFunc(StockFuncType.Up, stock, percentAmount);
					},
					(stock, percentAmount) =>
					{
						return new StockFunc(StockFuncType.Down, stock, percentAmount);
					},
					(stock, percentAmount) =>
					{
						return new StockFunc(StockFuncType.Dividend, stock, percentAmount);
					}
				}
			};

			Stocks = new Dictionary<string, Stock>();
			foreach (StockDto stockDto in initializer.Stocks)
			{
				Stocks.Add(stockDto.Name, new Stock(stockDto));
			}
			IsMarketOpen = false;
			IsStarted = false;
		}

		private async void RollTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			await Roll();
		}

		private async void MarketTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			await CloseMarket();
		}

		private async void RollPlayerDelayTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			// Do roll action after the presenter has shown it so clients don't get the update before it's on screen.
			Func<Task> rollMethod = null;
			switch (m_currentStockFunc.Type)
			{
				case StockFuncType.Up:
				{
					Stocks[m_currentStockFunc.StockName].IncreaseValue(m_currentStockFunc.PercentageAmount);
					rollMethod = ResolveSplitOrCrash;
					break;
				}
				case StockFuncType.Down:
				{
					Stocks[m_currentStockFunc.StockName].DecreaseValue(m_currentStockFunc.PercentageAmount);
					rollMethod = ResolveSplitOrCrash;
					break;
				}
				case StockFuncType.Dividend:
				{
					rollMethod = () => {
						return PayDividends(m_currentStockFunc.StockName, m_currentStockFunc.PercentageAmount);
					};
					break;
				}
			}
			await rollMethod();
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Add a player to the game.
		/// </summary>
		/// <param name="connectionId">The connection id.</param>
		/// <param name="username">The username.</param>
		/// <returns>The player's inventory.</returns>
		public PlayerInventoryDto AddPlayer(string connectionId, string username)
		{
			if (Players.ContainsKey(connectionId))
			{
				return null;
			}
			var playerId = GetNewPlayerId();
			var player = new Player(playerId, connectionId, username, Stocks.Values.Select(stock => stock.Name).ToList())
			{
				Money = m_startingMoney
			};
			Players.Add(playerId, player);
			PlayerInventoryDto inventory = player.GetPlayerInvetory();
			return inventory;
		}

		/// <summary>
		/// Starts the game.
		/// </summary>
		/// <returns></returns>
		public async Task StartGame()
		{
			IsStarted = true;
			await OpenMarket();
		}

		#endregion

		#region Market

		private async Task OpenMarket()
		{
			if (IsMarketOpen)
			{
				return;
			}
			if (m_currentRound == m_numberOfRounds)
			{
				// Game is over
				await EndGame();
				return;
			}
			++m_currentRound;
			IsMarketOpen = true;

			m_marketTimer.Start();

			var marketMiliseconds = m_marketOpenTimeInSeconds * 1000;
			var marketEndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + marketMiliseconds;
			MarketDto marketDto = GetMarketDto();
			marketDto.CurrentRound = m_currentRound;
			marketDto.TotalRounds = m_numberOfRounds;
			marketDto.MarketCloseTimeInMilliseconds = marketEndTime;
			await m_gameEventCommunicator.GameMarketChanged(marketDto);
		}

		private async Task CloseMarket()
		{
			IsMarketOpen = false;
			await m_gameEventCommunicator.GameMarketChanged(GetMarketDto());
			await Roll();
		}

		#endregion

		#region Dice Rolling

		/// <summary>
		/// Roll the dice.
		/// </summary>
		private async Task Roll()
		{
			if (m_currentRoll == m_numberOfRollsPerRound)
			{
				m_currentRoll = 0;
				await OpenMarket();
			}
			else
			{
				// Do rolling
				++m_currentRoll;
				m_currentStockFunc = GetRollFunc();
				MarketDto marketDto = GetMarketDto();

				marketDto.RollDto = new RollDto(m_currentStockFunc.StockName, m_currentStockFunc.Type.ToString(),
					(int)(m_currentStockFunc.PercentageAmount * 100), m_rollTimeInSeconds);

				// Show roll on presenter
				await m_gameEventCommunicator.GameRolled(marketDto);

				// Diaplay results of roll to players after the presenter shows the roll
				m_rollPlayerDelayTimer.Start();

				// Roll again after set amount of time
				m_rollTimer.Start();
			}
		}

		private StockFunc GetRollFunc()
		{
			var stockResult = m_stockDie.Roll();
			var amountResult = m_amountDie.Roll();
			if (m_isPrototype)
			{
				if (Stocks[stockResult].IsHalved && amountResult == 0.2M)
				{
					amountResult = 0.05M;
				}
			}
			Func<string, decimal, StockFunc> stockFuncResult = m_funcDie.Roll();
			return stockFuncResult(stockResult, amountResult);
		}

		/// <summary>
		/// Pay divideds to the holders of given stock based on the given percentage.
		/// </summary>
		/// <param name="stock">The stock paying dividends.</param>
		/// <param name="percentage">The percentage to pay out.</param>
		private async Task PayDividends(string stock, decimal percentage)
		{
			if (!Stocks[stock].IsPayingDividends())
			{
				return;
			}
			var updatedPlayerInvectories = new List<(string id, PlayerInventoryDto)>();
			foreach (Player player in Players.Values)
			{
				var holdings = player.Holdings[stock];
				if (holdings > 0)
				{
					player.Money += (int)(holdings * percentage);
					updatedPlayerInvectories.Add((player.ConnectionId, player.GetPlayerInvetory()));
				}
			}
			PlayerInventoryCollectionDto inventoryDto = GetInventoryCollectionDto();
			await m_gameEventCommunicator.PlayerInventoriesUpdated(inventoryDto);
		}

		/// <summary>
		/// Resolves any stocks that have split or crashed.
		/// </summary>
		private async Task ResolveSplitOrCrash()
		{
			foreach (Stock stock in Stocks.Values)
			{
				// If stock crashes, remove all shares
				if (stock.Value <= 0)
				{
					foreach (Player player in Players.Values)
					{
						player.Holdings[stock.Name] = 0;
					}
					stock.ResetValue();
				}
				// If stock splits, double all shares
				else if (stock.Value >= 2)
				{
					foreach (Player player in Players.Values)
					{
						player.Holdings[stock.Name] = player.Holdings[stock.Name] * 2;
					}
					stock.ResetValue();
				}
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(GetInventoryCollectionDto());
			await m_gameEventCommunicator.GameMarketChanged(GetMarketDto());
		}

		#endregion

		#region Buy and Sell

		/// <summary>
		/// Determine if the given player can perform a proposed buy operation.
		/// </summary>
		/// <param name="playerId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToBuy">The amount of stock to buy.</param>
		/// <returns>True if the operation is okay.</returns>
		public bool IsBuyOkay(string playerId, string stockName, int amountToBuy)
		{
			if (!IsStarted)
			{
				return false;
			}
			if (!Players.ContainsKey(playerId))
			{
				throw new InvalidOperationException($"The user '{playerId}' was not present in the game.");
			}
			if (!Stocks.ContainsKey(stockName))
			{
				return false;
			}
			if (amountToBuy % 500 != 0)
			{
				return false;
			}
			Player player = Players[playerId];
			var cost = Stocks[stockName].GetValueOfAmount(amountToBuy);
			return player.Money >= cost;
		}

		/// <summary>
		/// Buy an amount of stock.
		/// </summary>
		/// <param name="playerId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToBuy">The amount of stock to buy.</param>
		/// <returns>Updated player money.</returns>
		public PlayerInventoryDto BuyStock(string playerId, string stockName, int amountToBuy)
		{
			Player player = Players[playerId];
			if (!IsBuyOkay(playerId, stockName, amountToBuy))
			{
				return player.GetPlayerInvetory();
			}
			var cost = Stocks[stockName].GetValueOfAmount(amountToBuy);
			player.Money -= cost;
			player.Holdings[stockName] += amountToBuy;
			return player.GetPlayerInvetory();
		}

		/// <summary>
		/// Determine if the given player can perform a proposed sell operation.
		/// </summary>
		/// <param name="playerId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToSell">The amount of stock to buy.</param>
		/// <returns>True if the operation is okay.</returns>
		public bool IsSellOkay(string playerId, string stockName, int amountToSell)
		{
			if (!IsStarted)
			{
				return false;
			}
			if (!Players.ContainsKey(playerId))
			{
				throw new InvalidOperationException($"The user '{playerId}' was not present in the game.");
			}
			if (!Stocks.ContainsKey(stockName))
			{
				return false;
			}
			if (amountToSell % 500 != 0)
			{
				return false;
			}
			Player player = Players[playerId];
			return player.Holdings[stockName] >= amountToSell;
		}

		/// <summary>
		/// Sell an amount of stock.
		/// </summary>
		/// <param name="playerId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToSell">The amount of stock to buy.</param>
		/// <returns>Updated player money.</returns>
		public PlayerInventoryDto SellStock(string playerId, string stockName, int amountToSell)
		{
			Player player = Players[playerId];
			if (!IsSellOkay(playerId, stockName, amountToSell))
			{
				return player.GetPlayerInvetory();
			}
			var soldFor = Stocks[stockName].GetValueOfAmount(amountToSell);
			player.Holdings[stockName] -= amountToSell;
			player.Money += soldFor;
			return player.GetPlayerInvetory();
		}

		#endregion

		#region Game End

		private async Task EndGame()
		{
			// Send inventory update to observer with inventory breakdowns
			await m_gameEventCommunicator.GameOver(GetInventoryCollectionDto());
			SellAllShares();

			// Send update to players with money amount
			await m_gameEventCommunicator.PlayerInventoriesUpdated(GetInventoryCollectionDto());
			IsStarted = false;
		}

		/// <summary>
		/// Convert all player holdings to money.
		/// </summary>
		private void SellAllShares()
		{
			foreach (Player player in Players.Values)
			{
				foreach (KeyValuePair<string, int> holding in player.Holdings)
				{
					player.Money += Stocks[holding.Key].GetValueOfAmount(holding.Value);
				}
				player.ClearAllShares();
			}
		}

		#endregion

		#region Utilities

		/// <summary>
		/// Get a market dto.
		/// </summary>
		/// <returns>The dto.</returns>
		public MarketDto GetMarketDto()
		{
			var stocksDto = new Dictionary<string, StockDto>();
			foreach (KeyValuePair<string, Stock> kvp in Stocks)
			{
				stocksDto.Add(kvp.Key, kvp.Value.ToStockDto());
			}
			var marketDto = new MarketDto(IsMarketOpen, stocksDto)
			{
				PlayerInventories = GetInventoryCollectionDto()
			};
			return marketDto;
		}

		/// <summary>
		/// Get the inventory dto.
		/// </summary>
		/// <returns>The inventory dto.</returns>
		private PlayerInventoryCollectionDto GetInventoryCollectionDto()
		{
			var inventories = new Dictionary<string, PlayerInventoryDto>();

			foreach (KeyValuePair<string, Player> kvp in Players)
			{
				inventories.Add(kvp.Value.ConnectionId, kvp.Value.GetPlayerInvetory());
			}

			return new PlayerInventoryCollectionDto(inventories);
		}

		/// <summary>
		/// Get a new player id as a string.
		/// </summary>
		/// <returns>The id as a string.</returns>
		private string GetNewPlayerId()
		{
			return Guid.NewGuid().ToString();
		}

		#endregion
	}
}
