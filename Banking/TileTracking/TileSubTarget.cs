using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.TileTracking
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
