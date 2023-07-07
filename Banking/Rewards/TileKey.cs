using Banking.TileTracking;
using Newtonsoft.Json;
using System;

namespace Banking.Rewards
{
	/// <summary>
	/// Tile based key for ValueOverride.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class TileKey : IEquatable<TileKey>
	{
		//[JsonProperty]
		//public string NameOrType { get; set; } = "";

		/// <summary>
		/// Terraria Tile Id.
		/// </summary>
		[JsonProperty(Order = 0)]
		public ushort Type { get; set; } = 0;

		//[JsonProperty]
		//public int FrameX { get; set; } = -1;

		//[JsonProperty]
		//public int FrameY { get; set; } = -1;

		/// <summary>
		/// Terraria Wall Id.
		/// </summary>
		[JsonProperty(Order = 1)]
		public byte Wall { get; set; } = 0;

		public TileKey()
		{
		}

		public TileKey(Terraria.ITile tile)
		{
			Type = tile.type;
			//FrameX = tile.frameX;
			//FrameY = tile.frameY;
			Wall = tile.wall;
		}

		public TileKey(ushort tileOrWallId, TileSubTarget tileSubTarget)
		{
			switch (tileSubTarget)
			{
				case TileSubTarget.Tile:
					Type = tileOrWallId;
					break;
				case TileSubTarget.Wall:
					Wall = (byte)tileOrWallId;
					break;
			}
		}

		public override int GetHashCode() =>
			//return TileId.GetHashCode() ^ FrameX ^ FrameY ^ WallId;
			//return Type ^ FrameX ^ FrameY ^ Wall;
			Type ^ Wall;

		public bool Equals(TileKey other) => Type == other.Type && Wall == other.Wall;

		public override bool Equals(object obj)
		{
			var tk = obj as TileKey;
			if (tk != null)
				return Equals(tk);
			else
				return false;
		}
	}
}
