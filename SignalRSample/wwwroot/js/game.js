// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

"use strict";

var clickHandler;
var globalIsPrototype = false;

var log = function (msg) {
	console.log(msg);
};

//#region Cookies

var Cookie = {
	Cookies: {
		PlayerId: "playerid",
	},
	CreateCookie: function (name, value, days) {
		if (days) {
			var date = new Date();
			date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
			var expires = "; expires=" + date.toGMTString();
		} else {
			var expires = "";
		}
		document.cookie = name + "=" + value + expires + "; path=/";
	},
	GetCookieValue: function (cookieName) {
		var matched = document.cookie.match("(^|[^;]+)\\s*" + cookieName + "\\s*=\\s*([^;]+)");
		return matched ? matched.pop() : "";
	},
	DeleteCookie: function (cookieName) {
		createCookie(cookieName, "", -1);
	},
};

//#endregion

var Config = {
	InventoryMarketChartSwitchTime: 10000,
	CashColor: "#85bb65",
	DividendLabelAngle: '-20',
	DividendLabelColor: 'rgba(0, 0, 0, 0.15)',
};

var GameAudio = {
	Up: undefined,
	Down: undefined,
	Div: undefined,
	NoDiv: undefined,
	Split: undefined,
	Crash: undefined,
	Init: function () {
		GameAudio.Up = new Audio('audio/up.mp3');
		GameAudio.Down = new Audio('audio/down.mp3');
		GameAudio.Div = new Audio('audio/div.mp3');
		GameAudio.NoDiv = new Audio('audio/nodiv.mp3');
		GameAudio.Split = new Audio('audio/split.mp3');
		GameAudio.Crash = new Audio('audio/crash.mp3');
	},
	PlayAudio: function (audio) {
		if (!audio) {
			return;
		}
		if (audio.paused) {
			audio.play();
		}
		else {
			audio.pause();
			audio.currentTime = 0
			audio.play();
		}
	},
};

var Unit = {
	PercentToDecimal: function (percentage) {
		return percentage / 100;
	},
	PercentToDecimalString: function (percentage) {
		return (percentage / 100).toLocaleString(undefined, { minimumFractionDigits: 2 });
	},
	FloatEquals: function (lhs, rhs) {
		let epsilon = 0.000001;
		return Math.abs(lhs - rhs) < epsilon;
	},
};

var CurrentData = {
	Username: "StonkMaster",
	Holdings: { },
	StockValues: { },
	StockColors: { },
	StockHalves: {},
	PlayerInventories: {},
	Money: 0,
	CharacterId: 0,
};

