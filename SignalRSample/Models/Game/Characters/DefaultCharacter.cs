
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The default character that has no special effects.
	/// </summary>
	public class DefaultCharacter : CharacterBase
	{
		#region Properties

		/// <summary>
		/// The name of this character.
		/// </summary>
		public override string Name => "Default";

		/// <summary>
		/// The name of this character.
		/// </summary>
		public override string Description => "The default character.";

		/// <summary>
		/// The id of this character.
		/// </summary>
		public override int Id => 0;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		public override string GetDetailedInformation()
		{
			return "You can buy and sell stocks to try to make a profit.";
		}

		#endregion
	}
}
