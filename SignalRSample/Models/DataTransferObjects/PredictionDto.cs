using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class PredictionDto
	{

		[JsonInclude]
		[JsonProperty("stockName")]
		public string StockName { get; }

		[JsonInclude]
		[JsonProperty("isUp")]
		public bool IsUp{ get; }


		public PredictionDto(string stockName, bool isUp)
		{
			StockName = stockName;
			IsUp = isUp;
		}
	}
}
