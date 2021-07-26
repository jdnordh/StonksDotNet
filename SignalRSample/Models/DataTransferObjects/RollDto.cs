using System;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class RollDto
	{
		public string StockName { get; }

		public string Func { get; }

		public int Amount { get; }

		public RollDto(string stockName, string func, int amount)
		{
			StockName = stockName;
			Func = func;
			Amount = amount;
		}
	}
}
