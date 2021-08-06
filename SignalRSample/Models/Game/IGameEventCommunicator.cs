using Models.DataTransferObjects;
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
		/// <param name="gameOverDto">Game over data.</param>
		Task GameOver(GameOverDto gameOverDto);

		/// <summary>
		/// Send a game rolled update.
		/// </summary>
		/// <param name="marketDto">The updated market dto.</param>
		Task GameRolled(MarketDto marketDto);
	}
}
