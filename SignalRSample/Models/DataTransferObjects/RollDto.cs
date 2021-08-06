using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class RollDto
	{

		[JsonInclude]
		[JsonProperty("stockName")]
		public string StockName { get; }

		[JsonInclude]
		[JsonProperty("func")]
		public string Func { get; }

		[JsonInclude]
		[JsonProperty("amount")]
		public int Amount { get; }

		[JsonInclude]
		[JsonProperty("rollTimeInSeconds")]
		public int RollTimeInSeconds { get; }

		public RollDto(string stockName, string func, int amount, int rollTimeInSeconds)
		{
			StockName = stockName;
			Func = func;
			Amount = amount;
			RollTimeInSeconds = rollTimeInSeconds;
		}
	}
}
