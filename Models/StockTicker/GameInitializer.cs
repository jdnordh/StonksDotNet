
namespace StockTickerDotNetCore.Models.StockTicker
{
	public class GameInitializer
	{
		public int MarketOpenTimeInSeconds { get; set; }
		public int NumberOfRounds { get; set; }
		public int PlayerRollsPerRound { get; set; }
		public int StartingMoney { get; set; }
		public string [] StockNames { get; set; }
		public string [] StockColors { get; set; }
	}
}
