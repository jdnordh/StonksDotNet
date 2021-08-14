using Models.DataTransferObjects;
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

		public Player(string id, string connectionId, string username, List<string> stocks)
		{
			Id = id;
			ConnectionId = connectionId;
			Username = username;
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
			return new PlayerInventoryDto(Id, Money, Holdings, Username);
		}
	}
}
