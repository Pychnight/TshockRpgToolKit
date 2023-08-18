using System;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Determines whether Mining and Placing target a tile, or wall.
	/// </summary>
	[Flags]
	public enum TileSubTarget : byte
	{
		//None = 0x00,
		Tile = 0x01,
		Wall = 0x02,
	}
}
