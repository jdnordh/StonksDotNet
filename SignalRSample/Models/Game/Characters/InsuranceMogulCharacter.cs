
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The insurance mogul character that benefits from stocks crashing.
	/// </summary>
	public class InsuranceMogulCharacter : CharacterBase
	{
		private const decimal CashBonusPercentage = 0.15M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Insurance Mogul";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => $"This character can short sell stocks and gets a cash bonus for shares lost in a stock crash. WARNING: This is an advanced character.";

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
			return $"{shortInfo}As the Insurance Mogul, you can short sell stocks which is like betting against it. This works by selling borrowed shares now and paying them back later (hopefuly at a lower share price). Additionally, if a stock crashes, you are paid {Money(CashBonusPercentage)} for each share that you lose.";
		}

		/// <summary>
		/// Calculate the rebate amount this character gets at after a stock crashes.
		/// </summary>
		/// <param name="sharesLost">The shares that were lost by the player.</param>
		/// <returns>The rebate amount.</returns>
		public override int CalculateCrashRebateAmount(int sharesLost)
		{
			return (int)(sharesLost * CashBonusPercentage);
		}

		#endregion
	}
}
