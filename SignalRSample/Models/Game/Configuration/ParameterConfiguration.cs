using System;

namespace StonkTrader.Models.Game.Configuration
{
	public class ParameterConfiguration
	{
		public int Min => Range.Start.Value;
		public int Max => Range.End.Value;
		public Range Range { get; }
		public int DefaultValue { get; }

		public ParameterConfiguration(Index min, Index max, int defaultValue)
		{
			Range = new Range(min, max);
			DefaultValue = defaultValue;
		}
	}
}
