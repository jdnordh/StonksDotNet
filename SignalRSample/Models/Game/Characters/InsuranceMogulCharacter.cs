
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The insurance mogul character that benefits from stocks crashing.
	/// </summary>
	public class InsuranceMogulCharacter : CharacterBase
	{
		private const decimal CashBonusPercentage = 0.1M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Insurance Mogul";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => $"This character gets a cash bonus for all stock shares that are lost during a crash and has the ability to short sell stocks.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 6;

		/// <summary>
		/// Whether or not the character gets a vote to push down a stock.
		/// </summary>
		public override bool GetsPushDownVote => true;

		/// <summary>
		/// Whether or not the character gets to short a stock.
		/// </summary>
		public override bool GetsShort => true;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		public override string GetDetailedInformation()
		{
			string shortInfo = "";
			if (ShortPosition != null)
			{
				shortInfo = $"Currently shorting {ShortPosition.SharesAmount} {ShortPosition.StockName}. ";
			}
			return $"{shortInfo}As the Insurance Mogul, you can short sell stocks. This bets against the stock by selling borrowed shares now, and paying them back later (hopefuly at a lower share price). Additionally, if a stock crashes, you are paid {Money(CashBonusPercentage)} for each stock that is lost among all the players.";
		}

		/// <summary>
		/// Calculate the rebate amount this character gets at after a stock crashes.
		/// </summary>
		/// <param name="totalSharesLost">The total shares that were lost.</param>
		/// <returns>The rebate amount.</returns>
		public override int CalculateCrashRebateAmount(int totalSharesLost)
		{
			return (int)(totalSharesLost * CashBonusPercentage);
		}

		#endregion
	}
}
