using System.Collections.Generic;

namespace StockTickerDotNetCore.Models.Connection
{
	public class User
	{
		/// <summary>
		/// The id of this user.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// The game id that this user is a part of.
		/// </summary>
		public string GameId { get; set; }

		/// <summary>
		/// The username of this user.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// The connections that this user is registered with. May be more than one connection if user connects multiple sessions.
		/// </summary>
		public HashSet<string> Connections { get; }

		public User(string name)
		{
			Id = name;
			GameId = null;
			Connections = new HashSet<string>();
		}
	}
}
