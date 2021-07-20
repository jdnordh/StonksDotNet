// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

"use strict";
var clickHandler;

// TODO Redo this part...
var Connection = {
   ClientMethods: {
      GameCreated: "gameCreated",
      GameJoined: "gameJoined",
      InventoryUpdated: "inventoryUpdated",
      MarketUpdated: "marketUpdated",
      TransactionFailed: "transactionFailed",
   },
   ServerMethods: {
      CreateGame: "CreateGame",
      JoinGame: "JoinGame",
   },
   Init: function () {
      Connection.Hub = new signalR.HubConnectionBuilder()
         .configureLogging(signalR.LogLevel.Debug)
         .withUrl("/gameHub").build();

      connection.on("gameCreated", function (gameId) {
         // TODO Show screen with gameId, start game button, and stop game button.
      });

      connection.on("gameJoined", function (success) {
         // TODO Show waiting for game to start screen
      });

      connection.on("moneyUpdated", function (marketDto) {
         // TODO Update local variable and refresh display
      });

      connection.on("marketOpen", function (isMarketOpen) {
         if (isMarketOpen) {
            // TODO Set screen to market open page
         }
         else {
            // TODO set screen to gameplay screen
         }
      });

      connection.start().then(function () {

         // TODO Startup things
         connection.invoke(Game.ServerMethods.CreateGame, {});
      }).catch(function (err) {
         return console.error(err.toString());
      });
   },

   BuyStock: function (stockName, amount) {
      // TODO
      console.log('Bought ' + amount + ' ' + stockName);
   },
   SellStock: function (stockName, amount) {
      // TODO
      console.log('Sold ' + amount + ' ' + stockName);
   },
   CreateGame: function () {
      // TODO
      console.log('Created game.');
	},
   JoinGame: function (username) {
      // TODO
      console.log('Joined game with username ' + username + '.');
   },
   StartGame: function () {
      console.log('Started game.');
	},
   OnServerUpdate: function () {
      // TODO

      let updateMethod = ScreenOps.StateBuildMethod[ScreenOps.State];
      if (updateMethod) {
         updateMethod();
		}
	},

   CurrentData: {
      Holdings: {
         Gold: 3000,
         Silver: 0,
         Oil: 1000,
         Bonds: 500,
         Industry: 0,
         Grain: 2500
      },
      StockDictionary: {
         Gold: 40,
         Silver: 75,
         Oil: 165,
         Bonds: 120,
         Industry: 95,
         Grain: 130
      },
      Money: 2500,
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
      let stockValue = Connection.CurrentData.StockDictionary[stockName];
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
   MakeMarketClosedScreen: function () {
      return '<p class="market-closed">Market Closed</p>';
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
      html += (stockValue / 100).toLocaleString(undefined, { minimumFractionDigits: 2 });
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
   },
   StateBuildMethod: {
      MarketOpenBuy: function () {
         ScreenOps.SwitchToOpenMarket(true);
      },
      MarketOpenSell: function () {
         ScreenOps.SwitchToOpenMarket(false);
      },
   },
   SwitchToClosedMarket: function () {
      let mainGrid = $(ConstHtmlIds.MainGrid);
      mainGrid.empty();
      mainGrid.append(HtmlGeneration.MakeMarketClosedScreen());
      ScreenOps.State = ScreenOps.States.MarketClosed;
   },
   SwitchToStartGameMenu: function () {
      let mainGrid = $(ConstHtmlIds.MainGrid);
      mainGrid.empty();
      mainGrid.append(HtmlGeneration.MakeStartGameMenu());
      $(ConstHtmlIds.StartGame).on(clickHandler, function () {
         Connection.StartGame();
         Presenter.CreatePlot();
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

      for (let stockName in Connection.CurrentData.StockDictionary) {
         if (Connection.CurrentData.StockDictionary.hasOwnProperty(stockName)) {
            let stockValue = Connection.CurrentData.StockDictionary[stockName];
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

         // TODO Switch this
         ScreenOps.SwitchToOpenMarket(true);
      });
	},
};

var Presenter = {
   CreatePlot: function () {
      let mainGrid = $(ConstHtmlIds.MainGrid);
      mainGrid.empty();
      mainGrid.append(HtmlGeneration.MakePresenter());

      let stockNames = [];
      let values = [];
      let colors = ["#ff0000", "#ff00ff", "#ffff00", "#0000ff", "#00ffff", "#00ff00"];
      for (let stockName in Connection.CurrentData.StockDictionary) {
         if (Connection.CurrentData.StockDictionary.hasOwnProperty(stockName)) {
            stockNames.push(stockName);
            values.push(Connection.CurrentData.StockDictionary[stockName]);
         }
      }
      var trace1 = {
         x: stockNames,
         y: values,
         text: values.map(String),
         marker: {
            color: colors
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
   CreateChart: function () {
      let mainGrid = $(ConstHtmlIds.MainGrid);
      mainGrid.empty();
      mainGrid.append(HtmlGeneration.MakePresenter());

      let labels = [];
      let values = [];
      let colors = ["#ff0000", "#ff00ff", "#ffff00", "#0000ff", "#00ffff", "#00ff00"];
      for (let stockName in Connection.CurrentData.StockDictionary) {
         if (Connection.CurrentData.StockDictionary.hasOwnProperty(stockName)) {
            labels.push(stockName);
            values.push(Connection.CurrentData.StockDictionary[stockName]);
         }
      }
      const data = {
         labels: labels,
         datasets: [
            {
               data: values,
               borderColor: colors,
               backgroundColor: colors,
            }
         ]
      };
      let config = {
         type: 'bar',
         data: data,
         options: {
            responsive: true,
            plugins: {
               legend: {
                  position: 'top',
               },
               title: {
                  display: false,
               },
               scales: {
                  yAxes: [{
                     display: true,
                     ticks: {
                        suggestedMin: 0,
                        suggestedMax: 200,
                     }
                  }]
               },
            },
            
            legend: {
               display: false
            },
            tooltips: {
               callbacks: {
                  label: function (tooltipItem) {
                     return tooltipItem.yLabel;
                  }
               }
            }
         },
      };
      let myChart = new Chart(document.getElementById(ConstHtmlIds.PresenterChart).getContext('2d'), config);
   },
   StartTimer: function(seconds, display) {
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

   // Attach menu handlers
   $(ConstHtmlIds.CreateGame).on(clickHandler, function () {
      Connection.CreateGame();
      ScreenOps.SwitchToStartGameMenu();
   });
   $(ConstHtmlIds.JoinGame).on(clickHandler, function () {
      ScreenOps.SwitchToJoinMenu(false);
   });

   // TODO Other start up stuff??
   //ScreenOps.SwitchToOpenMarket(true);

   // TODO Any cookies like username or what ever
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
