using Models.DataTransferObjects;
using System.Collections.Generic;

namespace Models.Game
{
	public class Player
	{
		public string Id { get; }

		public string Name { get;  }

		public int Money { get; set; }

		public Dictionary<string, int> Holdings;

		private List<string> m_stocks;

		public Player(string id, string name, List<string> stocks)
		{
			Id = id;
			Name = name;
			m_stocks = stocks;
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
			return new PlayerInventoryDto(Money, Holdings);
		}
	}
}
