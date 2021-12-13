using Microsoft.AspNetCore.HttpOverrides;
using Models.DataTransferObjects;
using StonkTrader.Models.Game.Characters;
using System.Collections.Generic;
using System.Linq;

namespace Models.Game
{
	public class Player
	{
		/// <summary>
		/// The globally unique identifier of this player. Allows reconnection.
		/// </summary>
		public string Id { get; }

		public string Username { get;  }

		public int Money { get; set; }

		public readonly Dictionary<string, int> Holdings;

		public CharacterBase Character { get; }

		public ShortDto ShortPosition { get; set; }

		public Player(string id, string username, int startingMoney, List<string> stocks, CharacterBase character)
		{
			Id = id;
			Username = username;
			Money = startingMoney;
			Character = character;
			Holdings = new Dictionary<string, int>(stocks.ToDictionary(s => s, s => 0));
		}

		public void ClearAllShares()
		{
			foreach(var stockName in Holdings.Keys.ToList())
			{
				Holdings[stockName] = 0;
			}
		}

		public int GetShortValue(Dictionary<string, Stock> stocks)
		{
			int shortValue = 0;
			if(ShortPosition != null)
			{
				shortValue = ShortPosition.PurchasePrice + ShortPosition.SharesSoldPrice - stocks[ShortPosition.StockName].GetValueOfAmount(ShortPosition.SharesAmount);
			}
			return shortValue;
		}

		public int CalculateNetWorth(Dictionary<string, Stock> stocks)
		{
			int shortValue = GetShortValue(stocks);
			int netWorth = Money + shortValue;
			foreach(var holdingKvp in Holdings)
			{
				netWorth += stocks[holdingKvp.Key].GetValueOfAmount(holdingKvp.Value);
			}
			return netWorth;
		}

		public PlayerInventoryDto GetPlayerInventory(Dictionary<string, Stock> stocks)
		{
			int visualMoney = Money + GetShortValue(stocks);
			int netWorth = CalculateNetWorth(stocks);

			return new PlayerInventoryDto(Id, Money, visualMoney, netWorth, Holdings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
				Username, new CharacterDto(Character.GetDetailedInformation(), Character.Id), ShortPosition);
		}
	}
}
