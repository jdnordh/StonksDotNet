using Models.Game;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The high roller character that gets rebates for buying stock under a certain value.
	/// </summary>
	public class HighRollerCharacter : CharacterBase
	{
		private const decimal RebateMaxValue = 0.95M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "High Roller";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => 
			$"This character gets rebates when buying stocks valued {Num(RebateMaxValue * 100)} or under that are progressively bigger the lower the stock value, but is payed no dividends.";

		/// <summary>
		/// The description of this chacter.
		/// </summary>
		public override string DetailedInformation => $"As the High Roller, you are paid no dividends, but get paid cash back for every stock you buy below 100. The lower the stock value, the more cash you get back.";

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
				if (stock.Value <= RebateMaxValue && HoldingChanges[stockName] > 0)
				{
					decimal cost = HoldingChanges[stockName] * stock.Value;
					decimal rebatePercentage = (1M - stock.Value) / 2;
					rebateAmount += cost * rebatePercentage;
				}

			}
			return (int)rebateAmount;
		}

		#endregion
	}
}
