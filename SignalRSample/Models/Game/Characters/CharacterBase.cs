﻿using Models.DataTransferObjects;
using Models.Game;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The base class for characters
	/// </summary>
	public abstract class CharacterBase
	{
		protected Dictionary<string, int> m_holdingChanges;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stocks">The stocks in the game.</param>
		protected CharacterBase(IEnumerable<string> stocks)
		{
			m_holdingChanges = new Dictionary<string, int>();
			foreach (var stock in stocks)
			{
				m_holdingChanges.Add(stock, 0);
			}
		}

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public abstract string Name { get; }

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
		public virtual bool GetsFirstRollReveal => false;

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the divedend amount for this character.
		/// </summary>
		/// <param name="stockValue">The value of the stock that is dividending.</param>
		/// <param name="ogirnalDiv">The original dividend amount.</param>
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
			amount *= dto.IsBuyTransaction ? 1 : -1;
			m_holdingChanges[dto.StockName] += amount;
		}

		/// <summary>
		/// Calculate the rebate amount this character gets at the end of an open market.
		/// </summary>
		/// <param name="stockValues">The current market stock values.</param>
		/// <returns>The rebate amount.</returns>
		public virtual int CalculateMarketRebateAmount(Dictionary<string, Stock> stockValues)
		{
			return 0;
		}

		/// <summary>
		/// Calculate the rebate amount this character gets at after a stock crashes.
		/// </summary>
		/// <param name="totalSharesLost">The total shares that were lost.</param>
		/// <returns>The rebate amount.</returns>
		public virtual int CalculateCrashRebateAmount(int totalSharesLost)
		{
			return 0;
		}

		/// <summary>
		/// Resets the holding changes.
		/// </summary>
		public void ResetHoldingChanges()
		{
			foreach(KeyValuePair<string, int> kvp in m_holdingChanges)
			{
				m_holdingChanges[kvp.Key] = 0;
			}
		}

		#endregion
	}
}
