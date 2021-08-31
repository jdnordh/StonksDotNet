using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonkTrader.Models.Game.Characters;
using StonkTrader.Models.Game.Configuration;

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
