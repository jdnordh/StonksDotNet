
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
		[JsonProperty("numberOfRounds")]
		public int NumberOfRounds { get; set; }

		[JsonInclude]
		[JsonProperty("rollTimeInSeconds")]
		public int RollTimeInSeconds { get; set; }

		[JsonInclude]
		[JsonProperty("timeBetweenRollsInSeconds")]
		public int TimeBetweenRollsInSeconds { get; set; }

		[JsonInclude]
		[JsonProperty("isPrototype")]
		public bool IsPrototype { get; set; }

		[JsonInclude]
		[JsonProperty("stocks")]
		public StockDto [] Stocks { get; set; }
	}
}
