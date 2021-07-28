using Microsoft.AspNetCore.SignalR;
using Models.Game;
using Hubs;
using Models.Connection;

namespace StonkTrader.Models.Connection
{
	public class GameManager
	{
		#region Singleton Logic

		private static GameManager s_instance;
		private static object s_initializerLock = new object();
		private static IHubContext<GameHub> s_hubContext;
		public static GameManager Instance
		{
			get
			{
				if (s_instance == null)
				{
					lock (s_initializerLock)
					{
						if (s_instance == null)
						{
							s_instance = new GameManager();
						}
					}
				}
				return s_instance;
			}
		}

		private GameManager()
		{
			//m_games = new ConcurrentDictionary<string, StonkTraderGame>();
		}

		/// <summary>
		/// Set the hub context.
		/// </summary>
		/// <param name="hubContext">The hub context.</param>
		public static void SetHubContext(object hubContext)
		{
			s_hubContext = (IHubContext<GameHub>)hubContext;
		}

		#endregion

		#region Private Fields

		// TODO Add functionality to have multiple games
		//private ConcurrentDictionary<string, StonkTraderGame> m_games;
		private StonkTraderGame m_game;


		#endregion

		#region Properties

		public StonkTraderGame Game 
		{ 
			get 
			{
				return m_game;
			} 
		}

		#endregion

		#region Methods

		public void CreateNewGame()
		{
			m_game = new StonkTraderGame(GetDefaultGameInitializer(), new GameEventCommunicator(s_hubContext));
		}

		#endregion

		#region Game Defaults

		public static GameInitializer GetDefaultGameInitializer()
		{
			return new GameInitializer()
			{
				MarketOpenTimeInSeconds = 60,
				RollTimeInSeconds = 2,
				TimeBetweenRollsInSeconds = 2,
				NumberOfRounds = 2,
				RollsPerRound = 12,
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
			};
		}

		#endregion

		#region Game Key Methods

		// TODO

		#endregion
	}
}
