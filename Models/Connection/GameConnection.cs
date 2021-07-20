using Models.StockTicker.GameClasses;
using System.Collections.Generic;

namespace StockTickerDotNetCore.Models.Connection
{
	public class GameConnection
	{
		/// <summary>
		/// The name of the creator of this game.
		/// </summary>
		public string HostClientName { get; }

		/// <summary>
		/// The names of players in this game.
		/// </summary>
		public List<string> PlayerNames { get; }

		/// <summary>
		/// The game.
		/// </summary>
		public StonkTraderGame Game { get; }

		public GameConnection(StonkTraderGame game, string hostClientName)
		{
			Game = game;
			HostClientName = hostClientName;
			PlayerNames = new List<string>();
		}
	}
}
