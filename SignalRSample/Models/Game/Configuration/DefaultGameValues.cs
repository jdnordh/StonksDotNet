
namespace StonkTrader.Models.Game.Configuration
{
	public static class DefaultGameValues
	{
		public static readonly ParameterConfiguration MarketTime = new ParameterConfiguration(10, 300, 60);
		public static readonly ParameterConfiguration Money = new ParameterConfiguration(1000, 10000, 5000);
		public static readonly ParameterConfiguration NumberOfRounds = new ParameterConfiguration(1, 36, 5);
		public static readonly ParameterConfiguration NumberOfRollsPerRound = new ParameterConfiguration(4, 36, 10);
		public static readonly ParameterConfiguration StockPreset = new ParameterConfiguration(StockPresetProvider.MinStockPresetId, 
			StockPresetProvider.MaxStockPresetId, StockPresetProvider.MinStockPresetId);
	}
}
