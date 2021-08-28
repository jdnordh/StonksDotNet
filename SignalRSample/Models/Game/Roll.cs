
namespace Models.Game
{
	/// <summary>
	/// A function that modifies stock values, or pays divideds.
	/// </summary>
	public class Roll
	{
		public RollType Type {get;}

		public string StockName { get; }

		public decimal PercentageAmount { get; }

		public Roll(RollType type, string stockName, decimal percentageAmount)
		{
			Type = type;
			StockName = stockName;
			PercentageAmount = percentageAmount;
		}
	}

	public enum RollType
	{
		Up,
		Down,
		Dividend
	}
}
