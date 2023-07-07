using Microsoft.Xna.Framework;

namespace CustomNpcs.Projectiles
{
	/// <summary>
	/// Was part of a hack to fit in a specific syntax into lua for custom projectiles.
	/// </summary>
	public enum Offset
	{
		Center,
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		BottomLeft,
		Left
	}

	/// <summary>
	/// This (was) a hack to fit a specific syntax into lua for custom projectiles. 
	/// </summary>
	/// <remarks>There maybe better ways to do this.</remarks>
	public static class OffsetHelper
	{
		public static Vector2 ToUnitVector(this Offset edge)
		{
			switch (edge)
			{
				case Offset.TopLeft: return new Vector2(-1, -1);
				case Offset.Top: return new Vector2(0, -1);
				case Offset.TopRight: return new Vector2(1, -1);
				case Offset.Right: return new Vector2(1, 0);
				case Offset.BottomRight: return new Vector2(1, 1);
				case Offset.Bottom: return new Vector2(0, 1);
				case Offset.BottomLeft: return new Vector2(-1, 1);
				case Offset.Left: return new Vector2(-1, 0);
				default: return new Vector2(0, 0);
			}
		}
	}
}
