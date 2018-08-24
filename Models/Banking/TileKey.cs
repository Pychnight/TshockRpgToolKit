﻿//using Banking.TileTracking;
using Newtonsoft.Json;
//using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Tile based key for ValueOverride.
	/// </summary>
	[TypeConverter(typeof(TileKeyConverter))]
	[JsonObject(MemberSerialization.OptIn)]
	public class TileKey : IEquatable<TileKey>
	{
		//[JsonProperty]
		//public string NameOrType { get; set; } = "";

		/// <summary>
		/// Terraria Tile Id.
		/// </summary>
		[JsonProperty]
		public ushort Type { get; set; } = 0;

		//[JsonProperty]
		//public int FrameX { get; set; } = -1;

		//[JsonProperty]
		//public int FrameY { get; set; } = -1;

		/// <summary>
		/// Terraria Wall Id.
		/// </summary>
		[JsonProperty]
		public byte Wall { get; set; } = 0;

		public TileKey()
		{
		}

		//public TileKey(ITile tile)
		//{
		//	Type = tile.type;
		//	//FrameX = tile.frameX;
		//	//FrameY = tile.frameY;
		//	Wall = tile.wall;
		//}

		public TileKey(ushort tileOrWallId, TileSubTarget tileSubTarget)
		{
			switch( tileSubTarget )
			{
				case TileSubTarget.Tile:
					Type = tileOrWallId;
					break;
				case TileSubTarget.Wall:
					Wall = (byte)tileOrWallId;
					break;
			}
		}
		
		public override int GetHashCode()
		{
			//return TileId.GetHashCode() ^ FrameX ^ FrameY ^ WallId;
			//return Type ^ FrameX ^ FrameY ^ Wall;
			return Type ^ Wall;
		}

		public bool Equals(TileKey other)
		{
			return Type == other.Type && Wall == other.Wall;
		}

		public override bool Equals(object obj)
		{
			var tk = obj as TileKey;
			if( tk!=null )
				return Equals(tk);
			else
				return false;
		}

		public override string ToString()
		{
			return $"TileKey(Type:{Type}, Wall:{Wall})";
		}
	}
}
