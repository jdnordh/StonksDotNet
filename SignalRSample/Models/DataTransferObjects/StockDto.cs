using System;

namespace Models.DataTransferObjects
{
	[Serializable]
	public class StockDto
	{
		public int Value { get; }

		public string Name { get; }

		public string Color { get;}

		public bool IsHalved { get; }

		public StockDto(string name, decimal value, bool isHalved, string color)
		{
			Value = (int)(value * 100);
			Name = name;
			Color = color;
			IsHalved = isHalved;
		}
	}
}
