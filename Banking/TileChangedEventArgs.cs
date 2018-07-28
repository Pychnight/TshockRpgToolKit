using Banking.TileTracking;
using OTAPI.Tile;
using System;
using TShockAPI;

namespace Banking
{
	/// <summary>
	/// Contains information on a tile change event.
	/// </summary>
	public class TileChangedEventArgs : EventArgs
	{
		public TSPlayer Player { get; private set; }
		public int TileX { get; private set; }
		public int TileY { get; private set; }
		public ushort Type { get; private set; }
		public byte Wall { get; private set; }
		public TileSubTarget TileSubTarget { get; private set; }

		public TileChangedEventArgs(TSPlayer player, int tileX, int tileY, ushort type, byte wall, TileSubTarget tileSubTarget)
		{
			Player = player;
			TileX = tileX;
			TileY = tileY;
			Type = type;
			Wall = wall;
			TileSubTarget = tileSubTarget;
		}

		/// <summary>
		/// Helper method returns either Type or Wall, depending on TileSubTarget.
		/// </summary>
		/// <returns></returns>
		public ushort GetTypeOrWall()
		{
			if( TileSubTarget == TileSubTarget.Tile )
				return Type;
			else
				return Wall;
		}
	}
}