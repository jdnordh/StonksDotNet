using Models.DataTransferObjects;
using StonkTrader.Models.Game.Characters;
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
		#region Constants

		const int SecondsBetweenRolls = 2;
		const int RollTimeInSeconds = 2;

		#endregion

		#region Fields

		// Init fields
		private readonly int m_numberOfRounds;
		private readonly int m_numberOfRollsPerRound;
		private readonly int m_startingMoney;
		private readonly int m_marketOpenTimeInSeconds;
		private readonly int m_rollTimeInSeconds;
		private readonly int m_timeBetweenRollsInSeconds;
		private readonly IGameEventCommunicator m_gameEventCommunicator;

		// Rolling
		private List<List<Roll>> m_rolls;
		private Roll m_currentRoll;

		// Timers
		private Timer m_marketTimer;
		private Timer m_rollTimer;
		private Timer m_rollPlayerDelayTimer;

		// Trackers
		private int m_currentRoundNumber;
		private int m_currentRollNumber;

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
			m_rollTimeInSeconds = RollTimeInSeconds;
			m_timeBetweenRollsInSeconds = SecondsBetweenRolls;
			m_gameEventCommunicator = gameEventCommunicator;

			m_currentRoundNumber = 0;
			m_currentRollNumber = 0;

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

			//m_afterMarketDelayTimer = new Timer(m_timeBetweenRollsInSeconds * 1000);
			//m_afterMarketDelayTimer.Elapsed += RollTimerElapsed; 
			//m_afterMarketDelayTimer.AutoReset = false;

			GenerateRolls(initializer);

			Stocks = new Dictionary<string, Stock>();
			foreach (StockDto stockDto in initializer.Stocks)
			{
				Stocks.Add(stockDto.Name, new Stock(stockDto));
			}
			IsMarketOpen = false;
			IsStarted = false;
		}

		#region Event Handlers

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
			switch (m_currentRoll.Type)
			{
				case RollType.Up:
				{
					Stocks[m_currentRoll.StockName].IncreaseValue(m_currentRoll.PercentageAmount);
					rollMethod = ResolveSplitOrCrash;
					break;
				}
				case RollType.Down:
				{
					Stocks[m_currentRoll.StockName].DecreaseValue(m_currentRoll.PercentageAmount);
					rollMethod = ResolveSplitOrCrash;
					break;
				}
				case RollType.Dividend:
				{
					rollMethod = () => {
						return PayDividends(m_currentRoll.StockName, m_currentRoll.PercentageAmount);
					};
					break;
				}
			}
			await rollMethod();
		}

		#endregion

		#endregion

		#region Initialization

		private void GenerateRolls(GameInitializerDto initializer)
		{
			var stockDie = new Die<string>() { Results = initializer.Stocks.Select(stock => stock.Name).ToList() };
			var amountDie = new Die<decimal>
			{
				Results = new List<decimal>
				{
					.1M,
					.2M,
					.3M,
				}
			};
			var rollDie = new Die<Func<string, decimal, Roll>>
			{
				Results = new List<Func<string, decimal, Roll>>
				{
					(stock, percentAmount) =>
					{
						return new Roll(RollType.Up, stock, percentAmount);
					},
					(stock, percentAmount) =>
					{
						return new Roll(RollType.Down, stock, percentAmount);
					},
					(stock, percentAmount) =>
					{
						return new Roll(RollType.Dividend, stock, percentAmount);
					}
				}
			};

			m_rolls = new List<List<Roll>>();
			for (int round = 0; round < m_numberOfRounds; round++)
			{
				m_rolls.Add(new List<Roll>());
				for (int rollNum = 0; rollNum < m_numberOfRollsPerRound; rollNum++)
				{
					var stockName = stockDie.Roll();
					var amount = amountDie.Roll();
					var roll = rollDie.Roll();
					var rollResult = roll(stockName, amount);

					m_rolls[round].Add(rollResult);
				}
			}
		}

		/// <summary>
		/// Add a player to the game.
		/// </summary>
		/// <param name="connectionId">The connection id.</param>
		/// <param name="username">The username.</param>
		/// <param name="characterId">The character id.</param>
		/// <returns>The player's inventory.</returns>
		public PlayerInventoryDto AddPlayer(string connectionId, string username, int characterId)
		{
			if (Players.ContainsKey(connectionId))
			{
				return null;
			}
			var playerId = GetNewPlayerId();
			List<string> stockNames = GetStockNames();
			var player = new Player(playerId, connectionId, username, m_startingMoney, stockNames, 
				CharacterProvider.GetCharacterForId(characterId, stockNames));

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
			if (m_currentRoundNumber == m_numberOfRounds)
			{
				// Game is over
				await EndGame();
				return;
			}
			++m_currentRoundNumber;
			IsMarketOpen = true;

			m_marketTimer.Start();

			var marketMiliseconds = m_marketOpenTimeInSeconds * 1000;
			var marketEndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + marketMiliseconds;
			MarketDto marketDto = GetMarketDto();
			marketDto.MarketCloseTimeInMilliseconds = marketEndTime;
			await m_gameEventCommunicator.GameMarketChanged(marketDto);
		}

		private async Task CloseMarket()
		{
			IsMarketOpen = false;
			await m_gameEventCommunicator.GameMarketChanged(GetMarketDto());

			m_rollTimer.Start();
		}

		#endregion

		#region Dice Rolling

		/// <summary>
		/// Roll the dice.
		/// </summary>
		private async Task Roll()
		{
			if (m_currentRollNumber == m_numberOfRollsPerRound)
			{
				m_currentRollNumber = 0;
				await OpenMarket();
			}
			else
			{
				// Do rolling
				m_currentRoll = m_rolls[m_currentRoundNumber][m_currentRollNumber++];

				MarketDto marketDto = GetMarketDto();

				// Show roll on presenter
				await m_gameEventCommunicator.GameRolled(marketDto);

				// Diaplay results of roll to players after the presenter shows the roll
				m_rollPlayerDelayTimer.Start();

				// Roll again after set amount of time
				m_rollTimer.Start();
			}
		}

		/// <summary>
		/// Previews the first roll of a round if the player has access to it.
		/// </summary>
		/// <param name="playerId">The player id.</param>
		/// <returns>The roll dto, or null if not allowed.</returns>
		public RollDto PreviewFirstRoll(string playerId)
		{
			if (Players[playerId].Character.GetsFirstRollReveal && IsMarketOpen)
			{
				Roll roll = m_rolls[m_currentRoundNumber][0];
				return RollToRollDto(roll);
			}
			return null;
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
					decimal specificPercentage = player.Character.GetDivedendAmount(Stocks[stock].Value, percentage);
					player.Money += (int)(holdings * specificPercentage);
					updatedPlayerInvectories.Add((player.ConnectionId, player.GetPlayerInvetory()));
				}
			}
			PlayerInventoryCollectionDto inventoryCollectionDto = GetInventoryCollectionDto();
			await m_gameEventCommunicator.PlayerInventoriesUpdated(inventoryCollectionDto);
		}

		/// <summary>
		/// Resolves any stocks that have split or crashed.
		/// </summary>
		private async Task ResolveSplitOrCrash()
		{
			bool splitOrCrashed = false;
			foreach (Stock stock in Stocks.Values)
			{
				// If stock crashes, remove all shares
				if (stock.Value <= 0)
				{
					// Reset stock holdings of crashed stock
					int totalSharesLost = 0;
					foreach (Player player in Players.Values)
					{
						totalSharesLost += player.Holdings[stock.Name];
						player.Holdings[stock.Name] = 0;
					}

					// Payout rebates
					if (totalSharesLost > 0)
					{
						foreach (Player player in Players.Values)
						{
							int crashRebate = player.Character.CalculateCrashRebateAmount(totalSharesLost);
							player.Money += crashRebate;
						}
					}

					splitOrCrashed = true;
					stock.ResetValue();
				}
				// If stock splits, double all shares
				else if (stock.Value >= 2)
				{
					foreach (Player player in Players.Values)
					{
						player.Holdings[stock.Name] = player.Holdings[stock.Name] * 2;
					}
					splitOrCrashed = true;
					stock.ResetValue();
				}
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(GetInventoryCollectionDto());

			if (splitOrCrashed)
			{
				// Wait to show that stock has crash or split
				await Task.Delay(m_timeBetweenRollsInSeconds * 1000);
			}

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
			var preSellInventories = GetInventoryCollectionDto();
			SellAllShares();

			// Send update to players with money amount
			await m_gameEventCommunicator.PlayerInventoriesUpdated(GetInventoryCollectionDto());

			// Send inventory update to observer with inventory breakdowns
			await m_gameEventCommunicator.GameOver(preSellInventories);
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
			marketDto.CurrentRound = m_currentRoundNumber;
			marketDto.TotalRounds = m_numberOfRounds;
			if (m_currentRoll != null)
			{
				marketDto.RollDto = RollToRollDto(m_currentRoll);
			}
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
				inventories.Add(kvp.Key, kvp.Value.GetPlayerInvetory());
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

		/// <summary>
		/// Get the stock names in a list.
		/// </summary>
		/// <returns></returns>
		private List<string> GetStockNames()
		{
			return Stocks.Values.Select(stock => stock.Name).ToList();
		}

		/// <summary>
		/// Gets a <see cref="RollDto"/> from a <see cref="Roll"/>.
		/// </summary>
		/// <param name="roll">The roll to convert.</param>
		/// <returns>A roll dto.</returns>
		private RollDto RollToRollDto(Roll roll)
		{
			return new RollDto(roll.StockName, roll.Type.ToString(), (int)(roll.PercentageAmount * 100), m_rollTimeInSeconds);
		}

		#endregion
	}
}
