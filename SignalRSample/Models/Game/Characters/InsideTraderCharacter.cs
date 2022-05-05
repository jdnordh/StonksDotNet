
using System;
using System.Collections.Generic;

namespace StonkTrader.Models.Game.Characters
{
	/// <summary>
	/// The insider trader character that gets special privileges to see the first roll of a round.
	/// </summary>
	public class InsideTraderCharacter : CharacterBase
	{
		private const decimal MaxAuditChance = 0.4M;
		private const decimal AuditPercentage = 0.3M;
		private const int RoundsPerFreePreview = 5;

		private int m_riskFreePreviews;
		private decimal m_auditChanceIncrease;
		private decimal m_auditChance = 0M;
		private int m_rollPreviewTimes = 0;

		#region Properties

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Name => "Inside Trader";

		/// <summary>
		/// The name of this chacter.
		/// </summary>
		public override string Description => "This character gives you inside information on market changes in upcoming rounds. However, if you use your influence too much, you may be audited.";

		/// <summary>
		/// The id of this chacter.
		/// </summary>
		public override int Id => 1;

		/// <summary>
		/// Whether or not the character gets a reveal of the first roll of each round.
		/// </summary>
		public override bool GetsRollPreviews => true;

		#endregion

		#region Public Methods

		/// <inheritdoc/>
		protected override void GameRoundsSet()
		{
			m_riskFreePreviews = GameRounds / RoundsPerFreePreview;
			m_auditChanceIncrease = MaxAuditChance / (decimal)(GameRounds - m_riskFreePreviews);
		}

		/// <inheritdoc/>
		public override string GetDetailedInformation()
		{
			string preamble = $"You currently have a {Num(m_auditChance * 100)}% chance of being audited. ";
			bool isPlural = m_riskFreePreviews != 1;
			return $"{preamble}As the Insider Trader, you get inside information on market changes in the upcoming round. The first {(isPlural ? m_riskFreePreviews + " " : " ")}preview{(isPlural ? "s" : "")} {(isPlural ? "are" : "is")} risk free, but after that every time you preview you have an increased chance of being audited at the end of the game. Being audited will lose you {Num(AuditPercentage * 100)}% of your net worth.";
		}

		/// <summary>
		/// Called when rolls are previewed.
		/// </summary>
		public override void PreviewedRolls()
		{
			if (m_rollPreviewTimes >= m_riskFreePreviews && m_auditChance < 1M)
			{
				m_auditChance += m_auditChanceIncrease;

				if(m_auditChance > 1M)
				{
					m_auditChance = 1M;
				}
			}
			++m_rollPreviewTimes;
		}

		/// <summary>
		/// Checks the amount the player should be audited.
		/// </summary>
		public override decimal GetAuditPercentage()
		{
			int rand = new Random().Next(1, 100);
			int auditThreshold = (int)(m_auditChance * 100);

			return rand <= auditThreshold ? AuditPercentage : 0;
		}

		#endregion
	}
}
