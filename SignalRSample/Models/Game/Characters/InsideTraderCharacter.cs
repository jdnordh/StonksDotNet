using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The insider trader character that gets special privileges to see the first roll of a round.
	/// </summary>
	public class InsideTraderCharacter : CharacterBase
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stocks">The stocks in the game.</param>
		public InsideTraderCharacter(IEnumerable<string> stocks) : base(stocks)
		{
		}

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Inside Trader";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 1;

		/// <summary>
		/// Whether or not the character gets a reveal of the first roll of each round.
		/// </summary>
		public override bool GetsFirstRollReveal => true;

		#endregion
	}
}
