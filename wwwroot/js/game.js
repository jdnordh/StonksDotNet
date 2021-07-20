// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

"use strict";
var clickHandler;

var Game =
{
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
   DataTransferObjects: {},
}
// TODO Redo this part...
var Connection = {
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
   }
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

   },
   SwitchToOpenMarket: function (isBuy) {
      let mainGrid = $(ConstHtmlIds.MainGrid);
      mainGrid.empty();
      mainGrid.append(HtmlGeneration.MakeMarketScreen(Connection.CurrentData.Money));
      mainGrid.append(HtmlGeneration.MakeBuyStockBanner());

      ScreenOps.AttachOpenMarketTabHandlers();
      if (isBuy) {
         ScreenOps.State = ScreenOps.States.MarketOpenBuy;
         ScreenOps.SwitchToBuy();
      }
      else {
         ScreenOps.State = ScreenOps.States.MarketOpenBuy;
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
      let list = $(ConstHtmlIds.StockList);
      list.empty();
      list.append(HtmlGeneration.MakePreBuyScreen(stockName));

      // Add handlers
      $(ConstHtmlIds.Buy).on(clickHandler, function () {
         let stockAmount = $(ConstHtmlIds.BuyAmount).find(":selected").text();
         Connection.BuyStock(stockName, stockAmount);
         ScreenOps.SwitchToBuy();
      });
      $(ConstHtmlIds.Cancel).on(clickHandler, function () {
         ScreenOps.SwitchToBuy();
      });
   },
   PreSellStock: function (stockName) {
      let list = $(ConstHtmlIds.StockList);
      list.empty();
      list.append(HtmlGeneration.MakePreSellScreen(stockName));

      // Add handlers
      $(ConstHtmlIds.Sell).on(clickHandler, function () {
         let stockAmount = $(ConstHtmlIds.SellAmount).find(":selected").text();
         Connection.SellStock(stockName, stockAmount);
         ScreenOps.SwitchToSell();
      });
      $(ConstHtmlIds.Cancel).on(clickHandler, function () {
         ScreenOps.SwitchToSell();
      });
   },
};

$(document).ready(function () {
   clickHandler = ("ontouchstart" in window ? "touchend" : "click");

   ScreenOps.SwitchToOpenMarket(true);

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
