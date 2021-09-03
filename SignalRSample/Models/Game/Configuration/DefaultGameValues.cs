
namespace StonkTrader.Models.Game.Configuration
{
	public static class DefaultGameValues
	{
		public readonly static ParameterConfiguration MarketTime = new ParameterConfiguration(10, 300, 60);
		public readonly static ParameterConfiguration Money = new ParameterConfiguration(1000, 10000, 5000);
		public readonly static ParameterConfiguration NumberOfRounds = new ParameterConfiguration(1, 36, 5);
		public readonly static ParameterConfiguration NumberOfRollsPerRound = new ParameterConfiguration(2, 36, 10);
		public readonly static ParameterConfiguration StockPreset = new ParameterConfiguration(StockPresetProvider.MinStockPresetId, 
			StockPresetProvider.MaxStockPresetId, StockPresetProvider.MinStockPresetId);
	}
}
