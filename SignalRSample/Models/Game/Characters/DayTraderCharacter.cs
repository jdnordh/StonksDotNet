
namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The day trader character that gets to make transactions at half time of the market.
	/// </summary>
	public class DayTraderCharacter : CharacterBase
	{
		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Day Trader";

		/// <summary> 
		/// The name of this chacter.
		/// </summary>
		public override string Description => "This character can make additional trades half way through a closed market and gets information about trends.";

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
