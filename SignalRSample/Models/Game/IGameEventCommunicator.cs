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
		/// <param name="inventoriesDto">Player inventory collection dto.</param>
		Task PlayerInventoriesUpdated(PlayerInventoryCollectionDto inventoriesDto);

		/// <summary>
		/// Send a market changed update.
		/// </summary>
		/// <param name="marketDto">The updated market dto.</param>
		Task GameMarketChanged(MarketDto marketDto);

		/// <summary>
		/// Send a game ended update.
		/// </summary>
		/// <param name="inventoryCollectionDto">Player inventory data.</param>
		/// <param name="messages">Messages to send to players.</param>
		Task GameOver(PlayerInventoryCollectionDto inventoryCollectionDto, Dictionary<string, MessageDto> messages);

		/// <summary>
		/// Send a game rolled update.
		/// </summary>
		/// <param name="marketDto">The updated market dto.</param>
		Task GameRolled(MarketDto marketDto);

		/// <summary>
		/// Send a message to a specific client.
		/// </summary>
		/// <param name="playerId">The player ID.</param>
		/// <param name="message">The message.</param>
		Task SendMessageToPlayer(string playerId, MessageDto message);
	}
}
