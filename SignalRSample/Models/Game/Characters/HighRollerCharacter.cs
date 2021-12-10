using Models.Game;
using System;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The high roller character that gets rebates for buying stock under a certain value.
	/// </summary>
	public class HighRollerCharacter : CharacterBase
	{
		private const decimal RebateMaxValue = 0.95M;
		private const decimal DividendSubtraction = 0.1M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "High Roller";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => 
			$"This character is paid less dividends, but gets rebates when buying stocks under par. The rebates become progressively larger the lower the stock value.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 4;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		public override string GetDetailedInformation()
		{
			string preamble = "";
			if(StockValues != null)
			{
				int rebateAmount = CalculateMarketRebateAmount();
				preamble = rebateAmount > 0 ? $"You have qualified for a rebate of ${rebateAmount} this round. " : "You have not qualified for a rebate this round. ";
			}
			return $"{preamble}As the High Roller, you are paid {Num(100 * DividendSubtraction)}% less dividends, but get cash back for every share you buy of a stock below 100. The lower the stock value, the more cash you get back up to a maximum of 95%.";
		}

		/// <inheritdoc/>
		public override decimal GetDivedendAmount(decimal stockValue, decimal originalDiv)
		{
			var divPercentage = originalDiv - DividendSubtraction;
			return divPercentage >= 0 ? divPercentage : 0;
		}

		/// <summary>
		/// Calculate the rebate amount this character gets at the end of an open market.
		/// </summary>
		/// <returns>The rebate amount.</returns>
		public override int CalculateMarketRebateAmount()
		{
			decimal rebateAmount = 0;
			foreach(KeyValuePair<string, Stock> kvp in StockValues)
			{
				var stockName = kvp.Key;
				var stock = kvp.Value;
				if (stock.Value <= RebateMaxValue && HoldingChanges[stockName] > 0)
				{
					decimal cost = HoldingChanges[stockName] * stock.Value;
					decimal rebatePercentage = 1M - stock.Value;
					rebateAmount += cost * rebatePercentage;
				}

			}
			return (int) Math.Round(rebateAmount);
		}

		#endregion
	}
}