var Connection = {
	ClientType: -1,
	ClientTypes: {
		None: -1,
		Player: 1,
		Observer: 2,
		Creator: 3,
	},
	ClientMethods: {
		GameCreated: "gameCreated",
		CreateGameUnavailable: "createGameUnavailable",
		GameJoined: "gameJoined",
		GameNotJoined: "gameNotJoined",
		GameJoinedObserver: "gameJoinedObserver",
		GameStarted: "gameStarted",
		GameOver: "gameOver",
		GameEnded: "gameEnded",
		InventoryUpdated: "inventoryUpdated",
		MarketUpdated: "marketUpdated",
		PlayerInventoriesUpdated: "playerInventoriesUpdated",
		TransactionFailed: "transactionFailed",
		Rolled: "rolled",
		RollPreview: "rollPreview",
		TrendPreview: "trendPreview",
	},
	ServerMethods: {
		CreateGame: "CreateGame",
		JoinGame: "JoinGame",
		StartGame: "StartGame",
		EndGame: "EndGame",
		RequestTransaction: "RequestTransaction",
		RequestRollPreview: "RequestRollPreview",
		RequestTrendPreview: "RequestTrendPreview",
		RequestStockPushDown: "RequestStockPushDown",
		Reset: "Reset",
		ReJoin: "ReJoin",
	},
	Reset: function () {
		Connection.Hub.invoke(Connection.ServerMethods.Reset).catch(function (err) {
			return console.error(err.toString());
		});
	},
	Init: function () {
		Connection.Hub = new signalR.HubConnectionBuilder()
			.withUrl("/gameHub")
			.withAutomaticReconnect()
			.build();
		Connection.Hub.serverTimeoutInMilliseconds = 1800000;

		Connection.Hub.onreconnecting(function (error) {
			log('Reconnecting...');
		});

		Connection.Hub.on(Connection.ClientMethods.GameCreated, function () {
			Connection.ClientType = Connection.ClientTypes.Creator;
			ScreenOps.SwitchToStartGameMenu();
		});

		Connection.Hub.on(Connection.ClientMethods.GameJoined, function (playerInventoryDto) {
			log('Game Joined');
			// Save player id
			Cookie.CreateCookie(Cookie.Cookies.PlayerId, playerInventoryDto.playerId, 1);

			Connection.ClientType = Connection.ClientTypes.Player;
			Connection.UpdateInventory(playerInventoryDto);
			CurrentData.Username = playerInventoryDto.username;
			ScreenOps.SwitchToWaitingMenu();
		});

		Connection.Hub.on(Connection.ClientMethods.GameJoinedObserver, function (marketDto) {
			log('Game Joined - Observer');
			Connection.ClientType = Connection.ClientTypes.Observer;
			Connection.UpdateMarketValues(marketDto);
			Presenter.CreateCharts();
		});

		Connection.Hub.on(Connection.ClientMethods.CreateGameUnavailable, function (message) {
			log('Create game unavailable: ' + message);
			$(ConstHtmlIds.CreateGame).prop('disabled', true);
		});

		Connection.Hub.on(Connection.ClientMethods.GameNotJoined, function (message) {
			log('Join game unavailable: ' + message);
			Cookie.DeleteCookie(Cookie.Cookies.PlayerId);
		});

		Connection.Hub.on(Connection.ClientMethods.GameStarted, function () {
			log('Game started');
			if (Connection.ClientType === Connection.ClientTypes.Observer || Connection.ClientType === Connection.ClientTypes.Creator) {
				Presenter.CreateCharts();
			}
		});

		Connection.Hub.on(Connection.ClientMethods.InventoryUpdated, function (playerInventoryDto) {
			log('Inventory updated');
			if (Connection.ClientType === Connection.ClientTypes.Player) {
				Connection.UpdateInventory(playerInventoryDto);
			}
		});

		Connection.Hub.on(Connection.ClientMethods.PlayerInventoriesUpdated, function (marketDto) {
			log('Player inventories updated');
			if (Connection.ClientType === Connection.ClientTypes.Observer || Connection.ClientType === Connection.ClientTypes.Creator) {
				Connection.UpdatePlayerInventories(marketDto.playerInventories);
				Presenter.UpdateInventoryChart();
			}
		});

		Connection.Hub.on(Connection.ClientMethods.MarketUpdated, function (marketDto) {
			log('Market updated');
			Connection.UpdateMarketValues(marketDto);
			CurrentData.IsHalfTime = marketDto.isHalfTime;
			if (Connection.ClientType === Connection.ClientTypes.Observer || Connection.ClientType === Connection.ClientTypes.Creator) {
				Connection.UpdatePlayerInventories(marketDto.playerInventories);
				Presenter.UpdateChart();
				Presenter.UpdateInventoryChart();
				if (marketDto.isOpen) {
					let marketEndTime = Number(marketDto.marketCloseTimeInMilliseconds);
					Presenter.SetMarketOpen(marketEndTime, marketDto.currentRound, marketDto.totalRounds, marketDto.isHalfTime);
				}
				else {
					Presenter.SetMarketClosed();
				}
			}
			else if (Connection.ClientType === Connection.ClientTypes.Player) {
				let canParticipateInHalfTimeMarket = CurrentData.CharacterId === 2;
				if ((marketDto.isOpen && !marketDto.isHalfTime) ||
					(marketDto.isOpen && marketDto.isHalfTime && canParticipateInHalfTimeMarket)) {
					ScreenOps.SwitchToOpenMarket(marketDto);
				}
				else {
					ScreenOps.SwitchToClosedMarket();
				}
			}
		});

		Connection.Hub.on(Connection.ClientMethods.Rolled, function (marketDto) {
			log('Rolled');
			if (Connection.ClientType === Connection.ClientTypes.Observer || Connection.ClientType === Connection.ClientTypes.Creator) {
				Connection.UpdateStockValuesFromRoll(marketDto.rollDto);
				Presenter.ShowRoll(marketDto.rollDto);
			}
		});

		Connection.Hub.on(Connection.ClientMethods.RollPreview, function (rollDto) {
			log('Roll preview');
			// Remove button
			$(ConstHtmlIds.RollPreviewButton).remove();

			// Add roll preview
			let stockList = $(ConstHtmlIds.StockList);
			stockList.prepend(HtmlGeneration.MakeRollPreview(rollDto));
		});

		Connection.Hub.on(Connection.ClientMethods.TrendPreview, function (trendDto) {
			log('Trend preview');
			// Remove button
			log(trendDto);
			$(ConstHtmlIds.TrendPreviewButton).remove();

			// Add roll preview
			let stockList = $(ConstHtmlIds.StockList);
			stockList.prepend(HtmlGeneration.MakeTrendPreview(trendDto));
		});

		Connection.Hub.on(Connection.ClientMethods.GameEnded, function () {
			log('Game ended');
			Presenter.IsInitialized = false;
			if (Presenter.Chart) {
				Presenter.Chart.destroy();
				Presenter.Chart = undefined;
			}
			if (Presenter.InventoryChart) {
				Presenter.InventoryChart.destroy();
				Presenter.InventoryChart = undefined;
			}
			CurrentData = {
				Username: "StonkMaster",
				Holdings: {},
				StockValues: {},
				StockColors: {},
				StockHalves: {},
				PlayerInventories: {},
				Money: 0,
			};
			ScreenOps.SwitchToMainMenu();
		});

		Connection.Hub.on(Connection.ClientMethods.GameOver, function (inventoryCollectionDto) {
			log('Game Over');
			if (Connection.ClientType === Connection.ClientTypes.Observer || Connection.ClientType === Connection.ClientTypes.Creator) {
				Presenter.SetGameOver();
				Connection.UpdatePlayerInventories(inventoryCollectionDto);
				Presenter.UpdateInventoryChart();
				if (Connection.ClientType === Connection.ClientTypes.Creator) {
					ScreenOps.ShowEndGameButton();
				}
			}
			else {
				ScreenOps.SwitchToGameOver();
			}
		});

		Connection.Hub.start().then(function () {
			Connection.TryReJoinGame();

			ScreenOps.SwitchToMainMenu();
		}).catch(function (err) {
			return console.error(err.toString());
		});
	},
	TryReJoinGame: function () {
		let playerId = Cookie.GetCookieValue(Cookie.Cookies.PlayerId);
		if (playerId) {
			Connection.Hub.invoke(Connection.ServerMethods.ReJoin, playerId).catch(function (err) {
				return console.error(err.toString());
			});
		}
	},
	UpdateMarketValues: function (marketDto) {
		// Update stock values
		for (let stockName in marketDto.stocks) {
			if (marketDto.stocks.hasOwnProperty(stockName)) {
				let stockDto = marketDto.stocks[stockName];
				CurrentData.StockValues[stockName] = stockDto.value;
				CurrentData.StockColors[stockName] = stockDto.color;
				CurrentData.StockHalves[stockName] = stockDto.isHalved;
			}
		}
	},
	UpdatePlayerInventories: function (inventoryDto) {
		// Update player inventories
		for (let id in inventoryDto.inventories) {
			if (inventoryDto.inventories.hasOwnProperty(id)) {
				let inventory = inventoryDto.inventories[id];
				CurrentData.PlayerInventories[id] = {
					username: inventory.username,
					money: inventory.money,
					holdings: {}
				};

				for (let stockName in inventory.holdings) {
					if (inventory.holdings.hasOwnProperty(stockName)) {
						let amountHeld = inventory.holdings[stockName];
						CurrentData.PlayerInventories[id].holdings[stockName] = amountHeld;
					}
				}
			}
		}
	},
	UpdateStockValuesFromRoll: function (rollDto) {
		if (rollDto.func === 'Up') {
			CurrentData.StockValues[rollDto.stockName] += rollDto.amount;
			if (CurrentData.StockValues[rollDto.stockName] > 200) {
				CurrentData.StockValues[rollDto.stockName] = 200;
			}
		}
		else if (rollDto.func === 'Down') {
			CurrentData.StockValues[rollDto.stockName] -= rollDto.amount;
			if (CurrentData.StockValues[rollDto.stockName] < 0) {
				CurrentData.StockValues[rollDto.stockName] = 0;
			}
		}
	},
	UpdateInventory: function (playerInventoryDto) {
		for (let stockName in playerInventoryDto.holdings) {
			if (playerInventoryDto.holdings.hasOwnProperty(stockName)) {
				let amount = playerInventoryDto.holdings[stockName];
				CurrentData.Holdings[stockName] = amount;
			}
		}
		CurrentData.CharacterId = playerInventoryDto.characterId;
		Connection.SetMoney(playerInventoryDto.money);
		Connection.OnServerUpdate();
	},
	RequestTransaction: function (stockName, isBuy, amount) {
		Connection.Hub.invoke(Connection.ServerMethods.RequestTransaction, stockName, isBuy, Number(amount)).catch(function (err) {
			return console.error(err.toString());
		});
	},
	RequestRollPreview: function () {
		Connection.Hub.invoke(Connection.ServerMethods.RequestRollPreview).catch(function (err) {
			return console.error(err.toString());
		});
	},
	RequestTrendPreview: function () {
		Connection.Hub.invoke(Connection.ServerMethods.RequestTrendPreview).catch(function (err) {
			return console.error(err.toString());
		});
	},
	RequestStockPushDown: function (stockName) {
		Connection.Hub.invoke(Connection.ServerMethods.RequestStockPushDown, stockName).catch(function (err) {
			return console.error(err.toString());
		});
	},
	BuyStock: function (stockName, amount) {
		Connection.RequestTransaction(stockName, true, amount);
	},
	SellStock: function (stockName, amount) {
		Connection.RequestTransaction(stockName, false, amount);
	},
	CreateGame: function (CreateGameParams) {
		Connection.Hub.invoke(Connection.ServerMethods.CreateGame, CreateGameParams).catch(function (err) {
			return console.error(err.toString());
		});
	},
	JoinGame: function (username, characterId) {
		log('Joining game as ' + username);
		Connection.Hub.invoke(Connection.ServerMethods.JoinGame, username, true, characterId).catch(function (err) {
			return console.error(err.toString());
		});
	},
	JoinGameObserver: function () {
		log('Joining game as Observer');
		Connection.Hub.invoke(Connection.ServerMethods.JoinGame, "", false, 0).catch(function (err) {
			return console.error(err.toString());
		});
	},
	StartGame: function () {
		Connection.Hub.invoke(Connection.ServerMethods.StartGame).catch(function (err) {
			return console.error(err.toString());
		});
	},
	EndGame: function () {
		Connection.Hub.invoke(Connection.ServerMethods.EndGame).catch(function (err) {
			return console.error(err.toString());
		});
	},
	OnServerUpdate: function () {
		let updateMethod = ScreenOps.StateBuildMethod[ScreenOps.State];
		if (updateMethod) {
			updateMethod();
		}
	},
	SetMoney: function (money) {
		CurrentData.Money = money;
		$(ConstHtmlIds.Money).text('$' + money);
	},
};

