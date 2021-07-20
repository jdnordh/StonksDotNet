using System;
using System.Collections.Generic;
using System.Linq;

namespace StockTickerDotNetCore.Models.Utilities
{
	public class ColorManager
	{
		private Dictionary<string, bool> m_colors;

		public ColorManager()
		{
			m_colors = new Dictionary<string, bool>() 
			{
				{ "aqua", false},
				{ "deeppink", false},
				{ "aquamarine", false},
				{ "blueviolet", false},
				{ "forestgreen", false},
				{ "brown", false},
				{ "dodgerblue", false},
				{ "goldenrod", false},
				{ "crimson", false},
				{ "darkgrey", false},
				{ "indigo", false},
				{ "gold", false},
				{ "chocolate", false},
				{ "mediumpurple", false},
				{ "limegreen", false},
				{ "olive", false},
				{ "purple", false},
				{ "skyblue", false},
			};
		}

		public void SetColorInUse(string color)
		{
			if (IsColorInUse(color))
			{
				throw new InvalidOperationException($"Color {color} already in use.");
			}
			if (m_colors.ContainsKey(color))
			{
				m_colors[color] = true;
			}
			else
			{
				m_colors.Add(color, true);
			}
		}

		public bool IsColorInUse(string color)
		{
			if (!m_colors.ContainsKey(color))
			{
				return true;
			}
			return m_colors[color];
		}

		public string GetUnusedColor()
		{
			string color = m_colors.First(kvp => kvp.Value == false).Key;
			m_colors[color] = true;
			return color;
		}
	}
}
