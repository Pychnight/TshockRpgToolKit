using OTAPI.Tile;
using System;
using TShockAPI;

namespace Banking
{
	public class TileChangedEventArgs : EventArgs
	{
		public TSPlayer Player { get; private set; }
		public int TileX { get; private set; }
		public int TileY { get; private set; }
		public int Type { get; private set; }

		public TileChangedEventArgs(TSPlayer player, int tileX, int tileY, int type)
		{
			Player = player;
			TileX = tileX;
			TileY = tileY;
			Type = type;
		}
	}
}