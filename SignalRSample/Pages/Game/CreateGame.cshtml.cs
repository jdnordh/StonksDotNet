using Microsoft.AspNetCore.Mvc.RazorPages;
using StonkTrader.Models.Game.Configuration;

namespace StonkTrader.Pages.Game
{
	public class CreateGameModel : PageModel
	{
		public ParameterConfiguration MarketTime => DefaultGameValues.MarketTime;
		public ParameterConfiguration Money => DefaultGameValues.Money;
		public ParameterConfiguration NumberOfRounds => DefaultGameValues.NumberOfRounds;
		public ParameterConfiguration NumberOfRollsPerRound => DefaultGameValues.NumberOfRollsPerRound;
		public ParameterConfiguration StockPreset => DefaultGameValues.StockPreset;

		public void OnGet()
		{
		}
	}
}
