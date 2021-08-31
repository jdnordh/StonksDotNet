﻿using Models.Game;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The bulk buyer character tha gets rebates for buying high stock in high amounts.
	/// </summary>
	public class BulkBuyerCharacter : CharacterBase
	{
		private const decimal RebateMinValue = 1M;
		private const int StockAmountToRebate = 4000;
		private const decimal RebateAmount = 0.2M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Bulk Buyer";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => $"This character gets a {Num(RebateAmount * 100)}% rebate on all buys of more than {Num(StockAmountToRebate)} shares on stocks valued {Num(RebateMinValue * 100)} or over.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 5;

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
				if (stock.Value >= RebateMinValue && m_holdingChanges[stockName] > StockAmountToRebate)
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
