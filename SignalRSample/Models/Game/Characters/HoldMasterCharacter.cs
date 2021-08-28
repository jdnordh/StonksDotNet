using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The base class for characters
	/// </summary>
	public class HoldMasterCharacter : CharacterBase
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stocks">The stocks in the game.</param>
		public HoldMasterCharacter(IEnumerable<string> stocks) : base(stocks)
		{
		}

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Master of the Hold";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 3;

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the divedend amount for this character.
		/// </summary>
		/// <param name="stockValue">The value of the stock that is dividending.</param>
		/// <param name="ogirnalDiv">The original dividend amount.</param>
		/// <returns>The adjusted amout.</returns>
		public override decimal GetDivedendAmount(decimal stockValue, decimal originalDiv)
		{
			return originalDiv + 0.05M;
		}

		#endregion
	}
}
