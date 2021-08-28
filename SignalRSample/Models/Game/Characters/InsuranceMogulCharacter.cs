using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The insurance mogul character that benefits from stocks crashing.
	/// </summary>
	public class InsuranceMogulCharacter : CharacterBase
	{
		private const decimal RebateAmount = 0.5M;
		private const decimal RebateStockValue = 0.1M;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stocks">The stocks in the game.</param>
		public InsuranceMogulCharacter(IEnumerable<string> stocks) : base(stocks)
		{
		}

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Insurance Mogul";

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
			return (int)(totalSharesLost * RebateStockValue * RebateAmount);
		}

		#endregion
	}
}
