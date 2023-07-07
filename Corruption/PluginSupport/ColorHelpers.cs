using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Corruption.PluginSupport
{
	public static class ColorHelpers
	{
		public static string ToHexString(this Color color) => color.PackedValue.ToString("x8");

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

		public static string ColorText(this Color color, object obj) => $"[c/{color.Hex3()}:{obj.ToString()}]";
	}
}
