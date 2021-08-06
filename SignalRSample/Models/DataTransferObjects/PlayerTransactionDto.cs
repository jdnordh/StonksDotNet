using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class PlayerTransactionDto
	{
		[JsonInclude]
		[JsonProperty("isBuyTransaction")]
		public bool IsBuyTransaction { get; }

		[JsonInclude]
		[JsonProperty("stockAmount")]
		public int StockAmount { get; }

		[JsonInclude]
		[JsonProperty("stockName")]
		public string StockName { get; }

		public PlayerTransactionDto(bool isBuyTransaction, int stockAmount, string stockName)
		{
			IsBuyTransaction = isBuyTransaction;
			StockAmount = stockAmount;
			StockName = stockName;
		}
	}
}
