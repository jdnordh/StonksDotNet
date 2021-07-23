// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

"use strict";
var clickHandler;

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

var Connection = {
    ClientType: -1,
    ClientTypes: {
        Player: 0,
        Observer: 1,
    },
    ClientMethods: {
        GameCreated: "gameCreated",
        CreateGameUnavailable: "createGameUnavailable",
        GameJoined: "gameJoined",
        GameStarted: "gameStarted",
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
        RequestTransaction: "RequestTransaction",
    },
    Init: function (onConnectionStarted) {
        Connection.Hub = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

        Connection.Hub.on(Connection.ClientMethods.GameCreated, function () {
            log('Game created');
            Connection.ClientType = Connection.ClientTypes.Observer;
            ScreenOps.SwitchToStartGameMenu();
        });

        Connection.Hub.on(Connection.ClientMethods.GameJoined, function (playerInventoryDto) {
            log('Game joined');
            log(playerInventoryDto);
            Connection.ClientType = Connection.ClientTypes.Player;
            Connection.UpdateInventory(playerInventoryDto);
            Connection.CurrentData.Username = playerInventoryDto.username;
            ScreenOps.SwitchToWaitingMenu();
        });

        Connection.Hub.on(Connection.ClientMethods.CreateGameUnavailable, function () {
            log('Game has already started');
            $(ConstHtmlIds.CreateGame).prop('disabled', true);
        });

        Connection.Hub.on(Connection.ClientMethods.GameStarted, function (marketDto) {
            log('Game started');
            Connection.UpdateStockValues(marketDto);
            if (Connection.ClientType === Connection.ClientTypes.Observer) {
                Presenter.CreatePlot();
            }
        });

        Connection.Hub.on(Connection.ClientMethods.InventoryUpdated, function (playerInventoryDto) {
            log('Inventory updated');
            log(playerInventoryDto);
            if (Connection.ClientType === Connection.ClientTypes.Player) {
                Connection.UpdateInventory(playerInventoryDto);
            }
        });

        Connection.Hub.on(Connection.ClientMethods.MarketUpdated, function (marketDto) {
            let state = marketDto.isOpen ? 'open' : 'closed';
            log('Market updated: market is ' + state);
            log(marketDto);
            Connection.UpdateStockValues(marketDto);
            if (Connection.ClientType === Connection.ClientTypes.Observer) {
                if (marketDto.isOpen) {
                    Presenter.SetMarketOpen(marketDto.marketOpenTimeInSeconds);
                }
            }
            else {
                if (marketDto.isOpen) {
                    ScreenOps.SwitchToOpenMarket(true);
                }
                else {
                    ScreenOps.SwitchToClosedMarket();
                }
            }
        });

        Connection.Hub.on(Connection.ClientMethods.Rolled, function (marketDto) {
            if (Connection.ClientType === Connection.ClientTypes.Observer) {
                Connection.UpdateStockValues(marketDto);
            }
        });

        Connection.Hub.start().then(function () {
            onConnectionStarted();
        }).catch(function (err) {
            return console.error(err.toString());
        });
    },
    UpdateStockValues: function (marketDto) {
        for (let stockName in marketDto.stocks) {
            if (marketDto.stocks.hasOwnProperty(stockName)) {
                let stockDto = marketDto.stocks[stockName];
                Connection.CurrentData.StockValues[stockName] = stockDto.value;
                Connection.CurrentData.StockColors[stockName] = stockDto.color;
            }
        }
    },
    UpdateInventory: function (playerInventoryDto) {
        for (let stockName in playerInventoryDto.holdings) {
            if (playerInventoryDto.holdings.hasOwnProperty(stockName)) {
                let amount = playerInventoryDto.holdings[stockName];
                Connection.CurrentData.Holdings[stockName] = amount;
            }
        }
        Connection.SetMoney(playerInventoryDto.money);
    },
    RequestTransaction: function (stockName, isBuy, amount) {
        Connection.Hub.invoke(Connection.ServerMethods.RequestTransaction, stockName, isBuy, Number(amount)).catch(function (err) {
            return console.error(err.toString());
        });
    },
    BuyStock: function (stockName, amount) {
        log('Bought ' + amount + ' ' + stockName);
        Connection.RequestTransaction(stockName, true, amount);
    },
    SellStock: function (stockName, amount) {
        log('Sold ' + amount + ' ' + stockName);
        Connection.RequestTransaction(stockName, false, amount);
    },
    CreateGame: function () {
        log('Created game.');
        Connection.Hub.invoke(Connection.ServerMethods.CreateGame).catch(function (err) {
            return console.error(err.toString());
        });
    },
    JoinGame: function (username) {
        log('Joined game with username ' + username + '.');
        Connection.Hub.invoke(Connection.ServerMethods.JoinGame, username).catch(function (err) {
            return console.error(err.toString());
        });
    },
    StartGame: function () {
        console.log('Started game.');
        Connection.Hub.invoke(Connection.ServerMethods.StartGame).catch(function (err) {
            return console.error(err.toString());
        });
    },
    OnServerUpdate: function () {
        let updateMethod = ScreenOps.StateBuildMethod[ScreenOps.State];
        if (updateMethod) {
            updateMethod();
        }
    },
    CurrentData: {
        Username: "StonkMaster",
        Holdings: {},
        StockValues: {},
        StockColors: {},
        Money: 0,
    },
    SetMoney: function (money) {
        Connection.CurrentData.Money = money;
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
    Buy: "#buy",
    BuyAmount: "#buyAmount",
    Sell: "#sell",
    SellAmount: "#sellAmount",
    Cancel: "#cancel",
    CreateGame: "#createGame",
    JoinGame: "#joinGame",
    Username: "#username",
    StartGame: "#startGame",
    PresenterChart: "presenterChart",
    PresenterText: "presenterText",
}

var HtmlGeneration =
{
    MakePreBuyScreen: function (stockName) {
        let html = '<div class="fill grid-row-2"><p class="buy-sell-prompt">How much ';
        html += stockName;
        html += ' would you like to buy?</p><div class="buy-sell-control"><select class="buy-sell-prompt grid-column-2 grid-row-1 fill" name="amount" id="buyAmount">';
        let stockValue = Connection.CurrentData.StockValues[stockName];
        let money = Connection.CurrentData.Money;
        for (let i = 0; i <= (money / (stockValue / 100)); i += 500) {
            html += '<option value="';
            html += i;
            html += '">'
            html += i;
            html += '</option>';
        }
        html += '</select><button class="btn btn-success buy-sell-button fill grid-column-3 grid-row-1" id="buy">Buy</button><div class="buy-sell-cancel fill"><button class="btn btn-danger buy-sell-button fill grid-column-2" id="cancel">Cancel</button></div></div></div >';
        return html;
    },
    MakePreSellScreen: function (stockName) {
        let html = '<div class="fill grid-row-2"><p class="buy-sell-prompt">How much ';
        html += stockName;
        html += ' would you like to sell?</p><div class="buy-sell-control"><select class="buy-sell-prompt grid-column-2 grid-row-1 fill" name="amount" id="sellAmount">';
        let stockAmount = Connection.CurrentData.Holdings[stockName];
        for (let i = 0; i <= stockAmount; i += 500) {
            html += '<option value="';
            html += i;
            html += '">'
            html += i;
            html += '</option>';
        }
        html += '</select><button class="btn btn-info buy-sell-button fill grid-column-3 grid-row-1" id="sell">Sell</button><div class="buy-sell-cancel fill"><button class="btn btn-danger buy-sell-button fill grid-column-2" id="cancel">Cancel</button></div></div></div>';
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
        let html = '<div class="grid-row-1 grid-fill buy-sell-div"><div class="grid-row-1"><p class="market-closed">Market Closed</p></div><div class="grid-row-2"><p id="money">$'
        html += money;
        html += '</p></div></div><div class="grid-row-2 scrollviewer-vertical" id="stockList"></div>';
        return html;
    },
    MakeMarketScreen: function (money) {
        let html = '<div class="grid-row-1 grid-fill buy-sell-div"><div class="grid-row-1 buy-sell-buttons" id ="buySell"><button type="button" class="grid-column-2 buy-sell-text btn btn-primary" id="buyTab">Buy</button><button type="button" class="grid-column-3 buy-sell-text btn btn-outline-primary" id="sellTab">Sell</button></div><div class="grid-row-2"><p id="money">$';
        html += money;
        html += '</p></div></div><div class="grid-row-2 scrollviewer-vertical" id="stockList"></div>';
        return html;
    },
    MakeBuyStockBanner: function (stockName, stockValue) {
        let id = stockName + 'buy';
        let html = '<div class="stock-banner"><p class="grid-column-1 stock-text">';
        html += stockName;
        html += '</p ><p class="grid-column-2 stock-text">';
        html += Unit.PercentToDecimalString(stockValue);
        html += '</p > <button type="button" class="grid-column-3 btn btn-success stock-text" id="';
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
        html += '</p ><p class="grid-column-2 stock-text">';
        html += amountHeld;
        html += '</p > <button type="button" class="grid-column-3 btn btn-info stock-text" id="';
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
        return '<div class="center-absolute menu-grid"><div class="menu-sub-grid"> <label for="username" class="menu-text">Username:</label> <input autocomplete="off" type="text" class="menu-text" id="username" /></div> <button id="joinGame" class="btn btn-primary grid-row-2 menu-button">Join Game</button></div>';
    },
    MakeStartGameMenu: function () {
        return '<div class="center-absolute menu-grid"> <button id="startGame" class="btn btn-primary menu-button">Start Game</button></div>';
    },
    MakePresenter: function () {
        //return '<div id="presenter" class="fill"><h1 id="presenterText" class="grid-column-2 grid-row-1 menu-text">Market Closed</h1><div class="chart-div center-absolute"> <canvas id="presenterChart"></canvas></div></div>';
        return '<div id="presenter" class="fill"><h1 id="presenterText" class="grid-column-2 grid-row-1 menu-text">Market Closed</h1><div class="chart-div center-absolute" id="presenterChart"></div></div>';
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
            ScreenOps.SwitchToOpenMarket(true);
        },
        MarketOpenSell: function () {
            ScreenOps.SwitchToOpenMarket(false);
        },
        MarketClosed: function () {
            ScreenOps.SwitchToClosedMarket();
        },
        Waiting: function () {
            ScreenOps.SwitchToWaitingMenu();
        },
    },
    SwitchToClosedMarket: function () {
        ScreenOps.State = ScreenOps.States.MarketClosed;
        let mainGrid = $(ConstHtmlIds.MainGrid);
        mainGrid.empty();
        mainGrid.append(HtmlGeneration.MakeMarketClosedScreen(Connection.CurrentData.Money));
        let list = $(ConstHtmlIds.StockList);

        for (let stockName in Connection.CurrentData.Holdings) {
            if (Connection.CurrentData.Holdings.hasOwnProperty(stockName)) {
                let amountHeld = Connection.CurrentData.Holdings[stockName];
                if (amountHeld > 0) {
                    list.append(HtmlGeneration.MakeStockBannerMarketClosed(stockName, amountHeld));
                }
            }
        }
    },
    SwitchToStartGameMenu: function () {
        let mainGrid = $(ConstHtmlIds.MainGrid);
        mainGrid.empty();
        mainGrid.append(HtmlGeneration.MakeStartGameMenu());
        $(ConstHtmlIds.StartGame).on(clickHandler, function () {
            Connection.StartGame();
        });
    },
    SwitchToOpenMarket: function (isBuy) {
        let mainGrid = $(ConstHtmlIds.MainGrid);
        mainGrid.empty();
        mainGrid.append(HtmlGeneration.MakeMarketScreen(Connection.CurrentData.Money));
        mainGrid.append(HtmlGeneration.MakeBuyStockBanner());

        ScreenOps.AttachOpenMarketTabHandlers();
        if (isBuy) {
            ScreenOps.SwitchToBuy();
        }
        else {
            ScreenOps.SwitchToSell();
        }
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

        for (let stockName in Connection.CurrentData.StockValues) {
            if (Connection.CurrentData.StockValues.hasOwnProperty(stockName)) {
                let stockValue = Connection.CurrentData.StockValues[stockName];
                let generated = HtmlGeneration.MakeBuyStockBanner(stockName, stockValue);
                list.append(generated.html);
                $(generated.id).on(clickHandler, function () {
                    ScreenOps.PreBuyStock(stockName);
                });
            }
        }
    },
    SwitchToSell: function () {
        ScreenOps.State = ScreenOps.States.MarketOpenSell;
        let list = $(ConstHtmlIds.StockList);
        list.empty();

        for (let stockName in Connection.CurrentData.Holdings) {
            if (Connection.CurrentData.Holdings.hasOwnProperty(stockName)) {
                let amountHeld = Connection.CurrentData.Holdings[stockName];
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
        $(ConstHtmlIds.JoinGame).on(clickHandler, function () {
            let username = $(ConstHtmlIds.Username).val();
            Connection.JoinGame(username);
        });
    },
    SwitchToWaitingMenu: function () {
        ScreenOps.State = ScreenOps.States.Waiting;
        let mainGrid = $(ConstHtmlIds.MainGrid);
        mainGrid.empty();

        mainGrid.append(HtmlGeneration.MakeWaitingScreen(Connection.CurrentData.Username, Connection.CurrentData.Money));
    },
};

var Presenter = {
    CreatePlot: function () {
        let mainGrid = $(ConstHtmlIds.MainGrid);
        mainGrid.empty();
        mainGrid.append(HtmlGeneration.MakePresenter());

        let stockNames = [];
        let values = [];
        let stockColors = [];
        for (let stockName in Connection.CurrentData.StockValues) {
            if (Connection.CurrentData.StockValues.hasOwnProperty(stockName)) {
                stockNames.push(stockName);
                values.push(Connection.CurrentData.StockValues[stockName]);
                stockColors.push(Connection.CurrentData.StockColors[stockName]);
            }
        }
        var trace1 = {
            x: stockNames,
            y: values,
            text: values.map(String),
            marker: {
                color: stockColors
            },
            type: 'bar'
        };

        var data = [trace1];
        var layout = {
            title: '',
            font: {
                family: 'var(--bs-font-sans-serif)',
                size: 36,
                color: '#7f7f7f'
            },
            yaxis: { range: [0, 200] },
            showlegend: false,
        };

        Plotly.newPlot(ConstHtmlIds.PresenterChart, data, layout);
    },
    StartTimer: function (seconds, display) {
        var remainingSeconds = seconds;
        let interval = setInterval(function () {

            remainingSeconds = remainingSeconds < 10 ? "0" + remainingSeconds : remainingSeconds;

            display.text(remainingSeconds);

            if (--remainingSeconds < 0) {
                clearInterval(interval);
            }
        }, 1000);
    },
    SetMarketOpen: function (startingSeconds) {
        let presenterText = $(ConstHtmlIds.PresenterText);

        Presenter.StartTimer(startingSeconds, {
            text: function (value) {
                presenterText.val(value);
            }
        });
    }
};

$(document).ready(function () {
    clickHandler = ("ontouchstart" in window ? "touchend" : "click");

    let onConnectionStarted = function () {
        let createGameButton = $(ConstHtmlIds.CreateGame);
        let joinGameButton = $(ConstHtmlIds.JoinGame);

        // Attach menu handlers
        createGameButton.on(clickHandler, function () {
            Connection.CreateGame();
        });
        createGameButton.prop('disabled', false);
        joinGameButton.on(clickHandler, function () {
            ScreenOps.SwitchToJoinMenu(false);
        });
        joinGameButton.prop('disabled', false);
    };

    Connection.Init(onConnectionStarted);
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
