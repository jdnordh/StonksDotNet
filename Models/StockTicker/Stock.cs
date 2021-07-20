
namespace Models.StockTicker.GameClasses
{
	public class Stock
	{
		public decimal Value { get; private set; }

		public string Name { get; }

		public string Color { get; }

		public Stock(string name, string color)
		{
			Name = name;
			Value = 1;
			Color = color;
		}

		public void IncreaseValue(decimal increment)
		{
			Value += increment;
		}

		public void DecreaseValue(decimal decrement)
		{
			Value -= decrement;
		}

		public void ResetValue()
		{
			Value = 1;
		}
	}
}
