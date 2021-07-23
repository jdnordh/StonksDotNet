using System;
using System.Collections.Generic;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class MarketDto
	{
		public int RollNumber { get; set; }

		public int RoundNumber { get; set; }

		public bool IsOpen { get; }

		public int MarketOpenTimeInSeconds { get; }

		public Dictionary<string, StockDto> Stocks { get; }

		public MarketDto(bool isOpen, int marketOpenTimeInSeconds, Dictionary<string, StockDto> stocks)
		{
			IsOpen = isOpen;
			MarketOpenTimeInSeconds = marketOpenTimeInSeconds;
			Stocks = stocks;
		}
	}
}
