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

		private readonly static StockPreset[] PresetArray = new StockPreset[] 
		{
			// The first preset is the default
			new StockPreset()
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
			},
			new StockPreset()
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
			},
			new StockPreset()
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
			},
			new StockPreset()
			{
				Name = "Crypto",
				Stocks = new[]
				{
					new StockDto("Bitcoin", "#f2a900"),
					new StockDto("Ethereum", "#ff5050"),
					new StockDto("Dogecoin", "#00e6e6"),
				}
			},
			new StockPreset()
			{
				Name = "Lord of the Rings",
				Stocks = new[]
				{
					new StockDto("Gondor", "#0086b3"),
					new StockDto("Rohan", "#cc6600"),
					new StockDto("Dwarves", "#339966"),
					new StockDto("Isengard", "#e6e6e6"),
					new StockDto("Mordor", "#ff1800"),
				}
			},
			new StockPreset()
			{
				Name = "Planets",
				Stocks = new[]
				{
					new StockDto("Venus", "#f5a3a3"),
					new StockDto("Earth", "#0f8a0f"),
					new StockDto("Mars", "#c32222"),
					new StockDto("Uranus", "#94e3f2"),
					new StockDto("Neptune", "#3E54E8"),
					new StockDto("Pluto", "#f6ddbd"),
				}
			},
		};

		static StockPresetProvider()
		{
			Presets = new Dictionary<int, StockPreset>();
			for (int i = 0; i < PresetArray.Length; i++)
			{
				var preset = PresetArray[i];
				if (DefaultPreset == null)
				{
					DefaultPreset = preset;
				}
				Presets.Add(i + 1, preset);
			}

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
