using System;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class StockDto
	{
		public int Value { get; }

		public string Name { get; }

		public string Color { get; set; }

		public StockDto(string name, Decimal value, string color = null)
		{
			Value = (int)(value * 100);
			Name = name;
			Color = color;
		}
	}
}
