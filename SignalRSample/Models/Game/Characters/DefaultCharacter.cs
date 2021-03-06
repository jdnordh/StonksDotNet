
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The default character that has no special effects.
	/// </summary>
	public class DefaultCharacter : CharacterBase
	{
		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Default";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => "The default character.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 0;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		public override string GetDetailedInformation()
		{
			return "As the default character, you have no special abilities. It would suck to be you right now.";
		}

		#endregion
	}
}