var ConstHtmlIds =
{
	Money: "#money",
	BuyTab: "#buyTab",
	SellTab: "#sellTab",
	TabActive: "btn-primary",
	TabInactive: "btn-outline-primary",
	StockList: "#stockList",
	MainGrid: "#mainGrid",
	PreBuy: "#preBuy",
	PreSell: "#preSell",
	MarketOpenClosedHeader: "#marketOpenClosedHeader",
	Buy: "#buy",
	BuyAmount: "#buyAmount",
	Sell: "#sell",
	SellAmount: "#sellAmount",
	Cancel: "#cancel",
	CreateGame: "#createGame",
	JoinGame: "#joinGame",
	WatchGame: "#watchGame",
	Username: "#username",
	StartGame: "#startGame",
	PresenterChart: "presenterChart", // No hash in front because it's used by chartjs, not jquery
	InventoryChart: "inventoryChart", // No hash in front because it's used by chartjs, not jquery
	PresenterText: "#presenterText",
	RollName: "#rollName",
	RollFunc: "#rollFunc",
	RollAmount: "#rollAmount",
	EndGameButton: '#endGameButton',
	CreateGameWithParameters: "#createGameWithParameters",
	ParamMarketOpenTime: "#marketOpenTime",
	ParamStartingMoney: "#startingMoney",
	ParamRollsPerRound: "#rollsPerRound",
	ParamRounds: "#rounds",
	ParamStockPresets: "#stockPresets",
	BuySellTimer: "#buySellTimer",
	IsPlayer: "#isPlayer",
	ChartSlideContainer: "#chart-slide-container",
	PresenterChartSlider: "#presenterChartSlider",
	InventoryChartSlider: "#inventoryChartSlider",
	SelectCharacter: "#selectCharacter",
	RollPreviewButton: "#rollPreviewBtn",
	TrendPreviewButton: "#trendPreviewBtn",
	PushDownButton: "#pushDownBtn",
	PushDownSendButton: "#pushDownSendBtn",
	PushDownValue: "#pushDownValue",
	BackButton: "#backButton",
}

