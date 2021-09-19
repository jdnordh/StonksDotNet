using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class TrendDto
	{

		[JsonInclude]
		[JsonProperty("stockName")]
		public string StockName { get; }

		[JsonInclude]
		[JsonProperty("direction")]
		public string Direction { get; }

		[JsonInclude]
		[JsonProperty("isNoInformation")]
		public bool IsNoInformation { get; }


		public TrendDto(string stockName, string direction, bool isNoInformation = false)
		{
			StockName = stockName;
			Direction = direction;
			IsNoInformation = isNoInformation;
		}
	}
}
