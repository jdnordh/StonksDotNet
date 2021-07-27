
namespace Models.Game
{
	/// <summary>
	/// A function that modifies stock values, or pays divideds.
	/// </summary>
	public class StockFunc
	{
		public StockFuncType Type {get;}

		public string StockName { get; }

		public decimal PercentageAmount { get; }

		public StockFunc(StockFuncType type, string stockName, decimal percentageAmount)
		{
			Type = type;
			StockName = stockName;
			PercentageAmount = percentageAmount;
		}
	}

	public enum StockFuncType
	{
		Up,
		Down,
		Dividend
	}
}
