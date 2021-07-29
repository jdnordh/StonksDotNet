
namespace Models.Game
{
	public class GameInitializer
	{
		public int MarketOpenTimeInSeconds { get; set; }
		public int NumberOfRounds { get; set; }
		public int RollTimeInSeconds { get; set; }
		public int TimeBetweenRollsInSeconds { get; set; }
		public int RollsPerRound { get; set; }
		public int StartingMoney { get; set; }
		public bool IsPrototype { get; set; }
		public (string stockName, string color, bool isHalved) [] Stocks { get; set; }
	}
}
