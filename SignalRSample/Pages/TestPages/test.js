

var log = function (msg) {
	console.log(msg);
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
}

var Connection = {
	CurrentData: {
		Username: "StonkMaster",
		Holdings: {
			Gold: 1500,
			Silver: 2500,
			Oil: 500,
			Bonds: 1000,
			Industry: 2000,
			Grain: 500
		},
		StockValues: {
			Gold: 165,
			Silver: 075,
			Oil: 170,
			Bonds: 105,
			Industry: 100,
			Grain: 40
		},
		StockColors: {
			Gold: "#FFD700",
			Silver: "#C0C0C0",
			Oil: "#4682B4",
			Bonds: "#228B22",
			Industry: "#DA70D6",
			Grain: "#F0E68C"
		},
		Money: 1250,
	},
};

var chart = undefined;

function getChartData() {
	let stockNames = [];
	let stockValues = [];
	let stockColors = [];
	for (let stockName in Connection.CurrentData.StockValues) {
		if (Connection.CurrentData.StockValues.hasOwnProperty(stockName)) {
			stockNames.push(stockName);
			stockValues.push(Connection.CurrentData.StockValues[stockName]);
			stockColors.push(Connection.CurrentData.StockColors[stockName]);
		}
	}
	return {
		stockNames: stockNames,
		stockValues: stockValues,
		stockColors: stockColors,
	}
}

function createChart() {
	if (chart) {
		return;
	}

	let stockData = getChartData();
	let canvas = document.getElementById(ConstHtmlIds.PresenterChart);
	let ctx = canvas.getContext('2d');

	Chart.defaults.font.size = 36;
	let data = {
		labels: stockData.stockNames,
		datasets: [
			{
				label: "Stonks",
				data: stockData.stockValues,
				backgroundColor: stockData.stockColors
			}
		]
	};

	let config = {
		type: 'bar',
		data: data,
		options: {
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
			scales: {
				yAxes: {
					min: 0,
					max: 200
				}

			}
		}
	};

	chart = new Chart(ctx, config);
}

function showRoll(rollDto) {
	log('Showing roll...');
	let state = 0;
	let intervalId = -1;
	log('Stock name');
	$(ConstHtmlIds.RollName).text(rollDto.stockName);
	let intervalFunc = function () {
		if (state === 0) {
			log('Stock func');
			$(ConstHtmlIds.RollFunc).text(rollDto.func);
		}
		else if (state === 1) {
			log('Stock amount');
			$(ConstHtmlIds.RollAmount).text(rollDto.amount);
		}
		else if (state === 2) {
			log('Chart update');
		}
		else {
			log('Finished showing roll');
			clearInterval(intervalId);
		}
		state++;
	};
	intervalId = setInterval(intervalFunc, 500);
}