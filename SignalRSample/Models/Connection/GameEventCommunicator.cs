using Microsoft.AspNetCore.SignalR;
using Models.DataTransferObjects;
using Hubs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models.Game;

namespace Models.Connection
{
	/// <summary>
	/// An game event communicator.
	/// </summary>
	public class GameEventCommunicator : IGameEventCommunicator
	{
		private readonly IHubContext<GameHub> m_hubContext;

		public GameEventCommunicator(IHubContext<GameHub> hubContext)
		{
			m_hubContext = hubContext;
		}

		/// <inheritdoc/>
		public async Task PlayerInventoriesUpdated(List<(string connectionId, PlayerInventoryDto inventory)> inventories)
		{
			foreach ((string id, PlayerInventoryDto inventory) in inventories)
			{
				await m_hubContext.Clients.Client(id).SendAsync(ClientMethods.InventoryUpdated, inventory);
			}
		}

		/// <inheritdoc/>
		public async Task GameMarketChanged(MarketDto marketDto)
		{
			await m_hubContext.Clients.All.SendAsync(ClientMethods.MarketUpdated, marketDto);
		}

		/// <inheritdoc/>
		public async Task GameRolled(MarketDto marketDto)
		{
			await m_hubContext.Clients.All.SendAsync(ClientMethods.Rolled, marketDto);
		}

		/// <inheritdoc/>
		public async Task GameOver(GameOverDto gameOverDto)
		{
			await m_hubContext.Clients.All.SendAsync(ClientMethods.GameOver, gameOverDto);
		}
	}
}
