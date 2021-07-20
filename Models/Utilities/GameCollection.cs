using System;
using System.Collections.Generic;

namespace StockTickerDotNetCore.Models.Utilities
{
	public class GameCollection<T>
	{
		private readonly string[] m_letters = new[]
		{
			"A",
			"B",
			"C",
			"D",
			"E",
			"F",
			"G",
			"H",
			"J",
			"K",
			"M",
			"N",
			"P",
			"R",
			"S",
			"T",
			"U",
			"V",
			"W",
			"X",
			"Y",
			"Z",
		};
		private Dictionary<string, T> m_games;

		public GameCollection()
		{
			m_games = new Dictionary<string, T>();
		}

		/// <summary>
		/// Add a game to the collection.
		/// </summary>
		/// <param name="game">The game to add.</param>
		/// <returns>The id of the new game.</returns>
		public string AddGame(T game)
		{
			string id = GetNewId();
			m_games.Add(id, game);
			return id;
		}

		/// <summary>
		/// Get a game with the given id.
		/// </summary>
		/// <param name="id">The id of the game.</param>
		/// <returns>The game or default if not found.</returns>
		public T GetGame(string id)
		{
			if (!GameExists(id))
			{
				return default;
			}
			return m_games[id];
		}

		/// <summary>
		/// Check if a game exists.
		/// </summary>
		/// <param name="id">The game id.</param>
		/// <returns>True if the game exists.</returns>
		public bool GameExists(string id)
		{
			return m_games.ContainsKey(id);
		}

		/// <summary>
		/// Gets a new id for a game.
		/// </summary>
		/// <returns>The unused id.</returns>
		private string GetNewId()
		{
			var random = new Random();
			string id;
			do
			{
				id = "";
				for (int i = 0; i < 4; i++)
				{
					id += m_letters[random.Next(0, m_letters.Length - 1)];
				}
			} while (m_games.ContainsKey(id));
			return id;
		}
	}
}
