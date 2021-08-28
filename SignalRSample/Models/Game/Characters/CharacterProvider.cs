using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	public static class CharacterProvider
	{
		/// <summary>
		/// Get a character given an id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="stocks">The stocks in the game.</param>
		/// <returns>A character.</returns>
		public static CharacterBase GetCharacterForId(int id, IEnumerable<string> stocks)
		{
			return id switch
			{
				1 => new InsideTraderCharacter(stocks),
				2 => new DayTraderCharacter(stocks),
				3 => new HoldMasterCharacter(stocks),
				4 => new HighRollerCharacter(stocks),
				5 => new BulkBuyerCharacter(stocks),
				6 => new InsuranceMogulCharacter(stocks),
				_ => new DefaultCharacter(stocks),
			};
		}
	}
}
