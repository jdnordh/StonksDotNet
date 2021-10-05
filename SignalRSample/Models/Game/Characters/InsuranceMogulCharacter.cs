
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The insurance mogul character that benefits from stocks crashing.
	/// </summary>
	public class InsuranceMogulCharacter : CharacterBase
	{
		private const decimal CashBonusPercentage = 0.25M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Insurance Mogul";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => $"This character gets a cash bonus for all stock shares that are lost during a crash and has the ability to sabotage stocks.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 6;

		/// <summary>
		/// Whether or not the character gets a vote to push down a stock.
		/// </summary>
		public override bool GetsPushDownVote => true;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		public override string GetDetailedInformation()
		{
			return $"As the Insurance Mogul, each round you get to vote for a stock to sabotage. All players who have chosen the Insurance Mogul get a vote. When the market closes, the votes are counted and the stock with the most votes gets an extra down roll that round. Additionally, if a stock crashes, you are paid $0.20 for each stock that is lost among all the players.";
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