var HtmlGeneration =
{
	MakePreBuyScreen: function (stockName) {
		let stockValue = CurrentData.StockValues[stockName];
		let money = CurrentData.Money;
		let html = '<div class="fill grid-row-2"><p class="buy-sell-prompt">How much ';
		html += stockName;
		html += ' (';
		html += Unit.PercentToDecimalString(stockValue);
		html += ') would you like to buy?</p><div class="buy-sell-control"><select class="buy-sell-prompt grid-column-2 grid-row-1 fill" name="amount" id="buyAmount">';
		let maxBuyAmount = (money * 10000) / (stockValue * 100);
		let initialBuyFor = 0;
		let buyAmounts = [];
		for (let i = 500; i <= maxBuyAmount; i += 500) {
			buyAmounts.push(i);
		}
		for (let i = buyAmounts.length - 1; i >= 0; i--) {
			let buyAmount = buyAmounts[i];
			if (!initialBuyFor) {
				initialBuyFor = (buyAmount * stockValue) / 100;
			}
			html += '<option value="';
			html += buyAmount;
			html += '">'
			html += buyAmount;
			html += '</option>';
		}
		html += '</select><button class="btn btn-success buy-sell-button fill grid-column-2 grid-row-2" id="buy">Buy for $';
		html += initialBuyFor;
		html += '</button ><button class="btn btn-outline-danger buy-sell-button fill grid-column-2 grid-row-3" id="cancel">Cancel</button></div></div>';
		return html;
	},
	MakePreSellScreen: function (stockName) {
		let stockAmount = CurrentData.Holdings[stockName];
		let stockValue = CurrentData.StockValues[stockName];
		let html = '<div class="fill grid-row-2"><p class="buy-sell-prompt">How much ';
		html += stockName;
		html += ' (';
		html += Unit.PercentToDecimalString(stockValue);
		html += ') would you like to sell?</p><div class="buy-sell-control"><select class="buy-sell-prompt grid-column-2 grid-row-1 fill" name="amount" id="sellAmount">';
		let initialSellFor = (stockAmount * stockValue) / 100;
		for (let i = stockAmount; i > 0; i -= 500) {
			html += '<option value="';
			html += i;
			html += '">'
			html += i;
			html += '</option>';
		}
		html += '</select><button class="btn btn-info buy-sell-button fill grid-column-2 grid-row-2" id="sell">Sell for $';
		html += initialSellFor;
		html += '</button><button class="btn btn-outline-danger buy-sell-button fill grid-column-2 grid-row-3" id="cancel">Cancel</button></div></div>';
		return html;
	},
	MakePrePushDownScreen: function () {
		let html = '<div class="fill grid-row-2"><p class="buy-sell-prompt">Choose the stock you want to sabotage.</p><div class="buy-sell-control"><select class="buy-sell-prompt grid-column-2 grid-row-1 fill" name="amount" id="pushDownValue">';
		for (let stockName in CurrentData.Holdings) {
			if (CurrentData.Holdings.hasOwnProperty(stockName)) {
				html += '<option value="';
				html += stockName;
				html += '">'
				html += stockName;
				html += '</option>';
			}
		}
		html += '</select><button class="btn btn-danger buy-sell-button fill grid-column-2 grid-row-2" id="pushDownSendBtn">Sabotage</button><button class="btn btn-outline-danger buy-sell-button fill grid-column-2 grid-row-3" id="cancel">Cancel</button></div></div>';
		return html;
	},
	MakeWaitingScreen: function (username, money) {
		let html = '<div class="grid-row-1 grid-fill buy-sell-div"><div class="grid-row-1"><p class="market-closed">';
		html += username;
		html += '</p></div><div class="grid-row-2"><p id="money">$'
		html += money;
		html += '</p></div></div>';
		return html;
	},
	MakeMarketClosedScreen: function (money) {
		let html = '<div class="grid-row-1 grid-fill buy-sell-div"><div class="grid-row-1"><p class="market-closed" id="marketOpenClosedHeader">Market Closed</p></div><div class="grid-row-2"><p id="money">$'
		html += money;
		html += '</p></div></div><div class="grid-row-2 scrollviewer-vertical" id="stockList"></div>';
		return html;
	},
	MakeGameOverScreen: function (money) {
		let html = '<div class="grid-row-1 grid-fill buy-sell-div"><div class="grid-row-1"><p class="market-closed" id="marketOpenClosedHeader">Game Over</p></div><div class="grid-row-2"><p id="money">$'
		html += money;
		html += '</p></div></div><div class="grid-row-2 scrollviewer-vertical" id="stockList"></div>';
		return html;
	},
	MakeMarketScreen: function (money, isBuy) {
		let html = '<div class="grid-row-1 grid-fill buy-sell-div"><div class="grid-row-1 buy-sell-buttons" id ="buySell"><button type="button" class="grid-column-2 buy-sell-text btn ';
		if (isBuy) {
			html += 'btn-primary" id="buyTab">Buy</button><p class="grid-column-3 buy-sell-timer" id="buySellTimer"></p><button type="button" class="grid-column-4 buy-sell-text btn btn-outline-primary" id="sellTab">Sell</button></div><div class="grid-row-2"><p id="money">$';
		}
		else {
			html += 'btn-outline-primary" id="buyTab">Buy</button><p class="grid-column-3 buy-sell-timer" id="buySellTimer"></p><button type="button" class="grid-column-4 buy-sell-text btn btn-primary" id="sellTab">Sell</button></div><div class="grid-row-2"><p id="money">$';
		}
		html += money;
		html += '</p></div></div><div class="grid-row-2 scrollviewer-vertical" id="stockList"></div>';
		return html;
	},
	MakeRollPreviewButton: function () {
		return '<div class="roll-preview-banner" id="rollPreviewBtn"><button class="btn btn-primary roll-preview-btn">Tap to see first roll</button></div>';
	},
	MakeTrendPreviewButton: function () {
		return '<div class="roll-preview-banner" id="trendPreviewBtn"><button class="btn btn-primary roll-preview-btn">Tap to see trend</button></div>';
	},
	MakePushDownButton: function () {
		return '<div class="roll-preview-banner" id="pushDownBtn"><button class="btn btn-primary roll-preview-btn">Tap to sabotage</button></div>';
	},
	MakeRollPreview: function (rollDto) {
		let html = '<div class="stock-banner"><p class="grid-column-1 stock-text">';
		html += rollDto.stockName;
		html += '</p><p class="grid-column-2 stock-text">';
		html += rollDto.func;
		html += '</p><p class="grid-column-3 stock-text right-align-p">';
		html += rollDto.amount;
		html += '</p></div>';
		return html;
	},
	MakeTrendPreview: function (trendDto) {
		let html = '<div class="stock-banner"><p class="grid-column-1 stock-text">';
		if (trendDto.isNoInformation) {
			html += 'No Information Sucker</p></div>';
			return html;
		}
		html += trendDto.stockName;
		html += '</p><p class="grid-column-2 stock-text">Trending</p><p class="grid-column-3 stock-text right-align-p">';
		html += trendDto.direction;
		html += '</p></div>';
		return html;
	},
	MakeBuyStockBanner: function (stockName, stockValue) {
		let id = stockName + 'buy';
		let html = '<div class="stock-banner"><p class="grid-column-1 stock-text">';
		let isStockHalved = CurrentData.StockHalves[stockName];
		let buttonClass = isStockHalved ? 'btn-warning' : 'btn-success';
		html += stockName;
		html += '</p ><p class="grid-column-2 stock-text">';
		html += Unit.PercentToDecimalString(stockValue);
		html += '</p > <button type="button" class="grid-column-3 btn ';
		html += buttonClass;
		html += ' stock-text buy-sell-banner-button" id="';
		html += id;
		html += '">Buy</button></div >';
		// TODO There is a bug when a stock name has a space in it
		return {
			html: html,
			id: '#' + id
		}
	},
	MakeSellStockBanner: function (stockName, amountHeld) {
		let id = stockName + 'sell';
		let html = '<div class="stock-banner"><p class="grid-column-1 stock-text">';
		html += stockName;
		html += '</p><p class="grid-column-2 stock-text">';
		html += amountHeld;
		html += '</p><button type="button" class="grid-column-3 btn btn-info stock-text buy-sell-banner-button" id="';
		html += id;
		html += '">Sell</button></div >';
		return {
			html: html,
			id: '#' + id
		}
	},
	MakeStockBannerMarketClosed: function (stockName, amountHeld) {
		let html = '<div class="stock-banner"><p class="grid-column-1 stock-text">';
		html += stockName;
		html += '</p><p class="grid-column-2 stock-text">';
		html += amountHeld;
		html += '</p></div>';
		return html;
	},
	MakeJoinMenu: function (initialUsername) {
		if (initialUsername && initialUsername.length > 0) {
			let html = '<div class="center-absolute menu-join-grid"><div class="menu-sub-grid"><label for="username" class="menu-text">Username (';
			html += initialUsername.length;
			html += '/12):</label><input autocomplete="off" type="text" maxlength="12" class="menu-text" id="username" value="';
			html += initialUsername;
			html += '"/></div><button id="joinGame" class="btn btn-primary grid-row-2 menu-button">Join Game</button><button id="backButton" class="btn btn-outline-primary menu-button">Back</button></div>';
			return html;
		}
		else {
			return '<div class="center-absolute menu-join-grid"><div class="menu-sub-grid"><label for="username" class="menu-text">Username (0/12):</label><input autocomplete="off" type="text" maxlength="12" class="menu-text" id="username"/></div><button id="joinGame" class="btn btn-primary grid-row-2 menu-button">Join Game</button><button id="backButton" class="btn btn-outline-primary menu-button">Back</button></div>';
		}
	},
	MakeStartGameMenu: function () {
		return '<div class="center-absolute menu-grid"> <button id="startGame" class="btn btn-primary menu-button">Start Game</button></div>';
	},
	MakePresenter: function () {
		return '<div class="grid-observer-main grid-fill" id="mainGrid"> <div id="presenter" class="fill"> <h1 id="presenterText" class="grid-column-2 grid-row-1">Market Closed</h1> </div><div class="chart-grid grid-row-2"> <div id="chart-slide-container" class="grid-row-1"> <div id="inventoryChartSlider" class="chart-fill"> <canvas class="canvas-chart" id="inventoryChart"></canvas> </div><div id="presenterChartSlider" class="chart-fill"> <canvas class="canvas-chart" id="presenterChart"></canvas> </div></div><div class="roll-display grid-row-2"> <h1 class="grid-column-1 roll-text" id="rollName"></h1> <h1 class="grid-column-2 roll-text" id="rollFunc"></h1> <h1 class="grid-column-3 roll-text" id="rollAmount"></h1> </div></div></div>';
	},
	MakeEndGameButton: function () {
		return '<button class="btn btn-primary menu-button" id="endGameButton">End Game</button>';
	},
	MakeMainMenu: function () {
		return '<div class="grid-player-main grid-fill" id="mainGrid"><div class="center-absolute menu-grid"><button id="createGame" class="btn btn-primary grid-row-1 menu-button" disabled>Create Game</button><button id="joinGame" class="btn btn-primary grid-row-2 menu-button" disabled>Join Game</button><button id="watchGame" class="btn btn-primary grid-row-3 menu-button" disabled>Watch Game</button><form action="/help" class="grid-row-4"><button type="submit" id="howToPlay" class="btn btn-primary fill menu-button">How To Play</button></form></div></div>';
	},
	MakeEmptyGameplayGrid: function () {
		return '<div class="grid-player-main grid-fill" id="mainGrid"></div>';
	},
}

