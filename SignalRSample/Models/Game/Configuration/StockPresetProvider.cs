using Models.DataTransferObjects;
using System.Collections.Generic;
using System.Linq;

namespace StonkTrader.Models.Game.Configuration
{
	public static class StockPresetProvider
	{
		private readonly static Dictionary<int, StockPreset> Presets;
		private readonly static StockPreset DefaultPreset;

		public readonly static int MinStockPresetId;
		public readonly static int MaxStockPresetId;

		static StockPresetProvider()
		{
			DefaultPreset = new StockPreset() 
			{
				Name="Modern" ,
				Stocks = new[]
				{
					new StockDto("Property", "#228B22"),
					new StockDto("Oil", "#4682B4"),
					new StockDto("Dogecoin", "#f2b90d"),
					new StockDto("Bonds", "#8724a8"),
					new StockDto("Industry", "#6e6a5f"),
					new StockDto("Tech", "#990000"),
				}
			};

			Presets = new Dictionary<int, StockPreset>()
			{
				{1, DefaultPreset},
				{2, new StockPreset()
					{
						Name = "Classic",
						Stocks = new[]
						{
							new StockDto("Gold", "#FFD700"),
							new StockDto("Silver", "#C0C0C0"),
							new StockDto("Oil", "#4682B4"),
							new StockDto("Bonds", "#228B22"),
							new StockDto("Industrial", "#DA70D6"),
							new StockDto("Grain", "#F0E68C"),
						}
					}
				},
				{3, new StockPreset()
					{
						Name = "Ancient",
						Stocks = new[]
						{
							new StockDto("Stone", "#3d475c"),
							new StockDto("Wood", "#993300"),
							new StockDto("Iron", "#d9d9d9"),
							new StockDto("Water", "#0099ff"),
							new StockDto("Livestock", "#ff6666"),
							new StockDto("Grain", "#F0E68C"),
						}
					}
				},
				{4, new StockPreset()
					{
						Name = "Crypto",
						Stocks = new[]
						{
							new StockDto("Bitcoin", "#f2a900"),
							new StockDto("Ethereum", "#ff5050"),
							new StockDto("Dogecoin", "#00e6e6"),
						}
					}
				},
				{5, new StockPreset()
					{
						Name = "Countries",
						Stocks = new[]
						{
							new StockDto("USA", "#041E42"),
							new StockDto("China", "#C8102E"),
							new StockDto("India", "#FF8F1C"),
							new StockDto("Germany", "#000000"),
							new StockDto("UAE", "#009639"),
						}
					}
				},
				{6, new StockPreset()
					{
						Name = "Planets",
						Stocks = new[]
						{
							new StockDto("Sun", "#FFCC33"),
							new StockDto("Earth", "#68cc25"),
							new StockDto("Mars", "#c1440e"),
							new StockDto("Uranus", "#94e3f2"),
							new StockDto("Neptune", "#3E54E8"),
							new StockDto("Pluto", "#f6ddbd"),
						}
					}
				},
			};

			MinStockPresetId = Presets.Keys.First();
			MaxStockPresetId = Presets.Keys.Last();
		}

		/// <summary>
		/// Get a stock preset given an id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns>The associated stock preset, or the default if not found.</returns>
		public static StockPreset GetPreset(int id)
		{
			if(!Presets.TryGetValue(id, out StockPreset value))
			{
				value = DefaultPreset;
			}
			return value;
		}

		/// <summary>
		/// Gets all stock presets.
		/// </summary>
		/// <returns>A dictionary of all stock presets keyed by id.</returns>
		public static Dictionary<int, StockPreset> GetAllPresets()
		{
			return Presets;
		}
	}
}
