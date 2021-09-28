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

		public PlayerInventoryDto GetPlayerInvetory()
		{
			return new PlayerInventoryDto(Id, Money, Holdings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
				Username, new CharacterDto(Character.GetDetailedInformation(), Character.Id));
		}
	}
}
