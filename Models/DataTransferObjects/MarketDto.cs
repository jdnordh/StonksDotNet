using System;
using System.Collections.Generic;

namespace StockTickerDotNetCore.Models.DataTransferObjects
{
	[Serializable]
	public class MarketDto
	{
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
