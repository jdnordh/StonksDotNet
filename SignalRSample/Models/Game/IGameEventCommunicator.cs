using Models.DataTransferObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Models.Game
{
	/// <summary>
	/// An interface to send game events to clients.
	/// </summary>
	public interface IGameEventCommunicator
	{
		/// <summary>
		/// Send an inventory update.
		/// </summary>
		/// <param name="inventories">Player connection ID's connected to their inventory.</param>
		Task PlayerInventoriesUpdated(List<(string connectionId, PlayerInventoryDto inventory)> inventories);

		/// <summary>
		/// Send a market changed update.
		/// </summary>
		/// <param name="marketDto">The updated market dto.</param>
		Task GameMarketChanged(MarketDto marketDto);

		/// <summary>
		/// Send a game ended update.
		/// </summary>
		/// <param name="wallets">Player connection ID's connected to their money amount.</param>
		Task GameEnded(List<(string playerName, int money)> wallets);

		/// <summary>
		/// Send a game rolled update.
		/// </summary>
		/// <param name="marketDto">The updated market dto.</param>
		Task GameRolled(MarketDto marketDto);
	}
}
