using Models.DataTransferObjects;
using Models.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The base class for characters
	/// </summary>
	public abstract class CharacterBase
	{
		protected readonly static Func<decimal, string> Num = (d) => d.ToString("N0");
		protected readonly static Func<decimal, string> Money = (d) => d.ToString("C");

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// The description of this chacter.
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public abstract int Id { get; }

		/// <summary>
		/// Whether or not the character gets half time transactions.
		/// </summary>
		public virtual bool GetsHalfTimeTransaction => false;

		/// <summary>
		/// Whether or not the character gets a reveal of the first roll of each round.
		/// </summary>
		public virtual bool GetsRollPreviews => false;

		/// <summary>
		/// Whether or not the character gets a vote to push down a stock.
		/// </summary>
		public virtual bool GetsPushDownVote => false;

		/// <summary>
		/// Whether or not the character gets a vote to push down a stock.
		/// </summary>
		public virtual bool GetsPrediction => false;

		/// <summary>
		/// Whether or not the character gets to short a stock.
		/// </summary>
		public virtual bool GetsShort => false;

		/// <summary>
		/// If the stocks are initialized.
		/// </summary>
		public bool AreStocksInitialized { get; set; }

		/// <summary>
		/// The current market prediction. Null if none exists.
		/// </summary>
		public PredictionDto Prediction { get; set; }

		/// <summary>
		/// The current short position. Null if none exists.
		/// </summary>
		public ShortDto ShortPosition { get; set; }
		
		/// <summary>
		/// The changes to the holdings this round.
		/// </summary>
		protected Dictionary<string, int> HoldingChanges { get; private set; }

		/// <summary>
		/// The values of stocks in the current market.
		/// </summary>
		protected Dictionary<string, Stock> StockValues { get; private set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the detailed information about how to play this character.
		/// </summary>
		/// <returns>A string with detailed information.</returns>
		public abstract string GetDetailedInformation();

		/// <summary>
		/// Gets the divedend amount for this character.
		/// </summary>
		/// <param name="stockValue">The value of the stock that is dividending.</param>
		/// <param name="originalDiv">The original dividend amount.</param>
		/// <returns>The adjusted amout.</returns>
		public virtual decimal GetDivedendAmount(decimal stockValue, decimal originalDiv)
		{
			return originalDiv;
		}

		/// <summary>
		/// Records a transaction for this player.
		/// </summary>
		/// <param name="dto">The successful transaction dto.</param>
		public void RecordTransaction(PlayerTransactionDto dto)
		{
			int amount = dto.StockAmount;
			HoldingChanges[dto.StockName] += amount * (dto.IsBuyTransaction ? 1 : -1);
		}

		/// <summary>
		/// Calculate the rebate amount this character gets at the end of an open market.
		/// </summary>
		/// <returns>The rebate amount.</returns>
		public virtual int CalculateMarketRebateAmount()
		{
			return 0;
		}

		/// <summary>
		/// Calculate the rebate amount this character gets at after a stock crashes.
		/// </summary>
		/// <param name="sharesLost">The shares that were lost by the player.</param>
		/// <returns>The rebate amount.</returns>
		public virtual int CalculateCrashRebateAmount(int sharesLost)
		{
			return 0;
		}

		/// <summary>
		/// Called when a prediction is correct.
		/// </summary>
		public virtual void PredictionWasCorrect()
		{
		}

		/// <summary>
		/// Called when rolls are previewed.
		/// </summary>
		public virtual void PreviewedRolls()
		{
		}

		/// <summary>
		/// Checks the amount the player should be audited.
		/// </summary>
		public virtual decimal GetAuditPercentage()
		{
			return 0M;
		}

		/// <summary>
		/// Resets any data associated with the round.
		/// </summary>
		/// <param name="stockValues">The current market stock values.</param>
		public void PrepareForOpenMarket(Dictionary<string, Stock> stockValues)
		{
			foreach(var kvp in HoldingChanges.ToList())
			{
				HoldingChanges[kvp.Key] = 0;
			}
			StockValues = stockValues;
		}

		/// <summary>
		/// Prepares for the closed market.
		/// </summary>
		public void PrepareForClosedMarket()
		{
			StockValues = null;
		}

		/// <summary>
		/// Resets any prediction.
		/// </summary>
		public void ResetPrediction()
		{
			Prediction = null;
		}

		/// <summary>
		/// Initialize the stocks.
		/// </summary>
		/// <param name="stocks">the stock names.</param>
		public void InitializeStocks(IEnumerable<string> stocks)
		{
			HoldingChanges = new Dictionary<string, int>();
			foreach(string stock in stocks)
			{
				HoldingChanges.Add(stock, 0);
			}
		}

		#endregion
	}
}
