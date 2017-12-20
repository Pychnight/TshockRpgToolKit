using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs.Projectiles
{
	/// <summary>
	/// Part of a hack to fit in a specific syntax into lua for custom projectiles.
	/// </summary>
	public enum NpcEdge
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
	/// This is a hack to fit a specific syntax into lua for custom projectiles. 
	/// </summary>
	/// <remarks>There maybe better ways to do this.</remarks>
	public class CenterOffsetHelper
	{
		public readonly NpcEdge TopLeft = NpcEdge.TopLeft;
		public readonly NpcEdge Top = NpcEdge.Top;
		public readonly NpcEdge TopRight = NpcEdge.TopRight;
		public readonly NpcEdge Right = NpcEdge.Right;
		public readonly NpcEdge BottomRight = NpcEdge.BottomRight;
		public readonly NpcEdge Bottom = NpcEdge.Bottom;
		public readonly NpcEdge BottomLeft = NpcEdge.BottomLeft;
		public readonly NpcEdge Left = NpcEdge.Left;
				
		internal static Vector2 GetUnitVectorFromNpcEdge(NpcEdge edge)
		{
			switch(edge)
			{
				case NpcEdge.TopLeft: return new Vector2(-1, -1);
				case NpcEdge.Top: return new Vector2(0, -1);
				case NpcEdge.TopRight: return new Vector2(1, -1);
				case NpcEdge.Right: return new Vector2(1, 0);
				case NpcEdge.BottomRight: return new Vector2(1, 1);
				case NpcEdge.Bottom: return new Vector2(0, 1);
				case NpcEdge.BottomLeft: return new Vector2(-1, 1);
				case NpcEdge.Left: return new Vector2(-1, 0);
				default: return new Vector2(0, 0);
			}
		}
	}
}
