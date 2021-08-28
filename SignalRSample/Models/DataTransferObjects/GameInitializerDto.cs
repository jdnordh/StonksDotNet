
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class GameInitializerDto
	{
		[JsonInclude]
		[JsonProperty("marketOpenTimeInSeconds")]
		public int MarketOpenTimeInSeconds { get; set; }

		[JsonInclude]
		[JsonProperty("startingMoney")]
		public int StartingMoney { get; set; }

		[JsonInclude]
		[JsonProperty("rollsPerRound")]
		public int RollsPerRound { get; set; }

		[JsonInclude]
		[JsonProperty("stockPreset")]
		public int StockPreset { get; set; }

		[JsonInclude]
		[JsonProperty("numberOfRounds")]
		public int NumberOfRounds { get; set; }

		[JsonInclude]
		[JsonProperty("stocks")]
		public StockDto [] Stocks { get; set; }
	}
}
