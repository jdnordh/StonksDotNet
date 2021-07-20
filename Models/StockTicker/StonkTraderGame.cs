using StockTickerDotNetCore.Models.DataTransferObjects;
using StockTickerDotNetCore.Models.StockTicker;
using StockTickerDotNetCore.Models.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Models.StockTicker.GameClasses
{
	public class StonkTraderGame
	{
		#region Fields

		private readonly int m_rounds;
		private readonly int m_playerRollsPerRound;
		private int m_totalRollsPerRound;
		private int m_currentRollNumber = 0;
		private readonly int m_startingMoney;
		private readonly Die<string> m_stockDie;
		private readonly Die<decimal> m_amountDie;
		private readonly Die<Action<string, decimal>> m_actionDie;
		private int m_userPlayingTurnIndex = 0;
		private bool m_gameStarted = false;
		private int m_marketOpenTimeInSeconds;

		#endregion

		#region Events

		/// <summary>
		/// Fires when dividends are paid.
		/// </summary>
		public event Action<StonkTraderGame> DividendsPaid;

		/// <summary>
		/// Fires when markets open. 
		/// </summary>
		public event Action<StonkTraderGame> MarketOpened;

		/// <summary>
		/// Fires when markets close. 
		/// </summary>
		public event Action<StonkTraderGame> MarketClosed;

		#endregion

		#region Properties

		public Dictionary<string, Stock> Stocks;

		public int CurrentRoll 
		{
			get
			{
				return m_currentRollNumber;
			}
			private set
			{
				m_currentRollNumber = value;

				// If round is completed, open markets
				if (!IsMarketOpen && m_currentRollNumber == m_totalRollsPerRound)
				{
					IsMarketOpen = true;
					m_currentRollNumber = 0;
					OpenMarket();
					MarketOpened?.Invoke(this);
					IsMarketOpen = false;
				}
			}
		}

		public bool IsMarketOpen { get; private set; }

		public string UserPlayingTurn { get; private set; }

		public Dictionary<string, Player> Players;

		public int WinnerMoney { get; private set; }

		#endregion

		#region Constructor 

		public StonkTraderGame(GameInitializer initializer)
		{
			Players = new Dictionary<string, Player>();
			m_rounds = initializer.NumberOfRounds;
			m_playerRollsPerRound = initializer.PlayerRollsPerRound;
			m_startingMoney = initializer.StartingMoney;
			WinnerMoney = -1;
			UserPlayingTurn = null;
			m_marketOpenTimeInSeconds = initializer.MarketOpenTimeInSeconds;

			m_stockDie = new Die<string>() { Results = initializer.StockNames.ToList() };
			m_amountDie = new Die<decimal>
			{
				Results = new List<decimal>
				{
					0.05M,
					.1M,
					.2M
				}
			};
			m_actionDie = new Die<Action<string, decimal>>
			{
				Results = new List<Action<string, decimal>>
				{
					(stock, amount) =>
					{
						Stocks[stock].IncreaseValue(amount);
						ResolveSplitOrCrash();
					},
					(stock, amount) =>
					{
						Stocks[stock].DecreaseValue(amount);
						ResolveSplitOrCrash();
					},
					(stock, amount) =>
					{
						PayDividends(stock, amount);
					}
				}
			};

			CreateStocks(initializer);
		}

		private void CreateStocks(GameInitializer initializer)
		{
			Stocks = new Dictionary<string, Stock>();
			var colorManager = new ColorManager();
			for (int i = 0; i < initializer.StockNames.Length; ++i)
			{
				var stockName = initializer.StockNames[i];
				string color = null;
				if (initializer.StockColors.Length > i)
				{
					color = initializer.StockColors[i];
					if (colorManager.IsColorInUse(color))
					{
						color = null;
					}
					else
					{
						colorManager.SetColorInUse(color);
					}
				}
				if (color == null)
				{
					color = colorManager.GetUnusedColor();
				}
				Stocks.Add(stockName, new Stock(stockName, color));
			}
		}

		public void AddPlayer(string id, Player player)
		{
			player.Money = m_startingMoney;
			Players.Add(id, player);
			if (UserPlayingTurn == null)
			{
				UserPlayingTurn = id;
			}
		}

		public void StartGame()
		{
			m_totalRollsPerRound = Players.Values.Count * m_playerRollsPerRound;
			m_gameStarted = true;
		}

		#endregion

		#region Gameplay

		/// <summary>
		/// Get a market dto.
		/// </summary>
		/// <returns>The dto.</returns>
		public MarketDto GetMarketDto()
		{
			var stocksDto = new Dictionary<string, StockDto>();
			foreach(var kvp in Stocks)
            {
				stocksDto.Add(kvp.Key, kvp.Value);
            }
			return new MarketDto(IsMarketOpen, m_marketOpenTimeInSeconds, stocksDto);
		}

		/// <summary>
		/// Opens the market
		/// </summary>
		private async void OpenMarket()
		{
			if (IsMarketOpen)
			{
				throw new InvalidOperationException("Market was already open.");
			}
			MarketOpened?.Invoke(this);

			await Task.Run(async delegate
			{
				await Task.Delay(m_marketOpenTimeInSeconds * 1000);
			});
			IsMarketOpen = false;
			MarketClosed?.Invoke(this);
		}

		/// <summary>
		/// Roll the dice.
		/// </summary>
		/// <param name="userId">The user rolling.</param>
		public void Roll(string userId)
		{
			if (!m_gameStarted)
			{
				return;
			}
			if (!Players.ContainsKey(userId))
			{
				throw new InvalidOperationException($"The user '{userId}' was not present in the game.");
			}
			m_userPlayingTurnIndex++;
			UserPlayingTurn = Players.Keys.ElementAt(m_userPlayingTurnIndex);
			var stock = m_stockDie.Roll();
			var amount = m_amountDie.Roll();
			var action = m_actionDie.Roll();
			action(stock, amount);

			CurrentRoll++;
		}

		private void PayDividends(string stock, decimal percentage)
		{
			foreach (var player in Players.Values)
			{
				int holdings = player.Holdings[stock];
				if (holdings > 0)
				{
					player.Money += (int)(holdings * percentage);
				}
			}
			DividendsPaid?.Invoke(this);
		}

		private void ResolveSplitOrCrash()
		{
			foreach (var stock in Stocks.Values)
			{
				// If stock crashes, remove all shares
				if (stock.Value <= 0)
				{
					foreach (var player in Players.Values)
					{
						player.Holdings[stock.Name] = 0;
					}
					stock.ResetValue();
				}
				// If stock splits, double all shares
				if (stock.Value >= 2)
				{
					foreach (var player in Players.Values)
					{
						player.Holdings[stock.Name] = player.Holdings[stock.Name] * 2;
					}
					stock.ResetValue();
				}
			}
		}

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
			if (!m_gameStarted)
			{
				return false;
			}
			if (!Players.ContainsKey(userId))
			{
				throw new InvalidOperationException($"The user '{userId}' was not present in the game.");
			}
			if (!Stocks.ContainsKey(stockName))
			{
				throw new InvalidOperationException($"The stock '{stockName}' was not present in the game.");
			}
			if (amountToBuy % 500 != 0)
			{
				throw new InvalidOperationException($"The amount '{amountToBuy}' was not a multiple of 500.");
			}
			var player = Players[userId];
			int cost = (int)Stocks[stockName].Value * amountToBuy;
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
			if (!IsBuyOkay(userId, stockName, amountToBuy))
			{
				return null;
			}
			var player = Players[userId];
			int cost = (int)Stocks[stockName].Value * amountToBuy;
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
			if (!m_gameStarted)
			{
				return false;
			}
			if (!Players.ContainsKey(userId))
			{
				throw new InvalidOperationException($"The user '{userId}' was not present in the game.");
			}
			if (!Stocks.ContainsKey(stockName))
			{
				throw new InvalidOperationException($"The stock '{stockName}' was not present in the game.");
			}
			if (amountToSell % 500 != 0)
			{
				throw new InvalidOperationException($"The amount '{amountToSell}' was not a multiple of 500.");
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
			if (!IsSellOkay(userId, stockName, amountToSell))
			{
				return null;
			}
			var player = Players[userId];
			int cost = (int)Stocks[stockName].Value * amountToSell;
			player.Holdings[stockName] -= amountToSell;
			player.Money += cost;
			return player.GetPlayerInvetory();
		}

		#endregion

		#endregion

		#region Game End

		/// <summary>
		/// Convert all player holdings to money.
		/// </summary>
		private void SellAllShares()
		{
			foreach (var player in Players.Values)
			{
				foreach (var holding in player.Holdings)
				{
					player.Money += (int)Stocks[holding.Key].Value * holding.Value;
				}
				player.ClearAllShares();
			}
		}

		#endregion
	}
}
