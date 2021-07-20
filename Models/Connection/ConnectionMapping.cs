using System.Collections.Generic;
using System.Linq;

namespace StockTickerDotNetCore.Models.Connection
{
   public class ConnectionMapping<T>
   {
      private readonly Dictionary<T, User> m_connections =
          new Dictionary<T, User>();

      public int Count
      {
         get
         {
            return m_connections.Count;
         }
      }

      /// <summary>
      /// Add a connection.
      /// </summary>
      /// <param name="key">The key.</param>
      /// <param name="connectionId">The connection id.</param>
      public void Add(T key, string connectionId)
      {
         lock (m_connections)
         {
            if (!m_connections.TryGetValue(key, out var user))
            {
               user = new User(key.ToString());
               m_connections.Add(key, user);
            }

            lock (user.Connections)
            {
               user.Connections.Add(connectionId);
            }
         }
      }

      /// <summary>
      /// Get the user associated with a given key.
      /// </summary>
      /// <param name="key">The associated key</param>
      /// <returns>A <see cref="User"/> or null if none found.</returns>
      public User GetUser(T key)
      {
         if (m_connections.TryGetValue(key, out var user))
         {
            return user;
         }
         return null;
      }

      /// <summary>
      /// Get all connections associated with a given key.
      /// </summary>
      /// <param name="key">The associated key</param>
      /// <returns>An <see cref="IEnumerable{string}"/> of connection ids.</returns>
      public IEnumerable<string> GetConnections(T key)
      {
         if (m_connections.TryGetValue(key, out var user))
         {
            return user.Connections;
         }

         return Enumerable.Empty<string>();
      }

      /// <summary>
      /// Removes the given connection.
      /// </summary>
      /// <param name="key">The key associated with the connection.</param>
      /// <param name="connectionId">The connection to remove.</param>
      public void Remove(T key, string connectionId)
      {
         lock (m_connections)
         {
				if (!m_connections.TryGetValue(key, out var user))
				{
					return;
				}

				lock (user.Connections)
            {
               user.Connections.Remove(connectionId);

               if (user.Connections.Count == 0)
               {
                  m_connections.Remove(key);
               }
            }
         }
      }
   }
}
