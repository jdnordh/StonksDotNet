using Models.Game;
using System;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The bulk buyer character gets rebates for buying high stock in high amounts.
	/// </summary>
	public class BulkBuyerCharacter : CharacterBase
	{
		private const int MinimumBuyAmountToRebate = 4000;
		private const decimal RebateAmount = 0.2M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Bulk Buyer";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => $"This character gets cash back when buying large amounts of shares.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 5;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		public override string GetDetailedInformation()
		{
			string preamble = "";
			if (StockValues != null)
			{
				int rebateAmount = CalculateMarketRebateAmount();
				preamble = rebateAmount > 0 ? $"You have qualified for a rebate of ${rebateAmount} this round. " : "You have not qualified for a rebate this round. ";
			}
			return $"{preamble}As the Bulk Buyer, you get {Num(RebateAmount * 100)}% cash back when you spend ${Num(MinimumBuyAmountToRebate)} or more on a single stock. This will happen for each stock on which you spend at least ${Num(MinimumBuyAmountToRebate)}. This is based off of the net holding differences from when the market opens till it closes.";
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
				Stock stock = kvp.Value;
				int holdingChange = HoldingChanges[stockName];
				var cost = holdingChange * stock.Value;
				if (cost >= MinimumBuyAmountToRebate && holdingChange > 0)
				{
					rebateAmount += cost * RebateAmount;
				}
			}
			return (int) Math.Round(rebateAmount);
		}

		#endregion
	}
}
