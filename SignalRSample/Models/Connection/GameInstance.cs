using Microsoft.AspNetCore.SignalR;
using Models.DataTransferObjects;
using Models.Game;
using SignalRSample.Hubs;
using StonkTrader.Hubs;
using System.Collections.Generic;

namespace StonkTrader.Models.Connection
{
    public static class GameInstance
    {
        private static StonkTraderGame s_game;
        private static string s_observerId;
        private static IHubContext<GameHub> s_hubContext;

		public static string OberserverConnectionId
        {
            get
            {
				return s_observerId;
            }
            set
            {
                s_observerId = value;
            }
		}

        public static StonkTraderGame Game
        {
            get
            {
				return s_game;
            }
        }

        public static void SetHubContext(object context)
        {
            s_hubContext = (IHubContext<GameHub>)context;
        }

        static GameInstance()
        {
            s_game = new StonkTraderGame(new GameInitializer()
			{
				MarketOpenTimeInSeconds = 60,
				RollTimeInSeconds = 7,
				NumberOfRounds = 2,
				RollsPerRound = 2,
				StartingMoney = 5000,
				Stocks = new (string stockName, string color)[]
				{
					("Gold", "#FFD700"),
					("Silver", "#C0C0C0"),
					("Oil", "#4682B4"),
					("Bonds", "#228B22"),
					("Industrial", "#DA70D6"),
					("Grain", "#F0E68C"),
				}
			});

			s_game.PlayerInventoriesUpdated += PlayerInventoriesUpdated;
			s_game.MarketUpdated += GameMarketChanged;
			s_game.GameEnded += GameEnded;
			s_game.Rolled += GameRolled;
		}

		#region Game Event Handlers

		private static async void PlayerInventoriesUpdated(StonkTraderGame senderGame, List<(string id, PlayerInventoryDto inventory)> inventories)
		{
			foreach (var (id, inventory) in inventories)
			{
				await s_hubContext.Clients.Client(id).SendAsync(ClientMethods.InventoryUpdated, inventory);
			}
		}

		private static async void GameMarketChanged(StonkTraderGame senderGame, MarketDto marketDto)
		{
			await s_hubContext.Clients.All.SendAsync(ClientMethods.MarketUpdated, marketDto);
		}

		private static async void GameEnded(StonkTraderGame senderGame, List<(string playerName, int money)> wallets)
		{
			await s_hubContext.Clients.All.SendAsync(ClientMethods.GameEnded, wallets);
		}

		private static async void GameRolled(StonkTraderGame senderGame, MarketDto marketDto)
		{
			await s_hubContext.Clients.Client(GameInstance.OberserverConnectionId).SendAsync(ClientMethods.Rolled, marketDto);
		}

		#endregion

	}
}
