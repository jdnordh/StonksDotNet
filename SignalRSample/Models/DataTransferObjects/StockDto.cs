using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class StockDto
	{
		[JsonInclude]
		[JsonProperty("value")]
		public int Value { get; }


		[JsonInclude]
		[JsonProperty("name")]
		public string Name { get; }

		[JsonInclude]
		[JsonProperty("color")]
		public string Color { get; }

		[JsonInclude]
		[JsonProperty("isHalved")]
		public bool IsHalved { get; }

		/// <summary>
		/// Create a <see cref="StockDto"/> with a specific percentage value.
		/// </summary>
		/// <param name="name">The stock name.</param>
		/// <param name="color">The color of the stock.</param>
		/// <param name="isHalved">If the stock is halved or not.</param>
		/// <param name="value">The value.</param>
		public StockDto(string name, string color, bool isHalved = false, int value = 1)
		{
			Value = value;
			Name = name;
			Color = color;
			IsHalved = isHalved;
		}
	}
}
