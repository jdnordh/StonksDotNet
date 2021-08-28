using Models.DataTransferObjects;
using StonkTrader.Models.Game.Characters;
using System.Collections.Generic;

namespace Models.Game
{
	public class Player
	{
		/// <summary>
		/// The globally unique identifier of this player. Allows reconnection.
		/// </summary>
		public string Id { get; }

		public string ConnectionId { get; }

		public string Username { get;  }

		public int Money { get; set; }

		public Dictionary<string, int> Holdings;

		private List<string> m_stocks;

		public CharacterBase Character { get; }

		public Player(string id, string connectionId, string username, int startingMoney, List<string> stocks, CharacterBase character)
		{
			Id = id;
			ConnectionId = connectionId;
			Username = username;
			m_stocks = stocks;
			Money = startingMoney;
			Character = character;
			ClearAllShares();
		}

		public void ClearAllShares()
		{
			Holdings = new Dictionary<string, int>();
			foreach (var stock in m_stocks)
			{
				Holdings.Add(stock, 0);
			}
		}

		public PlayerInventoryDto GetPlayerInvetory()
		{
			return new PlayerInventoryDto(Id, Money, Holdings, Username, Character.Id);
		}
	}
}
