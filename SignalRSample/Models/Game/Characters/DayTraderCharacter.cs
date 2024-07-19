
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The day trader character that gets to make transactions at half time of the market.
	/// </summary>
	public class DayTraderCharacter : CharacterBase
	{
		#region Properties

		/// <inheritdoc />
		public override string Name => "Day Trader";

		/// <inheritdoc />
		public override string Description => "This character can make additional trades half way through a closed market and gets information about trends.";

		/// <inheritdoc />
		public override int Id => 2;

		/// <inheritdoc />
		public override bool GetsHalfTimeTransaction => true;

		/// <inheritdoc />
		public override bool GetsStockAnalyze => true;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		public override string GetDetailedInformation()
		{
			string preamble = AnalyzedStock == null ? "" : $"Currently analyzing {AnalyzedStock.Name}. ";
			return $"{preamble}As the Day Trader, you get to trade in the Half Time market. This is an exclusive open market halfway through the rounds. Additionally, during half time, you get to see a trend of what will happen to the analyzed stock in the coming rounds.";
		}

		#endregion
	}
}
