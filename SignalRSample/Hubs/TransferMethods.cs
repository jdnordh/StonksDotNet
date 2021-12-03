
namespace Hubs
{
	public static class ClientMethods
	{
		public const string GameCreated = "gameCreated";
		public const string CreateGameUnavailable = "createGameUnavailable";
		public const string GameJoined = "gameJoined";
		public const string GameJoinedObserver = "gameJoinedObserver";
		public const string GameNotJoined = "gameNotJoined";
		public const string GameStarted = "gameStarted";
		public const string GameOver = "gameOver";
		public const string GameEnded = "gameEnded";
		public const string InventoryUpdated = "inventoryUpdated";
		public const string MarketUpdated = "marketUpdated";
		public const string PlayerInventoriesUpdated = "playerInventoriesUpdated";
		
		public const string TransactionFailed = "transactionFailed";
		public const string Rolled = "rolled";
		public const string RollPreview = "rollPreview";
		public const string TrendPreview = "trendPreview";
		public const string StockPushDown = "stockPushDown";
	}

	public static class GameWorkerRequests
	{
		public const string CreateGameRequest = "createGameRequest";
		public const string JoinGameRequest = "gameJoinRequest";
		public const string ReJoinGameRequest = "gameReJoinRequest";
		public const string StartGameRequest = "startGameRequest";
		public const string TransactionRequest = "transactionRequest";
		public const string RollPreviewRequest = "rollPreviewRequest";
		public const string TrendPreviewRequest = "trendPreviewRequest";
		public const string StockPushDownRequest = "stockPushDownRequest";
		public const string ShortRequest = "shortRequest";
		public const string CoverShortRequest = "coverShortRequest";
		public const string MakePredictionRequest = "makePredictionRequest";
		public const string GameEndRequest = "gameEndRequest";

		public const string ResetRequest = "resetRequest";
	}

	public static class GameWorkerResponses
	{
		public const string JoinGameFailed = "JoinGameFailed";
		public const string PlayerJoinedGameResponse = "GameJoinedPlayer";
		public const string ObserverJoinedGameResponse = "GameJoinedObserver";
		public const string GameCreatedResponse = "GameCreated";
		public const string GameStarted = "GameStarted";

		public const string RollPreviewResponse = "RollPreviewResponse";
		public const string TrendPreviewResponse = "TrendPreviewResponse";
		public const string TransactionPosted = "TransactionPosted";
		public const string MarketUpdated = "MarketUpdated";
		public const string PlayerInventoriesUpdated = "PlayerInventoriesUpdated";
		public const string MarketUpdatedIndividual = "MarketUpdatedIndividual";
		public const string Rolled = "Rolled";

		public const string InventoriesUpdated = "InventoriesUpdated";


		public const string GameOver = "GameOver";
		public const string GameEnded = "GameEnded";

		public const string GameThreadJoined = "GameThreadJoined";

		public const string Key = "nlaP348jp39-2FFW[0-3432.1`~;'";
	}
}
