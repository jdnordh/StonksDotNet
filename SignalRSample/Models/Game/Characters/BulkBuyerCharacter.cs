using Models.Game;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The bulk buyer character tha gets rebates for buying high stock in high amounts.
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
		public override string Description => $"This character gets a {Num(RebateAmount * 100)}% rebate on all buys of ${Num(MinimumBuyAmountToRebate)} or more.";

		/// <summary>
		/// The description of this chacter.
		/// </summary>
		public override string DetailedInformation => $"As the Bulk Buyer, you get {Num(RebateAmount * 100)}% cash back when you spend ${Num(MinimumBuyAmountToRebate)} or more on a single stock. This is based off of the net difference from when the market opens till it closes, so buying ${Num(MinimumBuyAmountToRebate)} and then selling it in the same market time will not give a rebate.";

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
				Stock stock = kvp.Value;
				int holdingChange = HoldingChanges[stockName];
				var cost = holdingChange * stock.Value;
				if (cost >= MinimumBuyAmountToRebate && holdingChange > 0)
				{
					rebateAmount += cost * RebateAmount;
				}
			}
			return (int)rebateAmount;
		}

		#endregion
	}
}
