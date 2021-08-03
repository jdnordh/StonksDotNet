using Hubs;
using Microsoft.AspNetCore.SignalR;
using Models.Connection;
using Models.Game;

namespace StonkTrader.Models.Connection
{
	public class GameManager
	{
		#region Singleton Logic

		private static GameManager s_instance;
		private static readonly object s_initializerLock = new object();
		private static IHubContext<GameHub> s_hubContext;
		public static GameManager Instance
		{
			get {
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
			get {
				return m_game;
			}
		}

		#endregion

		#region Methods

		public void CreateNewGame(GameInitializer initializer)
		{
			//GameInitializer initializer = isPrototype ? GetPrototypeGameInitializer() : GetDefaultGameInitializer();
			m_game = new StonkTraderGame(initializer, new GameEventCommunicator(s_hubContext));
		}

		public void EndGame()
		{
			if (m_game != null)
			{
				m_game = null;
			}
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
				NumberOfRounds = 7,
				RollsPerRound = 12,
				StartingMoney = 5000,
				IsPrototype = false,
				Stocks = new (string stockName, string color, bool isHalved)[]
				{
					("Gold", "#FFD700", false),
					("Silver", "#C0C0C0", false),
					("Oil", "#4682B4", false),
					("Bonds", "#228B22", false),
					("Industrial", "#DA70D6", false),
					("Grain", "#F0E68C", false),
				}
			};
		}

		public static GameInitializer GetPrototypeGameInitializer()
		{
			// Della config
			return new GameInitializer()
			{
				MarketOpenTimeInSeconds = 90,
				RollTimeInSeconds = 2,
				TimeBetweenRollsInSeconds = 2,
				NumberOfRounds = 7,
				RollsPerRound = 12,
				StartingMoney = 7500,
				IsPrototype = true,
				Stocks = new (string stockName, string color, bool isHalved)[]
				{
					("Dogecoin", "#5cc3f7", false),
					("Crayola", "#ff33cc", false),
					("Twitch", "#6441a5", false),
					("Reddit", "#ff471a", false),
					("Memes", "#98FB98", false),
					("YouTube", "#e60000", false),
				}
			};
			/*
			return new GameInitializer()
			{
				MarketOpenTimeInSeconds = 90,
				RollTimeInSeconds = 2,
				TimeBetweenRollsInSeconds = 2,
				NumberOfRounds = 7,
				RollsPerRound = 12,
				StartingMoney = 7500,
				IsPrototype = true,
				Stocks = new (string stockName, string color, bool isHalved)[]
				{
					("Tech", "#5cc3f7", false),
					("Crypto", "#0df20d", false),
					("Oil", "#005cb3", false),
					("Retail", "#800000", false),
					("Art", "#98FB98", false),
					("Industrial", "#8B008B", false),
					
					("Power", "	#e61919", true),
					("Gold", "#FFD700", true),
					("Bonds", "#4aad18", true),
					("Silver", "#C0C0C0", true),
					("Transport", "#66ffff", true),
					("Grain", "#F0E68C", true),
				}
			};
			*/
		}

		#endregion

		#region Game Key Methods

		// TODO

		#endregion
	}
}
