using Models.DataTransferObjects;

namespace StonkTrader.Models.Game.Configuration
{
	public class StockPreset
	{
		public StockDto[] Stocks { get; set; }
		public string Name { get; set; }
	}
}
