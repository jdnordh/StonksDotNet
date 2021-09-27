
using System.Linq;
using Models.DataTransferObjects;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The base class for characters
	/// </summary>
	public class HoldMasterCharacter : CharacterBase
	{
		private const decimal StartingExtraDividendPercentage = 0.05M;
		private const decimal DividendPercentageIncrease = 0.05M;
		private decimal m_currentDividendBonus = StartingExtraDividendPercentage;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Master of the Hold";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => $"This character gets paid {Num(StartingExtraDividendPercentage * 100)}% more dividends. That value can increase by making correct market predictions.";

		/// <summary>
		/// The description of this chacter.
		/// </summary>
		public override string DetailedInformation => $"Current dividend bonus: {Num(m_currentDividendBonus)}. As the Master of the Hold, you start off getting {Num(StartingExtraDividendPercentage * 100)}% more dividends. You also have the ability to make market predictions for what will happen in the next round. If you make a correct prediction, your divend bonus will increase by {Num(DividendPercentageIncrease * 100)}%.";

		/// <summary>
		/// Whether or not the character gets a vote to push down a stock.
		/// </summary>
		public override bool GetsPrediction => true;

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 3;

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the divedend amount for this character.
		/// </summary>
		/// <param name="stockValue">The value of the stock that is dividending.</param>
		/// <param name="originalDiv">The original dividend amount.</param>
		/// <returns>The adjusted amout.</returns>
		public override decimal GetDivedendAmount(decimal stockValue, decimal originalDiv)
		{
			return originalDiv + m_currentDividendBonus;
		}

		/// <summary>
		/// Called when a prediction is correct.
		/// </summary>
		public override void PredictionWasCorrect()
		{
			m_currentDividendBonus += DividendPercentageIncrease;
		}

		#endregion
	}
}
