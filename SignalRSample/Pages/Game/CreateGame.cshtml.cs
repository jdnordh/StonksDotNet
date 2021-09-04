using Microsoft.AspNetCore.Mvc.RazorPages;
using StonkTrader.Models.Game.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace StonkTrader.Pages.Game
{
	public class CreateGameModel : PageModel
	{
		public ParameterConfiguration MarketTime => DefaultGameValues.MarketTime;
		public ParameterConfiguration Money => DefaultGameValues.Money;
		public ParameterConfiguration NumberOfRounds => DefaultGameValues.NumberOfRounds;
		public ParameterConfiguration NumberOfRollsPerRound => DefaultGameValues.NumberOfRollsPerRound;
		public ParameterConfiguration StockPreset => DefaultGameValues.StockPreset;
		public List<(int id, int count, string name)> StockPresetNames =>
			StockPresetProvider.GetAllPresets().Select(kvp => (kvp.Key, kvp.Value.Stocks.Length, kvp.Value.Name)).ToList();

		public string DefaultStockName => StockPresetProvider.GetPreset(DefaultGameValues.StockPreset.DefaultValue).Name;
		public int DefaultStockCount => StockPresetProvider.GetPreset(DefaultGameValues.StockPreset.DefaultValue).Stocks.Length;

		public void OnGet()
		{
		}
	}
}