var ScreenOps = {
	State: "None",
	States: {
		MarketOpenBuy: "MarketOpenBuy",
		MarketOpenSell: "MarketOpenSell",
		MarketOpenPreBuy: "MarketOpenPreBuy",
		MarketOpenPreSell: "MarketOpenPreSell",
		MarketOpenPrePushDown: "MarketOpenPrePushDown",
		MarketClosed: "MarketClosed",
		Waiting: "Waiting",
		GameOver: "GameOver",
	},
	StateBuildMethod: {
		MarketOpenBuy: function () {
			ScreenOps.SwitchToBuy();
		},
		MarketOpenSell: function () {
			ScreenOps.SwitchToSell();
		},
		MarketClosed: function () {
			ScreenOps.SwitchToClosedMarket();
		},
		GameOver: function () {
			ScreenOps.SwitchToGameOver();
		},
		MarketOpenPrePushDown: function () {
			ScreenOps.SwitchToBuy();
		}
	},
	SwitchToMainMenu: function () {
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakeMainMenu());

		// Attach menu handlers
		let createGameButton = $(ConstHtmlIds.CreateGame);
		let joinGameButton = $(ConstHtmlIds.JoinGame);
		let watchGameButton = $(ConstHtmlIds.WatchGame);

		createGameButton.on(clickHandler, function () {
			ScreenOps.SwitchToParametersMenu();
		});
		createGameButton.prop('disabled', false);
		joinGameButton.on(clickHandler, function () {
			ScreenOps.SwitchToJoinMenu(false);
		});
		joinGameButton.prop('disabled', false);
		watchGameButton.on(clickHandler, function () {
			Connection.JoinGameObserver();
		});
		watchGameButton.prop('disabled', false);
	},
	SwitchToGameOver: function () {
		ScreenOps.State = ScreenOps.States.GameOver;
		let mainGrid = $(ConstHtmlIds.MainGrid);
		mainGrid.empty();
		mainGrid.append(HtmlGeneration.MakeGameOverScreen(CurrentData.Money));
	},
	SwitchToClosedMarket: function () {
		ScreenOps.State = ScreenOps.States.MarketClosed;
		let mainGrid = $(ConstHtmlIds.MainGrid);
		mainGrid.empty();
		mainGrid.append(HtmlGeneration.MakeMarketClosedScreen(CurrentData.Money));
		let list = $(ConstHtmlIds.StockList);

		for (let stockName in CurrentData.Holdings) {
			if (CurrentData.Holdings.hasOwnProperty(stockName)) {
				let amountHeld = CurrentData.Holdings[stockName];
				if (amountHeld > 0) {
					list.append(HtmlGeneration.MakeStockBannerMarketClosed(stockName, amountHeld));
				}
			}
		}
	},
	SwitchToStartGameMenu: function () {
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakeEmptyGameplayGrid());
		let mainGrid = $(ConstHtmlIds.MainGrid);
		//mainGrid.empty();
		mainGrid.append(HtmlGeneration.MakeStartGameMenu());
		$(ConstHtmlIds.StartGame).on(clickHandler, function () {
			Connection.StartGame();
		});
	},
	SwitchToOpenMarket: function (marketDto) {
		let mainGrid = $(ConstHtmlIds.MainGrid);
		mainGrid.empty();
		mainGrid.append(HtmlGeneration.MakeMarketScreen(CurrentData.Money, true));
		mainGrid.append(HtmlGeneration.MakeBuyStockBanner());

		let timerFunc = function (secondsRemaining) {
			if (secondsRemaining > 0) {
				$(ConstHtmlIds.BuySellTimer).text(secondsRemaining + 's');
			}
			else {
				$(ConstHtmlIds.BuySellTimer).text('');
			}
		};
		Presenter.StartTimer(Number(marketDto.marketCloseTimeInMilliseconds), timerFunc, 1000);

		ScreenOps.AttachOpenMarketTabHandlers();
		ScreenOps.SwitchToBuy();
	},
	AttachOpenMarketTabHandlers: function () {
		$(ConstHtmlIds.BuyTab).on(clickHandler, function () {
			let buyButton = $(ConstHtmlIds.BuyTab);
			let sellButton = $(ConstHtmlIds.SellTab);

			if (buyButton.hasClass(ConstHtmlIds.TabInactive)) {
				buyButton.removeClass(ConstHtmlIds.TabInactive);
				buyButton.addClass(ConstHtmlIds.TabActive);
				sellButton.removeClass(ConstHtmlIds.TabActive);
				sellButton.addClass(ConstHtmlIds.TabInactive);
			}
			ScreenOps.SwitchToBuy();
		});

		$(ConstHtmlIds.SellTab).on(clickHandler, function () {
			let buyButton = $(ConstHtmlIds.BuyTab);
			let sellButton = $(ConstHtmlIds.SellTab);

			if (sellButton.hasClass(ConstHtmlIds.TabInactive)) {
				sellButton.removeClass(ConstHtmlIds.TabInactive);
				sellButton.addClass(ConstHtmlIds.TabActive);
				buyButton.removeClass(ConstHtmlIds.TabActive);
				buyButton.addClass(ConstHtmlIds.TabInactive);
			}
			ScreenOps.SwitchToSell();
		});
	},
	SwitchToBuy: function () {
		ScreenOps.State = ScreenOps.States.MarketOpenBuy;
		let list = $(ConstHtmlIds.StockList);
		list.empty();

		// Add roll preview button if player has privileges
		if (CurrentData.CharacterId === 1) {
			list.append(HtmlGeneration.MakeRollPreviewButton());
			$(ConstHtmlIds.RollPreviewButton).on(clickHandler, function () {
				Connection.RequestRollPreview();
			});
		}
		else if (CurrentData.CharacterId === 2 && CurrentData.IsHalfTime) {
			list.append(HtmlGeneration.MakeTrendPreviewButton());
			$(ConstHtmlIds.TrendPreviewButton).on(clickHandler, function () {
				Connection.RequestTrendPreview();
			});
		}
		else if (CurrentData.CharacterId === 6) {
			list.append(HtmlGeneration.MakePushDownButton());
			$(ConstHtmlIds.PushDownButton).on(clickHandler, function () {
				ScreenOps.PrePushDown(true);
			});
		}

		for (let stockName in CurrentData.StockValues) {
			if (CurrentData.StockValues.hasOwnProperty(stockName)) {
				let stockValue = CurrentData.StockValues[stockName];
				let generated = HtmlGeneration.MakeBuyStockBanner(stockName, stockValue);
				list.append(generated.html);
				if ((stockValue * 500) / 100 <= CurrentData.Money) {
					$(generated.id).on(clickHandler, function () {
						ScreenOps.PreBuyStock(stockName);
					});
				}
				else {
					// Disable buy button when user doesn't have enough money to buy 500 shares
					$(generated.id).prop('disabled', true);
				}
			}
		}
	},
	SwitchToSell: function () {
		ScreenOps.State = ScreenOps.States.MarketOpenSell;
		let list = $(ConstHtmlIds.StockList);
		list.empty();

		// Add roll preview button if player has privileges
		if (CurrentData.CharacterId === 1) {
			list.append(HtmlGeneration.MakeRollPreviewButton());
			$(ConstHtmlIds.RollPreviewButton).on(clickHandler, function () {
				Connection.RequestRollPreview();
			});
		}
		else if (CurrentData.CharacterId === 2 && CurrentData.IsHalfTime) {
			list.append(HtmlGeneration.MakeTrendPreviewButton());
			$(ConstHtmlIds.TrendPreviewButton).on(clickHandler, function () {
				Connection.RequestTrendPreview();
			});
		}
		else if (CurrentData.CharacterId === 6) {
			list.append(HtmlGeneration.MakePushDownButton());
			$(ConstHtmlIds.PushDownButton).on(clickHandler, function () {
				ScreenOps.PrePushDown(false);
			});
		}

		for (let stockName in CurrentData.Holdings) {
			if (CurrentData.Holdings.hasOwnProperty(stockName)) {
				let amountHeld = CurrentData.Holdings[stockName];
				if (amountHeld > 0) {
					let generated = HtmlGeneration.MakeSellStockBanner(stockName, amountHeld);
					list.append(generated.html);
					$(generated.id).on(clickHandler, function () {
						ScreenOps.PreSellStock(stockName);
					});
				}
			}
		}
	},
	PreBuyStock: function (stockName) {
		ScreenOps.State = ScreenOps.States.MarketOpenPreBuy;
		let list = $(ConstHtmlIds.StockList);
		list.empty();
		list.append(HtmlGeneration.MakePreBuyScreen(stockName));

		// Add handlers
		$(ConstHtmlIds.BuyAmount).change(function () {
			let stockAmount = $(ConstHtmlIds.BuyAmount).find(":selected").text();
			let cost = (CurrentData.StockValues[stockName] * stockAmount) / 100;
			$(ConstHtmlIds.Buy).text('Buy for $' + cost);
		});
		$(ConstHtmlIds.Buy).on(clickHandler, function () {
			let stockAmount = $(ConstHtmlIds.BuyAmount).find(":selected").text();
			if (stockAmount === '0') {
				return;
			}
			Connection.BuyStock(stockName, stockAmount);
			ScreenOps.SwitchToBuy();
		});
		$(ConstHtmlIds.Cancel).on(clickHandler, function () {
			ScreenOps.SwitchToBuy();
		});
	},
	PreSellStock: function (stockName) {
		ScreenOps.State = ScreenOps.States.MarketOpenPreSell;
		let list = $(ConstHtmlIds.StockList);
		list.empty();
		list.append(HtmlGeneration.MakePreSellScreen(stockName));

		// Add handlers
		$(ConstHtmlIds.SellAmount).change(function () {
			let stockAmount = $(ConstHtmlIds.SellAmount).find(":selected").text();
			let cost = (CurrentData.StockValues[stockName] * stockAmount) / 100;
			$(ConstHtmlIds.Sell).text('Sell for $' + cost);
		});
		$(ConstHtmlIds.Sell).on(clickHandler, function () {
			let stockAmount = $(ConstHtmlIds.SellAmount).find(":selected").text();
			if (stockAmount === '0') {
				return;
			}
			Connection.SellStock(stockName, stockAmount);
			ScreenOps.SwitchToSell();
		});
		$(ConstHtmlIds.Cancel).on(clickHandler, function () {
			ScreenOps.SwitchToSell();
		});
	},
	PrePushDown: function (isBuy) {
		ScreenOps.State = ScreenOps.States.MarketOpenPrePushDown;
		let list = $(ConstHtmlIds.StockList);
		list.empty();
		list.append(HtmlGeneration.MakePrePushDownScreen());

		// Add handlers
		$(ConstHtmlIds.PushDownSendButton).on(clickHandler, function () {
			let stockName = $(ConstHtmlIds.PushDownValue).find(":selected").text();
			log("Sabotaging " + stockName);
			Connection.RequestStockPushDown(stockName);
			if (isBuy) {
				ScreenOps.SwitchToBuy();
			}
			else {
				ScreenOps.SwitchToSell();
			}
		});
		$(ConstHtmlIds.Cancel).on(clickHandler, function () {
			if (isBuy) {
				ScreenOps.SwitchToBuy();
			}
			else {
				ScreenOps.SwitchToSell();
			}
		});
	},
	SwitchToJoinMenu: function (initialUsername) {
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakeEmptyGameplayGrid);
		let mainGrid = $(ConstHtmlIds.MainGrid);
		mainGrid.append(HtmlGeneration.MakeJoinMenu(initialUsername));
		let joinGameUpdateFunc = function () {
			// Make sure username is not blank
			let username = $(ConstHtmlIds.Username).val();
			let usernameLength = 0;

			// Check length of username without illegal characters
			username = username.replace(/\W/g, '');
			if (username) {
				usernameLength = $(ConstHtmlIds.Username).val().length;
			}
			$('label[for=username]').text('Username (' + usernameLength + '/12):');

			let shouldDisable = true;
			if (username) {
				shouldDisable = false;
			}
			//let validText = shouldDisable ? 'not valid' : 'valid';
			$(ConstHtmlIds.JoinGame).prop('disabled', shouldDisable);
		};

		$(ConstHtmlIds.Username).keydown(function (e) {
			let key = e.key;
			if (key && key !== ' ') {
				key = key.replace(/\W/g, '');
				if (!key) {
					e.preventDefault();
				}
			}

			var keyCode = e.which;
			// Don't allow special characters
			if (!((keyCode >= 48 && keyCode <= 57)
				|| (keyCode >= 65 && keyCode <= 90)
				|| (keyCode >= 97 && keyCode <= 122))
				&& keyCode != 8 && keyCode != 32) {
				e.preventDefault();
			}
		});
		$(ConstHtmlIds.Username).keyup(joinGameUpdateFunc);
		$(ConstHtmlIds.IsPlayer).change(joinGameUpdateFunc);

		$(ConstHtmlIds.JoinGame).on(clickHandler, function () {
			CurrentData.Username = $(ConstHtmlIds.Username).val();
			ScreenOps.SwitchToCharacterSelectMenu();
		});
		if (!initialUsername) {
			$(ConstHtmlIds.JoinGame).prop('disabled', true);
		}
		$(ConstHtmlIds.Username).focus();

		$(ConstHtmlIds.BackButton).on(clickHandler, function () {
			ScreenOps.SwitchToMainMenu();
		});
	},
	SwitchToCharacterSelectMenu: function () {
		let body = $('body');
		body.empty();
		body.load('/Game/Characters #menu-character-container', function () {
			// Attach handlers
			log('Characters loaded callback');
			let characterAmount = 6;
			for (let i = 1; i <= characterAmount; i++) {
				$(ConstHtmlIds.SelectCharacter + i).on(clickHandler, function () {
					log('Selecting character: ' + i);
					Connection.JoinGame(CurrentData.Username, i);
				});
			}

			$(ConstHtmlIds.BackButton).on(clickHandler, function () {
				ScreenOps.SwitchToJoinMenu(CurrentData.Username);
			});
		});
	},
	SwitchToWaitingMenu: function () {
		ScreenOps.State = ScreenOps.States.Waiting;
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakeEmptyGameplayGrid());
		$(ConstHtmlIds.MainGrid).append(HtmlGeneration.MakeWaitingScreen(CurrentData.Username, CurrentData.Money));
	},
	SwitchToParametersMenu: function () {
		let body = $('body');
		body.empty();
		body.load('/Game/CreateGame #parameters-menu', function () {
			// Attach handlers
			log('CreateGame loaded callback');
			$(ConstHtmlIds.CreateGameWithParameters).on(clickHandler, function () {
				let marketTime = Number($(ConstHtmlIds.ParamMarketOpenTime).val());
				let startingMoney = Number($(ConstHtmlIds.ParamStartingMoney).val());
				let rollsPerRound = Number($(ConstHtmlIds.ParamRollsPerRound).val());
				let rounds = Number($(ConstHtmlIds.ParamRounds).val());
				let stockPreset = Number($(ConstHtmlIds.ParamStockPresets).val());

				let params = {
					marketOpenTimeInSeconds: marketTime,
					startingMoney: startingMoney,
					rollsPerRound: rollsPerRound,
					numberOfRounds: rounds,
					stockPreset: stockPreset,
				};
				Connection.CreateGame(params);
			});

			$(ConstHtmlIds.ParamMarketOpenTime).on('input', function () {
				$('label[for=marketOpenTime]').text('Market Open Time: ' + this.value + 's');
			})
			$(ConstHtmlIds.ParamStartingMoney).on('input', function () {
				$('label[for=startingMoney]').text('Starting Money: $' + this.value);
			})
			$(ConstHtmlIds.ParamRollsPerRound).on('input', function () {
				$('label[for=rollsPerRound]').text('Rolls per Round: ' + this.value);
			})
			$(ConstHtmlIds.ParamRounds).on('input', function () {
				$('label[for=rounds]').text('Rounds: ' + this.value);
			})
			let getPresetName = function (preset) {
				// The preset name is in a hidden input, so grab it
				let name = $('#preset' + preset).val();
				log('Preset name: ' + name);
				return name;
			};
			$(ConstHtmlIds.ParamStockPresets).on('input', function () {
				let name = getPresetName(this.value);
				$('label[for=stockPresets]').text('Stock Preset: ' + name);
			});

			$(ConstHtmlIds.BackButton).on(clickHandler, function () {
				ScreenOps.SwitchToMainMenu();
			});
		});
	},
	ShowEndGameButton: function () {
		$('body').append(HtmlGeneration.MakeEndGameButton());
		$(ConstHtmlIds.EndGameButton).on(clickHandler, function () {
			Connection.EndGame();
			ScreenOps.SwitchToMainMenu();
		});
	},
};

