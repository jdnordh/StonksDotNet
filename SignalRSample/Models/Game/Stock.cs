
namespace Models.Game
{
	public class Stock
	{
		public decimal Value { get; private set; }

		public string Name { get; }

		public string Color { get; }

		public bool IsHalved { get; }

		public Stock(string name, string color, bool isHalved)
		{
			Name = name;
			Value = 1;
			Color = color;
			IsHalved = isHalved;
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

		public bool IsPayingDividends()
		{
			return Value >= 1M;
		}

		public int GetValueOfAmount(int amount)
		{
			return (int)(Value * (decimal)amount);
		}
	}
}
