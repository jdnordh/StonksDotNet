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

		// Dice
		private readonly Die<string> m_stockDie;
		private readonly Die<decimal> m_amountDie;
		private readonly Die<Action<string, decimal>> m_actionDie;

		// Keeping track of place in game
		private int m_currentRoundNumber = 0;
		private int m_currentRollNumber = 0;
		private bool m_gameStarted = false;

		#endregion

		#region Events

		/// <summary>
		/// Fires when any player inventories are updated.
		/// </summary>
		public event Action<StonkTraderGame, List<(string id, PlayerInventoryDto inventory)>> PlayerInventoriesUpdated;

		/// <summary>
		/// Fires when markets open. 
		/// </summary>
		public event Action<StonkTraderGame, MarketDto> MarketUpdated;

		/// <summary>
		/// Fires when the game ends.
		/// </summary>
		public event Action<StonkTraderGame, List<(string playerName, int money)>> GameEnded;

		/// <summary>
		/// Fires when a roll happens
		/// </summary>
		public event Action<StonkTraderGame, MarketDto> Rolled;

		#endregion

		#region Properties

		public Dictionary<string, Stock> Stocks;

		public bool IsMarketOpen { get; private set; }


		public Dictionary<string, Player> Players;

		public string LastRollActionName { get; private set; }

		#endregion

		#region Constructor 

		public StonkTraderGame(GameInitializer initializer)
		{
			Players = new Dictionary<string, Player>();
			m_numberOfRounds = initializer.NumberOfRounds;
			m_numberOfRollsPerRound = initializer.RollsPerRound;
			m_startingMoney = initializer.StartingMoney;
			m_marketOpenTimeInSeconds = initializer.MarketOpenTimeInSeconds;
			m_rollTimeInSeconds = initializer.RollTimeInSeconds;

			m_stockDie = new Die<string>() { Results = initializer.Stocks.Select(stock => stock.stockName).ToList() };
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
						LastRollActionName = "UP";
						ResolveSplitOrCrash();
					},
					(stock, amount) =>
					{
						Stocks[stock].DecreaseValue(amount);
						LastRollActionName = "DOWN";
						ResolveSplitOrCrash();
					},
					(stock, amount) =>
					{
						LastRollActionName = "DIVIDEND";
						PayDividends(stock, amount);
					}
				}
			};

			Stocks = new Dictionary<string, Stock>();
			foreach (var (stockName, color) in initializer.Stocks)
			{
				Stocks.Add(stockName, new Stock(stockName, color));
			}
			IsMarketOpen = false;
		}

		public PlayerInventoryDto AddPlayer(string id, string username)
		{
            var player = new Player(id, username, Stocks.Values.Select(stock => stock.Name).ToList())
            {
                Money = m_startingMoney
            };
            Players.Add(id, player);
			return player.GetPlayerInvetory();
		}
		#endregion

		#region Gameplay

		public async void StartGame()
		{
			m_gameStarted = true;

			for(int round = 0; round < m_numberOfRounds; round++)
            {
				await OpenAndCloseMarket();

				for(int roll = 0; roll < m_numberOfRollsPerRound; roll++)
                {
					Roll();

					await Task.Delay(m_rollTimeInSeconds * 1000);
                }
            }
			EndGame();
		}

		public void EndGame()
		{
			m_gameStarted = false;
			var playerWallets = new List<(string id, int wallet)>();
			SellAllShares();
			foreach(var player in Players.Values)
            {
				playerWallets.Add((player.Name, player.Money));
            }
			GameEnded?.Invoke(this, playerWallets);
		}

		/// <summary>
		/// Get a market dto.
		/// </summary>
		/// <returns>The dto.</returns>
		public MarketDto GetMarketDto()
		{
			var stocksDto = new Dictionary<string, StockDto>();
			foreach(var kvp in Stocks)
            {
				stocksDto.Add(kvp.Key, new StockDto(kvp.Value.Name, kvp.Value.Value) { Color = kvp.Value.Color });
            }
			return new MarketDto(IsMarketOpen, m_marketOpenTimeInSeconds, stocksDto) 
			{
				RollNumber = m_currentRollNumber,
				RoundNumber = m_currentRoundNumber
			};
		}

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
			MarketUpdated?.Invoke(this, GetMarketDto());

			await Task.Delay(m_marketOpenTimeInSeconds * 1000);

			IsMarketOpen = false;
			MarketUpdated?.Invoke(this, GetMarketDto());
		}

		/// <summary>
		/// Roll the dice.
		/// </summary>
		private void Roll()
		{
			var stock = m_stockDie.Roll();
			var amount = m_amountDie.Roll();
			var action = m_actionDie.Roll();
			action(stock, amount);

			m_currentRollNumber++;

			Rolled?.Invoke(this, GetMarketDto());
		}

		private void PayDividends(string stock, decimal percentage)
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
			PlayerInventoriesUpdated?.Invoke(this, updatedPlayerInvectories);
		}

		private void ResolveSplitOrCrash()
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
				if (stock.Value >= 2)
				{
					foreach (var player in Players.Values)
					{
						updatedPlayerInvectories.Add((player.Id, player.GetPlayerInvetory()));
						player.Holdings[stock.Name] = player.Holdings[stock.Name] * 2;
					}
					stock.ResetValue();
				}
			}
			PlayerInventoriesUpdated?.Invoke(this, updatedPlayerInvectories);
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

        #region Utilities


        #endregion
    }
}
