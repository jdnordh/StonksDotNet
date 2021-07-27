using Models.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.Game
{
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

		// Rolling
		private readonly Die<string> m_stockDie;
		private readonly Die<decimal> m_amountDie;
		private readonly Die<Func<string, decimal, StockFunc>> m_funcDie;


		#endregion

		#region Properties

		public Dictionary<string, Stock> Stocks;

		public bool IsStarted { get; private set; }

		public bool IsMarketOpen { get; private set; }

		public Dictionary<string, Player> Players;

		#endregion

		#region Constructor 

		public StonkTraderGame(GameInitializer initializer, IGameEventCommunicator gameEventCommunicator)
		{
			Players = new Dictionary<string, Player>();
			m_numberOfRounds = initializer.NumberOfRounds;
			m_numberOfRollsPerRound = initializer.RollsPerRound;
			m_startingMoney = initializer.StartingMoney;
			m_marketOpenTimeInSeconds = initializer.MarketOpenTimeInSeconds;
			m_rollTimeInSeconds = initializer.RollTimeInSeconds;
			m_timeBetweenRollsInSeconds = initializer.TimeBetweenRollsInSeconds;
			m_gameEventCommunicator = gameEventCommunicator;

			m_stockDie = new Die<string>() { Results = initializer.Stocks.Select(stock => stock.stockName).ToList() };
			m_amountDie = new Die<decimal>
			{
				Results = new List<decimal>
				{
					//0.05M,
					//.1M,
					//.2M
					1.0M
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
			foreach (var (stockName, color) in initializer.Stocks)
			{
				Stocks.Add(stockName, new Stock(stockName, color));
			}
			IsMarketOpen = false;
			IsStarted = false;
		}

		#endregion

		#region Initialization
		public PlayerInventoryDto AddPlayer(string connectionId, string username)
		{
			var player = new Player(connectionId, username, Stocks.Values.Select(stock => stock.Name).ToList())
			{
				Money = m_startingMoney
			};
			Players.Add(connectionId, player);
			var inventory = player.GetPlayerInvetory();
			return inventory;
		}

		public async void StartGame()
		{
			IsStarted = true;

			for (int round = 0; round < m_numberOfRounds; round++)
			{
				await OpenAndCloseMarket();

				for (int roll = 0; roll < m_numberOfRollsPerRound; roll++)
				{
					await Roll();

					await Task.Delay(m_timeBetweenRollsInSeconds * 1000);
				}
			}
			EndGame();
		}

		#endregion

		#region Dice Rolling

		/// <summary>
		/// Opens the market.
		/// </summary>
		private async Task OpenAndCloseMarket()
		{
			if (IsMarketOpen)
			{
				return;
			}
			IsMarketOpen = true;

			var marketMiliseconds = m_marketOpenTimeInSeconds * 1000;
			var marketEndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + marketMiliseconds;
			var marketDto = GetMarketDto();
			marketDto.MarketCloseTimeInMilliseconds = marketEndTime;
			await m_gameEventCommunicator.GameMarketChanged(marketDto);

			await Task.Delay(m_marketOpenTimeInSeconds * 1000);

			IsMarketOpen = false;
			await m_gameEventCommunicator.GameMarketChanged(GetMarketDto());
		}

		/// <summary>
		/// Roll the dice.
		/// </summary>
		private async Task Roll()
		{
			var stockResult = m_stockDie.Roll();
			var amountResult = m_amountDie.Roll();
			var stockFuncResult = m_funcDie.Roll();
			var rollResult = stockFuncResult(stockResult, amountResult);



			var marketDto = GetMarketDto();
			marketDto.RollDto = new RollDto(rollResult.StockName, rollResult.Type.ToString(), 
				(int)(rollResult.PercentageAmount * 100), m_rollTimeInSeconds);

			// Show roll on presenter
			await m_gameEventCommunicator.GameRolled(marketDto);

			// Wait for display to complete
			await Task.Delay(m_rollTimeInSeconds * 1000);

			// Do roll action after the presenter has shown it so clients don't get the update before it's on screen.
			switch (rollResult.Type)
			{
				case StockFuncType.Up:
					{
						Stocks[rollResult.StockName].IncreaseValue(rollResult.PercentageAmount);
						await ResolveSplitOrCrash();
						break;
					}
				case StockFuncType.Down:
					{
						Stocks[rollResult.StockName].DecreaseValue(rollResult.PercentageAmount);
						await ResolveSplitOrCrash();
						break;
					}
				case StockFuncType.Dividend:
					{
						await PayDividends(rollResult.StockName, rollResult.PercentageAmount);
						break;
					}
			}
		}

		/// <summary>
		/// Pay divideds to the holders of given stock based on the given percentage.
		/// </summary>
		/// <param name="stock">The stock paying dividends.</param>
		/// <param name="percentage">The percentage to pay out.</param>
		private async Task PayDividends(string stock, decimal percentage)
		{
			var updatedPlayerInvectories = new List<(string id, PlayerInventoryDto)>();
			foreach (var player in Players.Values)
			{
				int holdings = player.Holdings[stock];
				if (holdings > 0)
				{
					updatedPlayerInvectories.Add((player.Id, player.GetPlayerInvetory()));
					player.Money += (int)(holdings * percentage);
				}
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(updatedPlayerInvectories);
		}

		/// <summary>
		/// Resolves any stocks that have split or crashed.
		/// </summary>
		private async Task ResolveSplitOrCrash()
		{
			var updatedPlayerInvectories = new List<(string id, PlayerInventoryDto)>();
			foreach (var stock in Stocks.Values)
			{
				// If stock crashes, remove all shares
				if (stock.Value <= 0)
				{
					foreach (var player in Players.Values)
					{
						updatedPlayerInvectories.Add((player.Id, player.GetPlayerInvetory()));
						player.Holdings[stock.Name] = 0;
					}
					stock.ResetValue();
				}
				// If stock splits, double all shares
				else if (stock.Value >= 2)
				{
					foreach (var player in Players.Values)
					{
						updatedPlayerInvectories.Add((player.Id, player.GetPlayerInvetory()));
						player.Holdings[stock.Name] = player.Holdings[stock.Name] * 2;
					}
					stock.ResetValue();
				}
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(updatedPlayerInvectories);
		}

		#endregion

		#region Buy and Sell

		/// <summary>
		/// Determine if the given player can perform a proposed buy operation.
		/// </summary>
		/// <param name="userId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToBuy">The amount of stock to buy.</param>
		/// <returns>True if the operation is okay.</returns>
		public bool IsBuyOkay(string userId, string stockName, int amountToBuy)
		{
			if (!IsStarted)
			{
				return false;
			}
			if (!Players.ContainsKey(userId))
			{
				throw new InvalidOperationException($"The user '{userId}' was not present in the game.");
			}
			if (!Stocks.ContainsKey(stockName))
			{
				return false;
			}
			if (amountToBuy % 500 != 0)
			{
				return false;
			}
			var player = Players[userId];
			int cost = Stocks[stockName].GetValueOfAmount(amountToBuy);
			return player.Money >= cost;
		}

		/// <summary>
		/// Buy an amount of stock.
		/// </summary>
		/// <param name="userId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToBuy">The amount of stock to buy.</param>
		/// <returns>Updated player money.</returns>
		public PlayerInventoryDto BuyStock(string userId, string stockName, int amountToBuy)
		{
			var player = Players[userId];
			if (!IsBuyOkay(userId, stockName, amountToBuy))
			{
				return player.GetPlayerInvetory();
			}
			int cost = Stocks[stockName].GetValueOfAmount(amountToBuy);
			player.Money -= cost;
			player.Holdings[stockName] += amountToBuy;
			return player.GetPlayerInvetory();
		}

		/// <summary>
		/// Determine if the given player can perform a proposed sell operation.
		/// </summary>
		/// <param name="userId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToSell">The amount of stock to buy.</param>
		/// <returns>True if the operation is okay.</returns>
		public bool IsSellOkay(string userId, string stockName, int amountToSell)
		{
			if (!IsStarted)
			{
				return false;
			}
			if (!Players.ContainsKey(userId))
			{
				throw new InvalidOperationException($"The user '{userId}' was not present in the game.");
			}
			if (!Stocks.ContainsKey(stockName))
			{
				return false;
			}
			if (amountToSell % 500 != 0)
			{
				return false;
			}
			var player = Players[userId];
			return player.Holdings[stockName] >= amountToSell;
		}

		/// <summary>
		/// Sell an amount of stock.
		/// </summary>
		/// <param name="userId">The user id.</param>
		/// <param name="stockName">The stock to buy.</param>
		/// <param name="amountToSell">The amount of stock to buy.</param>
		/// <returns>Updated player money.</returns>
		public PlayerInventoryDto SellStock(string userId, string stockName, int amountToSell)
		{
			var player = Players[userId];
			if (!IsSellOkay(userId, stockName, amountToSell))
			{
				return player.GetPlayerInvetory();
			}
			int soldFor = Stocks[stockName].GetValueOfAmount(amountToSell);
			player.Holdings[stockName] -= amountToSell;
			player.Money += soldFor;
			return player.GetPlayerInvetory();
		}

		#endregion

		#region Game End

		private async void EndGame()
		{
			var playerWallets = new List<(string id, int wallet)>();
			var updatedPlayerInvectories = new List<(string id, PlayerInventoryDto)>();
			SellAllShares();
			foreach (var player in Players.Values)
			{
				updatedPlayerInvectories.Add((player.Id, player.GetPlayerInvetory()));
				playerWallets.Add((player.Username, player.Money));
			}
			await m_gameEventCommunicator.PlayerInventoriesUpdated(updatedPlayerInvectories);
			await m_gameEventCommunicator.GameEnded(new GameEndDto(Players.Values.Select(p => p.GetPlayerInvetory()).ToList()));
			IsStarted = false;
		}

		/// <summary>
		/// Convert all player holdings to money.
		/// </summary>
		private void SellAllShares()
		{
			foreach (var player in Players.Values)
			{
				foreach (var holding in player.Holdings)
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
			foreach (var kvp in Stocks)
			{
				stocksDto.Add(kvp.Key, new StockDto(kvp.Value.Name, kvp.Value.Value) { Color = kvp.Value.Color });
			}
			return new MarketDto(IsMarketOpen, stocksDto);
		}

		#endregion
	}
}
