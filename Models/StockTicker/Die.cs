using System;
using System.Collections.Generic;
using System.Linq;

namespace Models.StockTicker.GameClasses
{
	public class Die<TResult>
	{
		public List<TResult> Results { get; set; }

		public TResult Roll()
		{
			if (Results == null || !Results.Any())
			{
				throw new InvalidOperationException("Die results empty.");
			}
			var rand = new Random();
			int index = rand.Next(0, Results.Count);
			return Results[index];
		}
	}
}
