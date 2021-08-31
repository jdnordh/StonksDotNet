
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The insider trader character that gets special privileges to see the first roll of a round.
	/// </summary>
	public class InsideTraderCharacter : CharacterBase
	{
		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Inside Trader";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => "This character allows you to see the first roll of every round before it is rolled.";

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
