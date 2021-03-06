using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class ShortDto
	{
		[JsonInclude]
		[JsonProperty("stockName")]
		public string StockName { get; }

		[JsonInclude]
		[JsonProperty("sharesAmount")]
		public int SharesAmount{ get; set; }

		[JsonInclude]
		[JsonProperty("purchasePrice")]
		public int PurchasePrice { get; }

		[JsonInclude]
		[JsonProperty("sharesSoldPrice")]
		public int SharesSoldPrice { get; }

		public ShortDto(string stockName, int sharesAmount, int purchasePrice, int sharesSoldPrice)
		{
			StockName = stockName;
			SharesAmount = sharesAmount;
			PurchasePrice = purchasePrice;
			SharesSoldPrice = sharesSoldPrice;
		}
	}
}
