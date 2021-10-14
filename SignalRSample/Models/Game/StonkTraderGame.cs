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
		private static readonly List<decimal> s_amountDiceValues = new List<decimal>() { 0.1M, 0.15M, 0.2M, 0.3M };

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
		private readonly Dictionary<string, Stock> m_stocks;

		// Rolling
		private List<List<Roll>> m_rolls;
		private Roll m_currentRoll;

		// Timers
		private readonly Timer m_marketTimer;
		private readonly Timer m_marketHalfTimer;
		private readonly Timer m_rollTimer;
		private readonly Timer m_rollPlayerDelayTimer;

		/// <summary>
		/// The current round number that starts at 0 for the first round.
		/// </summary>
		private int m_currentRoundNumber;
		private int m_currentRollNumber;
		private long m_currentMarketEndTime;

		private readonly Dictionary<string, TrendDto> m_roundTrendIndexedByPlayer;
		private readonly Dictionary<string, string> m_pushDownVotesIndexedByPlayer;

		#endregion

		#region Properties

		public bool IsStarted { get; private set; }

		public bool IsMarketOpen { get; private set; }

		public bool IsMarketHalfTime { get; private set; }

		public Dictionary<string, Player> Players;

		private bool ShouldDoHalfTimeMarket
		{
			get => Players.Values.Where(p => p.Character.GetsHalfTimeTransaction).Any();
		}

		private bool ShouldCheckPredictions
		{
			get => Players.Values.Where(p => p.Character.GetsPrediction).Any();
		}

		private bool ShouldPushDownStock
		{
			get => Players.Values.Where(p => p.Character.GetsPushDownVote).Any();
		}

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

			m_currentRoundNumber = -1;
			m_currentRollNumber = 0;

			// Intialize timers
			m_marketTimer = new Timer(m_marketOpenTimeInSeconds * 1000);
			m_marketTimer.Elapsed += MarketTimerElapsed;
			m_marketTimer.AutoReset = false;
			m_marketHalfTimer = new Timer(m_marketOpenTimeInSeconds * 500);
			m_marketHalfTimer.Elapsed += MarketTimerElapsed;
			m_marketHalfTimer.AutoReset = false;

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

			m_roundTrendIndexedByPlayer = new Dictionary<string, TrendDto>();
			m_pushDownVotesIndexedByPlayer = new Dictionary<string, string>();

			m_stocks = new Dictionary<string, Stock>();
			foreach (StockDto stockDto in initializer.Stocks)
			{
				m_stocks.Add(stockDto.Name, new Stock(stockDto));
			}
			IsMarketOpen = false;
			IsStarted = false;
		}
		
		#endregion

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
					m_stocks[m_currentRoll.StockName].IncreaseValue(m_currentRoll.PercentageAmount);
					rollMethod = ResolveSplitOrCrash;
					break;
				}
				case RollType.Down:
				{
					m_stocks[m_currentRoll.StockName].DecreaseValue(m_currentRoll.PercentageAmount);
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

		#region Initialization

		private void GenerateRolls(GameInitializerDto initializer)
		{
			var stockDie = new Die<string>() { Results = initializer.Stocks.Select(stock => stock.Name).ToList() };
			var amountDie = new Die<decimal>
			{
				Results = s_amountDiceValues
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
		/// <param name="username">The username.</param>
		/// <param name="characterId">The character id.</param>
		/// <returns>The player's inventory.</returns>
		public PlayerInventoryDto AddPlayer(string username, int characterId)
		{
			var playerId = GetNewPlayerId();
			List<string> stockNames = GetStockNames();
			var player = new Player(playerId, username, m_startingMoney, stockNames, 
				CharacterProvider.GetCharacterForId(characterId));

			if (IsStarted && IsMarketOpen)
			{
				player.Character.InitializeStocks(GetStockNames());
				player.Character.PrepareForOpenMarket(m_stocks);
			}

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
			if (m_currentRoundNumber + 1 == m_numberOfRounds && !IsMarketHalfTime)
			{
				// Game is over
				await EndGame();
				return;
			}
			IsMarketOpen = true;

			if (IsMarketHalfTime)
			{
				SetupHalfTimeRoundTrends();
				m_marketHalfTimer.Start();
			}
			else
			{
				if (ShouldCheckPredictions)
				{
					await CheckPredictions();
				}
				++m_currentRoundNumber;
				m_marketTimer.Start();
			}
			await UpdatePlayerCharacters();

			int timeMultiplier = IsMarketHalfTime ? 500 : 1000;
			var marketMiliseconds = m_marketOpenTimeInSeconds * timeMultiplier;
			m_currentMarketEndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + marketMiliseconds;
			MarketDto marketDto = GetMarketDto();
			await m_gameEventCommunicator.GameMarketChanged(marketDto);
		}

		private async Task CloseMarket()
		{
			IsMarketOpen = false;

			await PayRebates();
			await m_gameEventCommunicator.GameMarketChanged(GetMarketDto());
			if(ShouldPushDownStock)
			{
				PushDownStock();
			}
			await UpdatePlayerCharacters();

			m_rollTimer.Start();
		}

		private async Task UpdatePlayerCharacters()
		{
			foreach(Player player in Players.Values)
			{
				if(IsMarketOpen)
				{
					if(!player.Character.AreStocksInitialized)
					{
						player.Character.InitializeStocks(GetStockNames());
					}
					player.Character.PrepareForOpenMarket(m_stocks);

					if(!IsMarketHalfTime)
					{
						player.Character.ResetPrediction();
					}
				}
				else
				{
					player.Character.PrepareForClosedMarket();
				}
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(GetInventoryCollectionDto());
		}

		#endregion

		#region Character Abilities

		/// <summary>
		/// Previews the first roll of a round if the player has access to it.
		/// </summary>
		/// <param name="playerId">The player id.</param>
		/// <returns>The roll dto, or null if not allowed.</returns>
		public RollDto PreviewFirstRoll(string playerId)
		{
			if(Players[playerId].Character.GetsFirstRollReveal && IsMarketOpen && !IsMarketHalfTime)
			{
				Roll roll = m_rolls[m_currentRoundNumber][0];
				return RollToRollDto(roll);
			}
			return null;
		}

		private async Task PayRebates()
		{
			foreach(var player in Players.Values)
			{
				player.Money += player.Character.CalculateMarketRebateAmount();
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(GetInventoryCollectionDto());
		}

		/// <summary>
		/// Sets the prediction for a player.
		/// </summary>
		/// <param name="playerId">The player id.</param>
		/// <param name="prediction">The prediction.</param>
		public void MakePrediction(string playerId, PredictionDto prediction)
		{
			if(Players[playerId].Character.GetsPrediction && IsMarketOpen && !IsMarketHalfTime)
			{
				Players[playerId].Character.Prediction = prediction;
			}
		}

		private async Task CheckPredictions()
		{
			if(m_currentRoundNumber < 0)
			{
				return;
			}

			var playersWithPredictions = Players.Values.Where(p => p.Character.Prediction != null).ToList();
			if (playersWithPredictions.Count == 0)
			{
				return;
			}

			// Dictionary keyed by stock name with value true if the stock value went up.
			var marketCopy = m_stocks.ToDictionary(kvp => kvp.Key, kvp => new Stock(kvp.Key));
			var rolls = m_rolls[m_currentRoundNumber];
			for(int i = 0; i < rolls.Count; i++)
			{
				Roll roll = rolls[i];
				switch(roll.Type)
				{
					case RollType.Up:
					{
						marketCopy[roll.StockName].IncreaseValue(roll.PercentageAmount);
						break;
					}
					case RollType.Down:
					{
						marketCopy[roll.StockName].DecreaseValue(roll.PercentageAmount);
						break;
					}
				}
			}

			var correctPredictions = new Dictionary<string, bool>();
			foreach(var kvp in marketCopy)
			{
				if(kvp.Value.Value == 1M)
				{
					continue;
				}
				correctPredictions.Add(kvp.Key, kvp.Value.Value > 1M);
			}

			foreach(var player in playersWithPredictions)
			{
				var prediction = player.Character.Prediction;
				if(correctPredictions.ContainsKey(prediction.StockName) && correctPredictions[prediction.StockName] == prediction.IsUp)
				{
					player.Character.PredictionWasCorrect();
				}
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(GetInventoryCollectionDto());
		}

		/// <summary>
		/// Previews the trend of a round if the player has access to it.
		/// </summary>
		/// <param name="playerId">The player id.</param>
		/// <returns>The roll dto, or null if not allowed.</returns>
		public TrendDto PreviewRoundTrend(string playerId)
		{
			if(Players[playerId].Character.GetsHalfTimeTransaction && IsMarketOpen && IsMarketHalfTime)
			{
				return m_roundTrendIndexedByPlayer.TryGetValue(playerId, out var trendDto) ? trendDto : m_roundTrendIndexedByPlayer.First().Value;
			}
			return null;
		}

		private void SetupHalfTimeRoundTrends()
		{
			var marketCopy = m_stocks.ToDictionary(kvp => kvp.Key, kvp => new Stock(kvp.Key));
			for(int i = m_currentRollNumber; i < m_numberOfRollsPerRound; i++)
			{
				Roll roll = m_rolls[m_currentRoundNumber][i];
				switch(roll.Type)
				{
					case RollType.Up:
					{
						marketCopy[roll.StockName].IncreaseValue(roll.PercentageAmount);
						break;
					}
					case RollType.Down:
					{
						marketCopy[roll.StockName].DecreaseValue(roll.PercentageAmount);
						break;
					}
				}
			}
			var trendData = new List<TrendDto>();
			foreach(var kvp in marketCopy)
			{
				if(kvp.Value.Value == 1M)
				{
					continue;
				}
				trendData.Add(new TrendDto(kvp.Key, kvp.Value.Value > 1M ? "Up" : "Down"));
			}
			if(trendData.Count == 0)
			{
				trendData.Add(new TrendDto("No Information", null, true));
			}

			m_roundTrendIndexedByPlayer.Clear();
			var rand = new Random();
			foreach(var playerKvp in Players)
			{
				m_roundTrendIndexedByPlayer.Add(playerKvp.Key, trendData[rand.Next(0, trendData.Count)]);
			}
		}

		/// <summary>
		/// Request a stock push down.
		/// </summary>
		/// <param name="playerId">The player id.</param>
		/// <param name="stockName">The stock to push down.</param>
		public void RequestStockPushDown(string playerId, string stockName)
		{
			if(Players[playerId].Character.GetsPushDownVote && IsMarketOpen && !IsMarketHalfTime)
			{
				if(m_pushDownVotesIndexedByPlayer.ContainsKey(playerId))
				{
					m_pushDownVotesIndexedByPlayer[playerId] = stockName;
				}
				else
				{
					m_pushDownVotesIndexedByPlayer.Add(playerId, stockName);
				}
			}
		}

		private void PushDownStock()
		{
			if(m_pushDownVotesIndexedByPlayer.Count == 0)
			{
				// No votes, so don't push down
				return;
			}

			var rand = new Random();
			string stockNameToPushDown;
			var votes = new Dictionary<string, int>();
			foreach(var stock in GetStockNames())
			{
				votes.Add(stock, 0);
			}
			foreach(var voteKvp in m_pushDownVotesIndexedByPlayer)
			{
				votes[voteKvp.Value]++;
			}
			int maxVotes = votes.Values.Max();
			var stocksToPushDown = votes.Where(kvp => kvp.Value == maxVotes).Select(kvp => kvp.Key).ToList();
			if(stocksToPushDown.Count == 1)
			{
				stockNameToPushDown = stocksToPushDown[0];
			}
			else if(stocksToPushDown.Count > 1)
			{
				// Tied, pick a random one out of the tied stocks
				int coinFlipWinner = rand.Next(0, stocksToPushDown.Count);
				stockNameToPushDown = stocksToPushDown[coinFlipWinner];
			}
			else
			{
				// No votes cast
				return;
			}

			double result = rand.NextDouble();
			double percentageVote = maxVotes / Players.Count;
			result += percentageVote;
			decimal percentageDown;
			if(result < 0.4)
			{
				percentageDown = 0.2M;
			}
			else if(result < 0.6)
			{
				percentageDown = 0.25M;
			}
			else if(result < 1)
			{
				percentageDown = 0.3M;
			}
			else if(result < 1.5)
			{
				percentageDown = 0.35M;
			}
			else
			{
				percentageDown = 0.4M;
			}
			var roll = new Roll(RollType.Down, stockNameToPushDown, percentageDown);
			m_rolls[m_currentRoundNumber].Insert(1, roll);
		}

		#endregion

		#region Dice Rolling

		/// <summary>
		/// Roll the dice.
		/// </summary>
		private async Task Roll()
		{
			if (m_currentRollNumber == m_rolls[m_currentRoundNumber].Count)
			{
				m_currentRollNumber = 0;
				await OpenMarket();
			}
			else if (m_currentRollNumber == Math.Ceiling((decimal)m_numberOfRollsPerRound / 2) && ShouldDoHalfTimeMarket && !IsMarketHalfTime)
			{
				IsMarketHalfTime = true;
				await OpenMarket();
			}
			else
			{
				IsMarketHalfTime = false;

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
		/// Pay divideds to the holders of given stock based on the given percentage.
		/// </summary>
		/// <param name="stock">The stock paying dividends.</param>
		/// <param name="percentage">The percentage to pay out.</param>
		private async Task PayDividends(string stock, decimal percentage)
		{
			if (!m_stocks[stock].IsPayingDividends())
			{
				return;
			}
			foreach (Player player in Players.Values)
			{
				var holdings = player.Holdings[stock];
				if (holdings > 0)
				{
					decimal specificPercentage = player.Character.GetDivedendAmount(m_stocks[stock].Value, percentage);
					player.Money += (int)(holdings * specificPercentage);
				}
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(GetInventoryCollectionDto());
		}

		/// <summary>
		/// Resolves any stocks that have split or crashed.
		/// </summary>
		private async Task ResolveSplitOrCrash()
		{
			bool splitOrCrashed = false;
			foreach (Stock stock in m_stocks.Values)
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
		/// Check if a transaction is valid.
		/// </summary>
		/// <param name="playerId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amount">The amount of stock to buy.</param>
		/// <returns>True if the transaction is valid.</returns>
		private bool IsTransactionValid(string playerId, string stockName, int amount)
		{
			if (!IsStarted)
			{
				return false;
			}
			if (!IsMarketOpen)
			{
				return false;
			}
			if (!Players.TryGetValue(playerId, out Player player))
			{
				// TODO Verify this needs an exception throw
				throw new InvalidOperationException($"The user '{playerId}' was not present in the game.");
			}
			if (IsMarketHalfTime && !player.Character.GetsHalfTimeTransaction)
			{
				return false;
			}
			if (!m_stocks.ContainsKey(stockName))
			{
				return false;
			}
			if (amount % 500 != 0)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Determine if the given player can perform a proposed buy operation.
		/// </summary>
		/// <param name="playerId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToBuy">The amount of stock to buy.</param>
		/// <returns>True if the operation is okay.</returns>
		public bool IsBuyOkay(string playerId, string stockName, int amountToBuy)
		{
			if (!IsTransactionValid(playerId, stockName, amountToBuy))
			{
				return false;
			}
			Player player = Players[playerId];
			var cost = m_stocks[stockName].GetValueOfAmount(amountToBuy);
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
			var cost = m_stocks[stockName].GetValueOfAmount(amountToBuy);
			player.Money -= cost;
			player.Holdings[stockName] += amountToBuy;
			player.Character.RecordTransaction(new PlayerTransactionDto(true, amountToBuy, stockName));

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
			if (!IsTransactionValid(playerId, stockName, amountToSell))
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
			var soldFor = m_stocks[stockName].GetValueOfAmount(amountToSell);
			player.Holdings[stockName] -= amountToSell;
			player.Money += soldFor;
			player.Character.RecordTransaction(new PlayerTransactionDto(false, amountToSell, stockName));

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
					player.Money += m_stocks[holding.Key].GetValueOfAmount(holding.Value);
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
			foreach (KeyValuePair<string, Stock> kvp in m_stocks)
			{
				stocksDto.Add(kvp.Key, kvp.Value.ToStockDto());
			}
			var marketDto = new MarketDto(IsMarketOpen, stocksDto)
			{
				PlayerInventories = GetInventoryCollectionDto()
			};
			marketDto.CurrentRound = m_currentRoundNumber;
			marketDto.TotalRounds = m_numberOfRounds;
			marketDto.IsHalfTime = IsMarketHalfTime;
			if (m_currentRoll != null)
			{
				marketDto.RollDto = RollToRollDto(m_currentRoll);
			}
			if(IsMarketOpen)
			{
				marketDto.MarketCloseTimeInMilliseconds = m_currentMarketEndTime;
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
			return m_stocks.Values.Select(stock => stock.Name).ToList();
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
