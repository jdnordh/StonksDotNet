using System;
using System.Collections.Generic;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class PlayerInventoryDto
	{
		public string Username { get; set; }
		public int Money { get; }

		public Dictionary<string, int> Holdings { get; }

		public PlayerInventoryDto(int money, Dictionary<string, int> holdings, string username)
		{
			Money = money;
			Holdings = holdings;
			Username = username;
		}
	}
}
