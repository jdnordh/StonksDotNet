
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

		/// <summary>
		/// The description of this chacter.
		/// </summary>
		public override string DetailedInformation => $"As the default character, you have no special bonuses.";

		#endregion
	}
}
