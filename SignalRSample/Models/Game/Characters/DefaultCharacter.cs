using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The default character that has no special effects.
	/// </summary>
	public class DefaultCharacter : CharacterBase
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stocks">The stocks in the game.</param>
		public DefaultCharacter(IEnumerable<string> stocks) : base(stocks)
		{
		}

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Default";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 0;

		#endregion
	}
}
