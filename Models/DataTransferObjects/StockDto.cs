using Models.StockTicker.GameClasses;
using System;

namespace StockTickerDotNetCore.Models.DataTransferObjects
{
	[Serializable]
	public class StockDto
	{
		public int Value { get; private set; }

		public string Name { get; }

		public StockDto(Stock stock)
		{
			Value = (int)(stock.Value * 100);
			Name = stock.Name;
		}

		public static implicit operator StockDto(Stock stock)
        {
			return new StockDto(stock);
        }
	}
}
