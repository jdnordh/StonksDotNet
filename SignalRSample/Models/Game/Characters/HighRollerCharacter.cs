using Models.Game;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The high roller character that gets rebates for buying stock under a certain value.
	/// </summary>
	public class HighRollerCharacter : CharacterBase
	{
		private const decimal SmallRebateMaxValue = 0.95M;
		private const decimal SmallRebateAmount = 0.15M;

		private const decimal BigRebateMaxValue = 0.5M;
		private const decimal BigRebateAmount = 0.3M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "High Roller";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => 
			$"This character gets a {Num(SmallRebateAmount * 100)}% rebate when buying stocks valued {Num(SmallRebateMaxValue * 100)} or under and a {Num(BigRebateAmount * 100)}% rebate when buying stocks valued {Num(BigRebateAmount * 100)} or under, but is payed no divideds.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 4;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		public override decimal GetDivedendAmount(decimal stockValue, decimal originalDiv)
		{
			return 0;
		}

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
				decimal rebatePercentage = 0M;
				if (stock.Value <= BigRebateMaxValue && HoldingChanges[stockName] > 0)
				{
					rebatePercentage = BigRebateAmount;
				}
				else if (stock.Value <= SmallRebateMaxValue && HoldingChanges[stockName] > 0)
				{
					rebatePercentage = SmallRebateAmount;
				}

				decimal cost = HoldingChanges[stockName] * stock.Value;
				rebateAmount += cost * rebatePercentage;
			}
			return (int)rebateAmount;
		}

		#endregion
	}
}
