using System;
using System.Collections.Generic;

namespace StockTickerDotNetCore.Models.DataTransferObjects
{
	[Serializable]
	public class PlayerInventoryDto
	{
		public string Name { get; }

		public int Money { get; }

		public Dictionary<string, int> Holdings { get; }

		public PlayerInventoryDto(string playerName, int money, Dictionary<string, int> holdings)
		{
			Name = playerName;
			Money = money;
			Holdings = holdings;
		}
	}
}
