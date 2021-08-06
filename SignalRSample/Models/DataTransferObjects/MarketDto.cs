using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class MarketDto
	{
		[JsonInclude]
		[JsonProperty("isOpen")]
		public bool IsOpen { get; }

		[JsonInclude]
		[JsonProperty("rollDto")]
		public RollDto RollDto { get; set; }

		[JsonInclude]
		[JsonProperty("currentRound")]
		public int CurrentRound { get; set; }

		[JsonInclude]
		[JsonProperty("totalRounds")]
		public int TotalRounds { get; set; }

		[JsonInclude]
		[JsonProperty("marketCloseTimeInMilliseconds")]
		public long MarketCloseTimeInMilliseconds { get; set; }

		[JsonInclude]
		[JsonProperty("stocks")]
		public Dictionary<string, StockDto> Stocks { get; }

		[JsonInclude]
		[JsonProperty("playerInventories")]
		public PlayerInventoryCollectionDto PlayerInventories { get; set; }

		public MarketDto(bool isOpen, Dictionary<string, StockDto> stocks)
		{
			IsOpen = isOpen;
			Stocks = stocks;
		}
	}
}
