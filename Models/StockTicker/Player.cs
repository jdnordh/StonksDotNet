using StockTickerDotNetCore.Models.DataTransferObjects;
using System.Collections.Generic;
using System.Linq;

namespace Models.StockTicker.GameClasses
{
	public class Player
	{
		public string Name { get; private set; }

		public int Money { get; set; }

		public Dictionary<string, int> Holdings;

		private List<string> m_stocks;

		public Player(string name, List<string> stocks)
		{
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
			return new PlayerInventoryDto(Name, Money, Holdings);
		}
	}
}
