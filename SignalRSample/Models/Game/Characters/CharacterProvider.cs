using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	public static class CharacterProvider
	{
		private readonly static Dictionary<int, CharacterBase> Characters;

		static CharacterProvider()
		{
			Characters = new Dictionary<int, CharacterBase>() 
			{
				{1, new InsideTraderCharacter()},
				{2, new DayTraderCharacter()},
				{3, new HoldMasterCharacter()},
				{4, new HighRollerCharacter()},
				{5, new BulkBuyerCharacter()},
				{6, new InsuranceMogulCharacter()},
			};
		}

		/// <summary>
		/// Get a character given an id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns>A character.</returns>
		public static CharacterBase GetCharacterForId(int id)
		{
			if (!Characters.TryGetValue(id, out CharacterBase value))
			{
				value = new DefaultCharacter();
			}
			return value;
		}

		/// <summary>
		/// Get all characters in a dictionary keyed by their id.
		/// </summary>
		/// <returns>A dictionary populated with all characters.</returns>
		public static Dictionary<int, CharacterBase> GetAllCharacters()
		{
			return Characters;
		}
	}
}
