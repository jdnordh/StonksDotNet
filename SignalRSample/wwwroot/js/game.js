// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

"use strict";

var clickHandler;
var globalIsPrototype = false;

var log = function (msg) {
	console.log(msg);
};

var Unit = {
	PercentToDecimal: function (percentage) {
		return percentage / 100;
	},
	PercentToDecimalString: function (percentage) {
		return (percentage / 100).toLocaleString(undefined, { minimumFractionDigits: 2 });
	},
};

var CurrentData = {
	Username: "StonkMaster",
	Holdings: { },
	StockValues: { },
	StockColors: { },
	StockHalves: { },
	Money: 0,
};

var Connection = {
	ClientType: -1,
	ClientTypes: {
		None: -1,
		Player: 1,
		Observer: 2,
	},
	ClientMethods: {
		GameCreated: "gameCreated",
		CreateGameUnavailable: "createGameUnavailable",
		GameJoined: "gameJoined",
		GameStarted: "gameStarted",
		GameOver: "gameOver",
		GameEnded: "gameEnded",
		InventoryUpdated: "inventoryUpdated",
		MarketUpdated: "marketUpdated",
		TransactionFailed: "transactionFailed",
		Rolled: "rolled",
	},
	ServerMethods: {
		CreateGame: "CreateGame",
		JoinGame: "JoinGame",
		StartGame: "StartGame",
		EndGame: "EndGame",
		RequestTransaction: "RequestTransaction",
		Reset: "Reset",
	},
	Reset: function () {
		Connection.Hub.invoke(Connection.ServerMethods.Reset).catch(function (err) {
			return console.error(err.toString());
		});
	},
	Init: function (onConnectionStarted) {
		Connection.Hub = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();
		Connection.Hub.serverTimeoutInMilliseconds = 1800000;

		Connection.Hub.on(Connection.ClientMethods.GameCreated, function () {
			Connection.ClientType = Connection.ClientTypes.Observer;
			ScreenOps.SwitchToStartGameMenu();
		});

		Connection.Hub.on(Connection.ClientMethods.GameJoined, function (playerInventoryDto) {
			Connection.ClientType = Connection.ClientTypes.Player;
			Connection.UpdateInventory(playerInventoryDto);
			CurrentData.Username = playerInventoryDto.username;
			ScreenOps.SwitchToWaitingMenu();
		});

		Connection.Hub.on(Connection.ClientMethods.CreateGameUnavailable, function () {
			$(ConstHtmlIds.CreateGame).prop('disabled', true);
		});

		Connection.Hub.on(Connection.ClientMethods.GameStarted, function () {
			if (Connection.ClientType === Connection.ClientTypes.Observer) {
				Presenter.CreateChart();
			}
		});

		Connection.Hub.on(Connection.ClientMethods.InventoryUpdated, function (playerInventoryDto) {
			if (Connection.ClientType === Connection.ClientTypes.Player) {
				Connection.UpdateInventory(playerInventoryDto);
			}
		});

		Connection.Hub.on(Connection.ClientMethods.MarketUpdated, function (marketDto) {
			Connection.UpdateStockValues(marketDto);
			if (Connection.ClientType === Connection.ClientTypes.Observer) {
				Presenter.UpdateChart();
				if (marketDto.isOpen) {
					let marketEndTime = Number(marketDto.marketCloseTimeInMilliseconds);
					Presenter.SetMarketOpen(marketEndTime, marketDto.currentRound, marketDto.totalRounds);
				}
				else {
					Presenter.SetMarketClosed();
				}
			}
			else if (Connection.ClientType === Connection.ClientTypes.Player) {
				if (marketDto.isOpen) {
					ScreenOps.SwitchToOpenMarket(marketDto);
				}
				else {
					ScreenOps.SwitchToClosedMarket();
				}
			}
		});

		Connection.Hub.on(Connection.ClientMethods.Rolled, function (marketDto) {
			if (Connection.ClientType === Connection.ClientTypes.Observer) {
				Connection.UpdateStockValuesFromRoll(marketDto.rollDto);
				Presenter.ShowRoll(marketDto.rollDto);
			}
		});

		Connection.Hub.on(Connection.ClientMethods.GameEnded, function () {
			Presenter.IsInitialized = false;
			if (Presenter.Chart) {
				Presenter.Chart.destroy();
				Presenter.Chart = undefined;
			}
			CurrentData = {
				Username: "StonkMaster",
				Holdings: {},
				StockValues: {},
				StockColors: {},
				StockHalves: {},
				Money: 0,
			};
			ScreenOps.SwitchToMainMenu();
		});

		Connection.Hub.on(Connection.ClientMethods.GameOver, function (gameEndDto) {
			if (Connection.ClientType === Connection.ClientTypes.Observer) {
				Presenter.SetGameOver(gameEndDto);
			}
			else {
				$(ConstHtmlIds.MarketOpenClosedHeader).text("Game Over");
			}
		});

		Connection.Hub.start().then(function () {
			ScreenOps.SwitchToMainMenu();
		}).catch(function (err) {
			return console.error(err.toString());
		});
	},
	UpdateStockValues: function (marketDto) {
		for (let stockName in marketDto.stocks) {
			if (marketDto.stocks.hasOwnProperty(stockName)) {
				let stockDto = marketDto.stocks[stockName];
				CurrentData.StockValues[stockName] = stockDto.value;
				CurrentData.StockColors[stockName] = stockDto.color;
				CurrentData.StockHalves[stockName] = stockDto.isHalved;
			}
		}
	},
	UpdateStockValuesFromRoll: function (rollDto) {
		if (rollDto.func === 'Up') {
			CurrentData.StockValues[rollDto.stockName] += rollDto.amount;
		}
		else if (rollDto.func === 'Down') {
			CurrentData.StockValues[rollDto.stockName] -= rollDto.amount;
		}
	},
	UpdateInventory: function (playerInventoryDto) {
		for (let stockName in playerInventoryDto.holdings) {
			if (playerInventoryDto.holdings.hasOwnProperty(stockName)) {
				let amount = playerInventoryDto.holdings[stockName];
				CurrentData.Holdings[stockName] = amount;
			}
		}
		Connection.SetMoney(playerInventoryDto.money);
		Connection.OnServerUpdate();
	},
	RequestTransaction: function (stockName, isBuy, amount) {
		Connection.Hub.invoke(Connection.ServerMethods.RequestTransaction, stockName, isBuy, Number(amount)).catch(function (err) {
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
	JoinGame: function (username) {
		Connection.Hub.invoke(Connection.ServerMethods.JoinGame, username).catch(function (err) {
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
	Username: "#username",
	StartGame: "#startGame",
	PresenterChart: "presenterChart", // No hash in front because it's used by chartjs, not jquery
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
	ParamRollTime: "#rollTime",
	ParamTimeBetweenRolls: "#timeBetweenRolls",
	ParamUsePrototype: "#usePrototype",
	BuySellTimer: "#buySellTimer",
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
		//log('Max buy amount: ' + maxBuyAmount);
		for (let i = 0; i <= maxBuyAmount; i += 500) {
			html += '<option value="';
			html += i;
			html += '">'
			html += i;
			html += '</option>';
		}
		html += '</select><button class="btn btn-success buy-sell-button fill grid-column-2 grid-row-2" id="buy">Buy for $0</button><button class="btn btn-danger buy-sell-button fill grid-column-2 grid-row-3" id="cancel">Cancel</button></div></div>';
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
		for (let i = 0; i <= stockAmount; i += 500) {
			html += '<option value="';
			html += i;
			html += '">'
			html += i;
			html += '</option>';
		}
		html += '</select><button class="btn btn-info buy-sell-button fill grid-column-2 grid-row-2" id="sell">Sell for $0</button><button class="btn btn-danger buy-sell-button fill grid-column-2 grid-row-3" id="cancel">Cancel</button></div></div>';
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
	MakeMainMenu: function () {
		return '<div class="center-absolute menu-grid"> <button id="createGame" class="btn btn-primary grid-row-1 menu-button">Create Game</button> <button id="joinGame" class="btn btn-primary grid-row-2 menu-button">Join Game</button></div>';
	},
	MakeJoinMenu: function (isCreateGame) {
		return '<div class="center-absolute menu-grid"><div class="menu-sub-grid"> <label for="username" class="menu-text">Username (0/12):</label> <input autocomplete="off" type="text" maxlength="12" class="menu-text" id="username" /></div> <button id="joinGame" class="btn btn-primary grid-row-2 menu-button">Join Game</button></div>';
	},
	MakeStartGameMenu: function () {
		return '<div class="center-absolute menu-grid"> <button id="startGame" class="btn btn-primary menu-button">Start Game</button></div>';
	},
	MakePresenter: function () {
		return '<div class="grid-observer-main grid-fill" id="mainGrid"><div id="presenter" class="fill"><h1 id="presenterText" class="grid-column-2 grid-row-1 menu-text">Market Open</h1></div><div class="chart-grid grid-row-2"><div class="chart-fill grid-row-1"><canvas id="presenterChart"></canvas></div><div class="roll-display grid-row-2"><h1 class="grid-column-1 roll-text" id="rollName"></h1><h1 class="grid-column-2 roll-text" id="rollFunc"></h1><h1 class="grid-column-3 roll-text" id="rollAmount"></h1></div></div></div>';
	},
	MakeEndGameButton: function () {
		return '<button class="btn btn-primary menu-button" id="endGameButton">End Game</button>';
	},
	MakeMainMenu: function () {
		return '<div class="grid-player-main grid-fill" id="mainGrid"> <div class="center-absolute menu-grid"> <button id="createGame" class="btn btn-primary grid-row-1 menu-button" disabled>Create Game</button> <button id="joinGame" class="btn btn-primary grid-row-2 menu-button" disabled>Join Game</button></div></div>';
	},
	MakeParametersMenu: function () {
		return '<div class="scrollviewer-vertical"><div class="grid-input-parameters"><label for="marketOpenTime" class="menu-text">Market Open Time (s):</label><input id="marketOpenTime" autocomplete="off" type="text" class="menu-text" value="60"/><label for="startingMoney" class="menu-text">Starting Money:</label><input id="startingMoney" autocomplete="off" type="text" class="menu-text" value="5000"/><label for="rollsPerRound" class="menu-text"># of Rolls per Round:</label><input id="rollsPerRound" autocomplete="off" type="text" class="menu-text" value="10"/><label for="rounds" class="menu-text"># of Rounds:</label><input id="rounds" autocomplete="off" type="text" class="menu-text" value="7"/><label for="rollTime" class="menu-text">Roll Time (s):</label><input id="rollTime" autocomplete="off" type="text" class="menu-text" value="2"/><label for="timeBetweenRolls" class="menu-text">Time Between Rolls:</label><input id="timeBetweenRolls" autocomplete="off" type="text" class="menu-text" value="2"/><div class="grid-fill"><label for="usePrototype" class="menu-text grid-column-1">Use Prototype:</label><input id="usePrototype" autocomplete="off" type="checkbox" class="form-check-label big-checkbox grid-column-2"/></div><button id="createGameWithParameters" class="btn btn-primary menu-button">Create Game</button><br/></div></div>';
	},
}

var ScreenOps = {
	State: "None",
	States: {
		MarketOpenBuy: "MarketOpenBuy",
		MarketOpenSell: "MarketOpenSell",
		MarketOpenPreBuy: "MarketOpenPreBuy",
		MarketOpenPreSell: "MarketOpenPreSell",
		MarketClosed: "MarketClosed",
		Waiting: "Waiting",
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
	},
	SwitchToMainMenu: function () {
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakeMainMenu());
		let createGameButton = $(ConstHtmlIds.CreateGame);
		let joinGameButton = $(ConstHtmlIds.JoinGame);

		// Attach menu handlers
		createGameButton.on(clickHandler, function () {
			ScreenOps.SwitchToParametersMenu();
		});
		createGameButton.prop('disabled', false);
		joinGameButton.on(clickHandler, function () {
			ScreenOps.SwitchToJoinMenu(false);
		});
		joinGameButton.prop('disabled', false);
		
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
		// TODO This is messy 
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakeMainMenu());
		let mainGrid = $(ConstHtmlIds.MainGrid);
		mainGrid.empty();
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
		Presenter.StartTimer(Number(marketDto.marketCloseTimeInMilliseconds), timerFunc);

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
	SwitchToJoinMenu: function () {
		let mainGrid = $(ConstHtmlIds.MainGrid);
		mainGrid.empty();
		mainGrid.append(HtmlGeneration.MakeJoinMenu());
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
		$(ConstHtmlIds.Username).keyup(function () {
			// Make sure username is not blank
			let username = $(ConstHtmlIds.Username).val();
			let usernameLength = 0;
			if (username) {
				usernameLength = $(ConstHtmlIds.Username).val().length;
			}
			$('label[for=username]').text('Username (' + usernameLength + '/12):');

			// Check length of username without illegal characters
			username = username.replace(/\W/g, '');
			let shouldDisable = true;
			if (username) {
				shouldDisable = false;
			}
			//let validText = shouldDisable ? 'not valid' : 'valid';
			//log('Input ' + username + ' is ' + validText);
			$(ConstHtmlIds.JoinGame).prop('disabled', shouldDisable);
		});
		$(ConstHtmlIds.JoinGame).on(clickHandler, function () {
			let username = $(ConstHtmlIds.Username).val();
			Connection.JoinGame(username);
		});
		$(ConstHtmlIds.JoinGame).prop('disabled', true);
		$(ConstHtmlIds.Username).focus();
	},
	SwitchToWaitingMenu: function () {
		ScreenOps.State = ScreenOps.States.Waiting;
		let mainGrid = $(ConstHtmlIds.MainGrid);
		mainGrid.empty();

		mainGrid.append(HtmlGeneration.MakeWaitingScreen(CurrentData.Username, CurrentData.Money));
	},
	SwitchToParametersMenu: function () {
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakeParametersMenu());

		// Attach handlers
		$(ConstHtmlIds.CreateGameWithParameters).on(clickHandler, function () {
			let marketTime = Number($(ConstHtmlIds.ParamMarketOpenTime).val());
			let startingMoney = Number($(ConstHtmlIds.ParamStartingMoney).val());
			let rollsPerRound = Number($(ConstHtmlIds.ParamRollsPerRound).val());
			let rounds = Number($(ConstHtmlIds.ParamRounds).val());
			let rollTime = Number($(ConstHtmlIds.ParamRollTime).val());
			let timeBetweenRolls = Number($(ConstHtmlIds.ParamTimeBetweenRolls).val());
			let usePrototype = $(ConstHtmlIds.ParamUsePrototype).is(":checked");

			let params = {
				marketTime: marketTime,
				startingMoney: startingMoney,
				rollsPerRound: rollsPerRound,
				rounds: rounds,
				rollTime: rollTime,
				timeBetweenRolls: timeBetweenRolls,
				usePrototype: usePrototype,
			};

			Connection.CreateGame(params);
		});
	},
};

var Presenter = {
	IsInitialized: false,
	OneTimeInit: function () {
		if (Presenter.IsInitialized) {
			return;
		}
		Chart.register(ChartDataLabels);
		Chart.defaults.font.size = 36;
	},
	Chart: undefined,
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
	GetChartConfig: function (data, isGameEnd) {
		let config = {
			type: 'bar',
			data: data,
			options: {
				showToolTips: false,
				plugins: {
					legend: {
						display: false
					}
				},
				tooltips: {
					enabled: false
				},
				responsive: true,
				maintainAspectRatio: true,
			}
		};

		if (!isGameEnd) {
			config.options.scales = {
				yAxes: {
					min: 0,
					max: 200
				}
			};
		}
		return config;
	},
	CreateChart: function () {
		Presenter.OneTimeInit();
		if (Presenter.Chart) {
			log('Chart was already created')
			return;
		}
		let body = $('body');
		body.empty();
		body.append(HtmlGeneration.MakePresenter());

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
					borderWidth: 5
				}
			]
		};
		let config = Presenter.GetChartConfig(data);
		Presenter.Chart = new Chart(ctx, config);
	},
	UpdateChart: function () {
		if (!Presenter.Chart) {
			Presenter.CreateChart();
		}
		let stockData = Presenter.GetChartData();
		for (let i = 0; i < stockData.stockValues.length; i++) {
			Presenter.Chart.data.datasets[0].data[i] = stockData.stockValues[i];
		}
		Presenter.Chart.update();
	},
	StartTimer: function (endTime, displayFunc) {
		let intervalId = -1;
		let intervalFunc = function () {
			let now = (new Date()).getTime();
			if (now >= endTime) {
				clearInterval(intervalId);
			}
			displayFunc(Math.ceil((endTime - now) / 1000));
		};
		intervalId = setInterval(intervalFunc, 1000);
	},
	SetMarketOpen: function (endTime, currentRound, totalRounds) {
		let timerFunc = function (secondsRemaining) {
			if (secondsRemaining > 0) {
				$(ConstHtmlIds.PresenterText).text('Round ' + currentRound + '/' + totalRounds + ' | Market Open for ' + secondsRemaining + 's');
			}
			else {
				$(ConstHtmlIds.PresenterText).text("Market Closed");
			}
		};
		Presenter.StartTimer(endTime, timerFunc);
	},
	SetMarketClosed: function () {
		$(ConstHtmlIds.PresenterText).text("Market Closed");
	},
	SetGameOver: function (gameOverDto) {
		log(gameOverDto.wallets);
		$(ConstHtmlIds.PresenterText).text("Game Over");
		$('body').append(HtmlGeneration.MakeEndGameButton());
		$(ConstHtmlIds.EndGameButton).on(clickHandler, function () {
			Connection.EndGame();
			ScreenOps.SwitchToMainMenu();
		});

		let comparer = function (lhs, rhs) {
			if (lhs.money < rhs.money) {
				return 1;
			}
			if (lhs.money > rhs.money) {
				return -1;
			}
			return 0;
		};
		gameOverDto.wallets.sort(comparer);

		let canvas = document.getElementById(ConstHtmlIds.PresenterChart);
		let ctx = canvas.getContext('2d');

		let labels = [];
		let walletAmounts = [];
		let userColors = [];
		for (let i = 0; i < gameOverDto.wallets.length; i++) {
			labels.push(gameOverDto.wallets[i].username);
			walletAmounts.push(gameOverDto.wallets[i].money);
			userColors.push('#' + Math.floor(Math.random() * 16777215).toString(16));
		}

		let data = {
			labels: labels,
			datasets: [
				{
					data: walletAmounts,
					backgroundColor: userColors
				}
			]
		};

		let config = Presenter.GetChartConfig(data, true);
		Presenter.Chart.destroy();
		Presenter.Chart = new Chart(ctx, config);
	},
	ShowRoll: function (rollDto) {
		let state = 0;
		let intervalId = -1;
		$(ConstHtmlIds.RollName).text(rollDto.stockName);
		log('Showing Roll Stock');
		let intervalFunc = function () {
			if (state === 0) {
				log('Showing Roll Func');
				$(ConstHtmlIds.RollFunc).text(rollDto.func);
			}
			else if (state === 1) {
				log('Showing Roll Amount');
				$(ConstHtmlIds.RollAmount).text(rollDto.amount);
			}
			else if (state === 2) {
				log('Updating Chart');
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
};

$(document).ready(function () {
	clickHandler = ("ontouchstart" in window ? "touchend" : "click");

	// Disable buttons until server connection is established
	$(ConstHtmlIds.CreateGame).prop('disabled', true);
	$(ConstHtmlIds.JoinGame).prop('disabled', true);

	Connection.Init();
});

//#region Cookies

// Based on https://stackoverflow.com/questions/5639346/what-is-the-shortest-function-for-reading-a-cookie-by-name-in-javascript
function createCookie(name, value, days) {
	if (days) {
		var date = new Date();
		date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
		var expires = "; expires=" + date.toGMTString();
	} else {
		var expires = "";
	}
	document.cookie = name + "=" + value + expires + "; path=/";
}

function getCookieValue(cookieName) {
	var matched = document.cookie.match("(^|[^;]+)\\s*" + cookieName + "\\s*=\\s*([^;]+)");
	return matched ? matched.pop() : "";
}

function deleteCookie(name) {
	createCookie(name, "", -1);
}

//#endregion
