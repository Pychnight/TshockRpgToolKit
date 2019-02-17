using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace Corruption.PluginSupport
{
	public static class ColorHelpers
	{
		public static string ToHexString(this Color color)
		{
			return color.PackedValue.ToString("x8");
		}

		public static Color FromHexString(string value)
		{
			try
			{
				var packed = Convert.ToUInt32(value, 16);
				var result = new Color(packed);

				return result;
			}
			catch
			{
				return Color.White;
			}
		}

		public static string ColorText(this Color color, object obj)
		{
			return $"[c/{color.Hex3()}:{obj.ToString()}]";
		}
	}
}
