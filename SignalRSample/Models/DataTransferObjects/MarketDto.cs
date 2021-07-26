using System;
using System.Collections.Generic;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class MarketDto
	{
		public bool IsOpen { get; }

		public RollDto RollDto { get; set; }

		public long MarketCloseTimeInMilliseconds { get; set; }

		public Dictionary<string, StockDto> Stocks { get; }

		public MarketDto(bool isOpen, Dictionary<string, StockDto> stocks)
		{
			IsOpen = isOpen;
			Stocks = stocks;
		}
	}
}
