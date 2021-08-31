using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The insurance mogul character that benefits from stocks crashing.
	/// </summary>
	public class InsuranceMogulCharacter : CharacterBase
	{
		private const decimal CashBonusPercentage = 0.2M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Insurance Mogul";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => $"This character gets a {Num(CashBonusPercentage * 100)}% bonus for all players shares that are lost during a stock crash.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 6;

		#endregion

		#region Public Methods

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
