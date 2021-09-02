using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonkTrader.Models.Game.Characters;

namespace StonkTrader.Pages.Game
{
	public class CharactersModel : PageModel
	{
		public Dictionary<int, CharacterBase> Characters { get => CharacterProvider.GetAllCharacters(); }

		public void OnGet()
		{
		}
	}
}
