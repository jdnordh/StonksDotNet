using System;
using System.Collections.Generic;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class PlayerTransactionDto
	{
		/// <summary>
		/// True if buying, false if selling.
		/// </summary>
		public bool IsBuyTransaction { get; }

		public int StockAmount { get; }

		public string StockName { get; }

		public PlayerTransactionDto(bool isBuyTransaction, int stockAmount, string stockName)
		{
			IsBuyTransaction = isBuyTransaction;
			StockAmount = stockAmount;
			StockName = stockName;
		}
	}
}
