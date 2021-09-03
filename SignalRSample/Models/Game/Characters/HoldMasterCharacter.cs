
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The base class for characters
	/// </summary>
	public class HoldMasterCharacter : CharacterBase
	{
		private const decimal ExtraDividendPercentage = 0.1M;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Master of the Hold";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => $"This character gets paid {Num(ExtraDividendPercentage * 100)}% more dividends.";

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
		/// <param name="originalDiv">The original dividend amount.</param>
		/// <returns>The adjusted amout.</returns>
		public override decimal GetDivedendAmount(decimal stockValue, decimal originalDiv)
		{
			return originalDiv + ExtraDividendPercentage;
		}

		#endregion
	}
}
