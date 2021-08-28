using Models.Game;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The high roller character that gets rebates for buying stock under a certain value.
	/// </summary>
	public class HighRollerCharacter : CharacterBase
	{
		private const decimal RebateUnderAmount = 0.5M;
		private const decimal RebateAmount = 0.1M;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stocks">The stocks in the game.</param>
		public HighRollerCharacter(IEnumerable<string> stocks) : base(stocks)
		{
		}

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "High Roller";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 4;

		#endregion

		#region Public Methods

		/// <summary>
		/// Calculate the rebate amount this character gets at the end of an open market.
		/// </summary>
		/// <param name="stockValues">The current market stock values.</param>
		/// <returns>The rebate amount.</returns>
		public override int CalculateMarketRebateAmount(Dictionary<string, Stock> stockValues)
		{
			decimal rebateAmount = 0;
			foreach(KeyValuePair<string, Stock> kvp in stockValues)
			{
				var stockName = kvp.Key;
				var stock = kvp.Value;
				if (stock.Value < RebateUnderAmount && m_holdingChanges[stockName] > 0)
				{
					decimal cost = m_holdingChanges[stockName] * stock.Value;
					rebateAmount += cost * RebateAmount;
				}
			}
			return (int)rebateAmount;
		}

		#endregion
	}
}
