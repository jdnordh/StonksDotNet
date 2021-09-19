
using Models.DataTransferObjects;

namespace Models.Game
{
	public class Stock
	{
		private const decimal DefaultStockValue = 1M;

		public decimal Value { get; private set; }

		public string Name { get; }

		public string Color { get; }

		public bool IsHalved { get; }

		/// <summary>
		/// Create a new stock based on a <see cref="StockDto"/> with the deafult value;
		/// </summary>
		/// <param name="stockDto">The <see cref="StockDto"/>.</param>
		public Stock(StockDto stockDto) : this (stockDto.Name, stockDto.Color, stockDto.IsHalved)
		{
		}

		/// <summary>
		/// Create a new stock with a name.
		/// </summary>
		/// <param name="name">Thestock name.</param>
		public Stock(string name) : this(name, null, false)
		{
		}

		/// <summary>
		/// Create a new stock.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="color">The color.</param>
		/// <param name="isHalved">If the stock is halved or not.</param>
		private Stock(string name, string color, bool isHalved)
		{
			Name = name;
			Value = DefaultStockValue;
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

		public StockDto ToStockDto()
		{
			return new StockDto(Name, Color, IsHalved, (int)(Value * 100));
		}
	}
}
