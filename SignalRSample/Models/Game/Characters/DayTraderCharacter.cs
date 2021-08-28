
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The day trader character that gets to make transactions at half time of the market.
	/// </summary>
	public class DayTraderCharacter : CharacterBase
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stocks">The stocks in the game.</param>
		public DayTraderCharacter(IEnumerable<string> stocks) : base(stocks)
		{
		}

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Day Trader";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 2;

		/// <summary>
		/// Whether or not the character gets half time transactions.
		/// </summary>
		public override bool GetsHalfTimeTransaction => true;

		#endregion
	}
}
