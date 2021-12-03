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
					new StockDto("Property", "#026C09"),
					new StockDto("Oil", "#139CB6"),
					new StockDto("Materials", "#691331"),
					new StockDto("Bonds", "#F7434A"),
					new StockDto("Industry", "#6e6a5f"),
					new StockDto("Tech", "#E6C857"),
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
					new StockDto("Bitcoin", "#E6BD00"),
					new StockDto("Litecoin", "#3381D8"),
					new StockDto("Dogecoin", "#68505A"),
					new StockDto("Ethereum", "#308AA2"),
					new StockDto("Tether", "#D6D6D6"),
					new StockDto("Binance", "#52C2A7"),
				}
			},
			new StockPreset()
			{
				Name = "Lord of the Rings",
				Stocks = new[]
				{
					new StockDto("Gondor", "#3AAFE8"),
					new StockDto("Rohan", "#2C93A6"),
					new StockDto("Dwarves", "#8E7A71"),
					new StockDto("Goblins", "#F27953"),
					new StockDto("Isengard", "#e6e6e6"),
					new StockDto("Mordor", "#C32A36"),
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
			new StockPreset()
			{
				Name = "Camping",
				Stocks = new[]
				{
					new StockDto("Canoes", "#702601"),
					new StockDto("Tents", "#3E54E8"),
					new StockDto("Marshmallows", "#e6e6e6"),
					new StockDto("Campfires", "#E63E16"),
					new StockDto("Trees", "#38552C"),
					new StockDto("Mountains", "#3d475c"),
				}
			},
			new StockPreset()
			{
				Name = "Internet",
				Stocks = new[]
				{
					new StockDto("Youtube", "#FF0000"),
					new StockDto("Instagram", "#833AB4"),
					new StockDto("Snapchat", "#FFC300"),
					new StockDto("TikTok", "#25F4EE"),
					new StockDto("Facebook", "#4267B2"),
					new StockDto("Google", "#34a853"),
				}
			},
			new StockPreset()
			{
				Name = "Christmas",
				Stocks = new[]
				{
					new StockDto("Eggnog", "#F5D695"),
					new StockDto("Trees", "#2C6125"),
					new StockDto("Rudolph", "#523B25"),
					new StockDto("Santa", "#9F171A"),
					new StockDto("Presents", "#226677"),
					new StockDto("Snowmen", "#E4F3F3"),
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