var Presenter = {
	IsInitialized: false,
	OneTimeInit: function () {
		if (Presenter.IsInitialized) {
			return;
		}
		// Register label plugin
		Chart.register(ChartDataLabels);

		// Don't show number when zero
		Chart.defaults.font.size = 36;

		GameAudio.Init();
	},
	Chart: undefined,
	InventoryChart: undefined,
	GetChartData: function () {
		let stockNames = [];
		let stockValues = [];
		let stockColors = [];
		let stockBorderColors = [];
		for (let stockName in CurrentData.StockValues) {
			if (CurrentData.StockValues.hasOwnProperty(stockName)) {
				stockNames.push(stockName);
				stockValues.push(CurrentData.StockValues[stockName]);
				let backgroundColor = CurrentData.StockColors[stockName];
				let borderColor = backgroundColor;
				stockColors.push(backgroundColor + 'B0');
				if (CurrentData.StockHalves[stockName]) {
					borderColor = "#f0ad4e";
				}
				stockBorderColors.push(borderColor);
			}
		}
		return {
			stockNames: stockNames,
			stockValues: stockValues,
			stockColors: stockColors,
			stockBorderColors: stockBorderColors,
		}
	},
	GetChartConfig: function (data) {
		let config = {
			type: 'bar',
			data: data,
			options: {
				showToolTips: false,
				plugins: {
					legend: {
						display: false
					},
					annotation: {
						annotations: {
							label1: {
								drawTime: 'beforeDatasetsDraw',
								type: 'line',
								scaleID: 'y',
								mode: 'horizontal',
								scaleID: 'yAxes',
								value: 150,
								borderColor: 'rgba(0, 0, 0, 0)',
								enter: function (c, e) {
									log(c.element);
								},
								label: {
									rotation: Config.DividendLabelAngle,
									content: 'DIVIDENDS PAYABLE',
									enabled: true,
									backgroundColor: Config.DividendLabelColor,
								}
							},
							label2: {
								drawTime: 'beforeDatasetsDraw',
								type: 'line',
								scaleID: 'y',
								mode: 'horizontal',
								scaleID: 'yAxes',
								value: 50,
								borderColor: 'rgba(0, 0, 0, 0)',
								label: {
									rotation: Config.DividendLabelAngle,
									content: 'DIVIDENDS NOT PAYABLE',
									enabled: true,
									backgroundColor: Config.DividendLabelColor,
								}
							},
							parLine: {
								drawTime: 'beforeDatasetsDraw',
								type: 'line',
								scaleID: 'y',
								mode: 'horizontal',
								scaleID: 'yAxes',
								value: 100,
								mode: 'horizontal',
								borderDash: [10,10],
								borderWidth: 3,
								borderColor: 'rgba(0, 0, 0, 0.4)',
							}
						}
					}
				},
				tooltips: {
					enabled: false
				},
				responsive: true,
				maintainAspectRatio: true,
				scales: {
					yAxes: {
						min: 0,
						max: 200
					},
				},
			}
		};
		return config;
	},
	CreateCharts: function () {
		Presenter.OneTimeInit();
		if (Presenter.Chart) {
			log('Chart was already created')
			return;
		}
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakePresenter());

		// Create inventory chart
		let inventoryConfig = Presenter.GetInventoryChartConfig(Presenter.GetInventoryChartData());
		let inventoryCanvas = document.getElementById(ConstHtmlIds.InventoryChart);
		let inventoryCtx = inventoryCanvas.getContext('2d');
		Presenter.InventoryChart = new Chart(inventoryCtx, inventoryConfig);

		// Create market chart
		let stockData = Presenter.GetChartData();
		let canvas = document.getElementById(ConstHtmlIds.PresenterChart);
		let ctx = canvas.getContext('2d');

		let data = {
			labels: stockData.stockNames,
			datasets: [
				{
					data: stockData.stockValues,
					backgroundColor: stockData.stockColors,
					borderColor: stockData.stockBorderColors,
					borderWidth: 5,
				}
			]
		};
		let config = Presenter.GetChartConfig(data);
		Presenter.Chart = new Chart(ctx, config);
	},
	UpdateChart: function () {
		if (!Presenter.Chart) {
			Presenter.CreateCharts();
		}
		let stockData = Presenter.GetChartData();
		for (let i = 0; i < stockData.stockValues.length; i++) {
			Presenter.Chart.data.datasets[0].data[i] = stockData.stockValues[i];
		}
		Presenter.Chart.update();
	},
	StartTimer: function (endTime, displayFunc, intervalLength) {
		let intervalId = -1;
		let intervalFunc = function () {
			let now = (new Date()).getTime();
			if (now >= endTime) {
				clearInterval(intervalId);
			}
			displayFunc(Math.ceil((endTime - now) / 1000));
		};
		intervalId = setInterval(intervalFunc, intervalLength);
	},
	SwitchToPlayerInventoryChart: function () {
		$(ConstHtmlIds.PresenterChartSlider).appendTo('#chart-slide-container');
	},
	SwitchToMarketChart: function () {
		$(ConstHtmlIds.InventoryChartSlider).appendTo('#chart-slide-container');
	},
	SetMarketOpen: function (endTime, currentRound, totalRounds, isHalfTime) {
		let timerFunc = function (secondsRemaining) {
			if (secondsRemaining > 0) {
				let roundText = isHalfTime ? 'Half Time' : 'Round ' + (currentRound + 1) + '/' + totalRounds;
				let displayText = roundText + ' | Market Open for ' + secondsRemaining + 's';
				$(ConstHtmlIds.PresenterText).text(displayText);
			}
			else {
				$(ConstHtmlIds.PresenterText).text("Market Closed");
			}
		};
		Presenter.StartTimer(endTime, timerFunc, 500);

		// Initially switch to inventory chart
		let isMarketChartActive = false;
		Presenter.SwitchToPlayerInventoryChart();

		let graphSwitchFunc = function (secondsRemaining) {
			if (secondsRemaining > 10) {
				if (isMarketChartActive) {
					isMarketChartActive = false;
					Presenter.SwitchToPlayerInventoryChart();
				}
				else {
					// Show market graph
					isMarketChartActive = true;
					Presenter.SwitchToMarketChart();
				}
			}
			else {
				// Show market graph
				if (!isMarketChartActive) {
					isMarketChartActive = true;
					Presenter.SwitchToMarketChart();
				}
			}
		};
		Presenter.StartTimer(endTime, graphSwitchFunc, Config.InventoryMarketChartSwitchTime);
	},
	SetMarketClosed: function () {
		$(ConstHtmlIds.PresenterText).text("Market Closed");

		// Show market graph
		Presenter.SwitchToMarketChart();
	},
	SetGameOver: function () {
		$(ConstHtmlIds.PresenterText).text("Game Over");
		// Show user graph
		Presenter.SwitchToPlayerInventoryChart();
	},
	ShowRoll: function (rollDto) {
		let state = 0;
		let intervalId = -1;
		$(ConstHtmlIds.RollName).text(rollDto.stockName);
		let intervalFunc = function () {
			if (state === 0) {
				$(ConstHtmlIds.RollFunc).text(rollDto.func);
			}
			else if (state === 1) {
				$(ConstHtmlIds.RollAmount).text(rollDto.amount);
			}
			else if (state === 2) {
				if (rollDto.func === 'Up') {
					// Check for split
					if (CurrentData.StockValues[rollDto.stockName] >= 200) {
						GameAudio.PlayAudio(GameAudio.Split);
					}
					else {
						GameAudio.PlayAudio(GameAudio.Up);
					}
				}
				else if (rollDto.func === 'Down') {
					// Check for crash
					if (CurrentData.StockValues[rollDto.stockName] <= 0) {
						GameAudio.PlayAudio(GameAudio.Crash);
					}
					else {
						GameAudio.PlayAudio(GameAudio.Down);
					}
				}
				else {
					// Only play div sound if stock is par or above
					if (CurrentData.StockValues[rollDto.stockName] >= 100) {
						GameAudio.PlayAudio(GameAudio.Div);
					}
					else {
						GameAudio.PlayAudio(GameAudio.NoDiv);
					}
				}
				Presenter.UpdateChart();
			}
			else {
				$(ConstHtmlIds.RollName).text('');
				$(ConstHtmlIds.RollFunc).text('');
				$(ConstHtmlIds.RollAmount).text('');
				clearInterval(intervalId);
			}
			state++;
		};
		let intervalTime = (rollDto.rollTimeInSeconds * 1000) / 4;
		intervalId = setInterval(intervalFunc, intervalTime);
	},
	GetInventoryChartData: function () {
		let moneyColor = Config.CashColor;
		let moneyKey = 'Cash';
		let datasetObject = {};
		let labels = [];

		// Initialize datasets - one for money and one for each stock
		datasetObject[moneyKey] = {
			label: moneyKey,
			data: [],
			borderColor: moneyColor,
			backgroundColor: moneyColor + 'B0'
		};
		for (let stockName in CurrentData.StockValues) {
			if (CurrentData.StockValues.hasOwnProperty(stockName)) {
				let backgroundColor = CurrentData.StockColors[stockName];
				let borderColor = backgroundColor;
				if (CurrentData.StockHalves[stockName]) {
					borderColor = "#f0ad4e";
				}
				datasetObject[stockName] = {
					label: stockName,
					data: [],
					borderColor: borderColor,
					backgroundColor: backgroundColor + 'B0'
				};
			}
		}

		// Sort users by net worth
		let sortedUserInventories = [];

		let comparer = function (lhs, rhs) {
			if (lhs.NetWorth < rhs.NetWorth) {
				return 1;
			}
			else if (lhs.NetWorth > rhs.NetWorth) {
				return -1;
			}
			return 0;
		};

		for (let id in CurrentData.PlayerInventories) {
			if (CurrentData.PlayerInventories.hasOwnProperty(id)) {
				let inventory = CurrentData.PlayerInventories[id];
				inventory.NetWorth = 0;
				inventory.NetWorth += inventory.money;

				// Add stock holdings as worth, not shares
				for (let stockName in inventory.holdings) {
					if (inventory.holdings.hasOwnProperty(stockName)) {
						let amountHeld = inventory.holdings[stockName];
						let shareWorth = CurrentData.StockValues[stockName];
						inventory.NetWorth += (amountHeld * shareWorth) / 100;
					}
				}
				sortedUserInventories.push(inventory);
			}
		}
		sortedUserInventories.sort(comparer);

		// Add user data
		for (let i = 0; i < sortedUserInventories.length; i++) {
			let inventory = sortedUserInventories[i];
			// Add username to labels
			labels.push(inventory.username);

			// Add money
			datasetObject[moneyKey].data.push(inventory.money);

			// Add stock holdings as worth, not shares
			for (let stockName in inventory.holdings) {
				if (inventory.holdings.hasOwnProperty(stockName)) {
					let amountHeld = inventory.holdings[stockName];
					let shareWorth = CurrentData.StockValues[stockName];
					datasetObject[stockName].data.push((amountHeld * shareWorth) / 100);
				}
			}
		}
		// Push datasets into an array
		let datasets = [];
		for (let assetName in datasetObject) {
			if (datasetObject.hasOwnProperty(assetName)) {
				let dataset = datasetObject[assetName];
				datasets.push(dataset);
			}
		}

		let data = {
			labels: labels,
			datasets: datasets
		};
		return data;
	},
	GetInventoryChartConfig: function (data) {
		let config = {
			type: 'bar',
			data: data,
			options: {
				showToolTips: false,
				plugins: {
					legend: {
						display: true
					},
					datalabels: {
						// TODO Fix that corner problem
						formatter: function (value, context) {
							if (value === null || value === undefined) {
								return null;
							}
							return '$' + value;
						},
						display: function (ctx) {
							let value = Number(ctx.dataset.data[ctx.dataIndex]);
							return value > 0;
						}
					}
				},
				tooltips: {
					enabled: false
				},
				responsive: true,
				maintainAspectRatio: true,
				scales: {
					x: {
						stacked: true,
					},
					y: {
						stacked: true
					}
				}
			}
		};
		return config;
	},
	UpdateInventoryChart: function () {
		let inventoryData = Presenter.GetInventoryChartData();

		let chartData = Presenter.InventoryChart.data;
		if (chartData.labels.length <= inventoryData.labels.length) {
			// Usernames have been updated
			chartData.labels = inventoryData.labels;
		}

		for (let i = 0; i < inventoryData.datasets.length; ++i) {
			for (let j = 0; j < inventoryData.datasets[i].data.length; ++j) {
				chartData.datasets[i].data[j] = inventoryData.datasets[i].data[j];
			}
		}

		Presenter.InventoryChart.update();
	},
};

$(document).ready(function () {
	clickHandler = ("ontouchstart" in window ? "touchend" : "click");

	Connection.Init();
});

