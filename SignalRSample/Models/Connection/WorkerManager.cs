using Models.DataTransferObjects;

namespace StonkTrader.Models.Connection
{
	public class WorkerManager
	{
		#region Singleton Logic

		private static WorkerManager s_instance;
		private static readonly object s_initializerLock = new object();
		public static WorkerManager Instance
		{
			get {
				if (s_instance == null)
				{
					lock (s_initializerLock)
					{
						if (s_instance == null)
						{
							s_instance = new WorkerManager();
						}
					}
				}
				return s_instance;
			}
		}

		private WorkerManager()
		{
			//m_games = new ConcurrentDictionary<string, StonkTraderGame>();
		}

		#endregion

		#region Private Fields

		// TODO Add functionality to have multiple games
		//private ConcurrentDictionary<string, StonkTraderGame> m_games;
		private string m_gameThreadConnectionId;


		#endregion

		#region Methods

		public void SetWorkerConnectionId(string connectionId)
		{
			m_gameThreadConnectionId = connectionId;
		}

		public bool WorkerExists
		{
			get 
			{
				return m_gameThreadConnectionId != null;
			}
		}

		#endregion

		#region Game Key Methods

		// TODO

		#endregion
	}
}
