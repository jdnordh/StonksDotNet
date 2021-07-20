using Models.StockTicker.GameClasses;
using System;
using System.Collections.Generic;

namespace StockTickerDotNetCore.Models.DataTransferObjects
{
	[Serializable]
	public class PlayerTransaction
	{
		/// <summary>
		/// True if buying, false if selling.
		/// </summary>
		public bool IsBuyTransaction { get; }

		public int StockAmount { get; }

		public string StockName { get; }

		public PlayerTransaction(bool isBuyTransaction, int stockAmount, string stockName)
		{
			IsBuyTransaction = isBuyTransaction;
			StockAmount = stockAmount;
			StockName = stockName;
		}
	}
}
