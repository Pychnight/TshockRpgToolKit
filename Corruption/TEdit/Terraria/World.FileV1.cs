using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.TEdit
{
	internal partial class World
	{
		public static Tile ReadTileDataFromStreamV1(BinaryReader b, uint version)
		{
			var tile = new Tile();

			tile.IsActive = b.ReadBoolean();

			TileProperty tileProperty = null;

			if( tile.IsActive )
			{
				tile.Type = b.ReadByte();
				tileProperty = TileProperties[tile.Type];


				if( tile.Type == (int)TileType.IceByRod )
					tile.IsActive = false;

				if( version < 72 &&
					( tile.Type == 35 || tile.Type == 36 || tile.Type == 170 || tile.Type == 171 || tile.Type == 172 ) )
				{
					tile.U = b.ReadInt16();
					tile.V = b.ReadInt16();
				}
				else if( !tileProperty.IsFramed )
				{
					tile.U = -1;
					tile.V = -1;
				}
				else if( version < 28 && tile.Type == (int)( TileType.Torch ) )
				{
					// torches didn't have extra in older versions.
					tile.U = 0;
					tile.V = 0;
				}
				else if( version < 40 && tile.Type == (int)TileType.Platform )
				{
					tile.U = 0;
					tile.V = 0;
				}
				else
				{
					tile.U = b.ReadInt16();
					tile.V = b.ReadInt16();

					if( tile.Type == (int)TileType.Timer )
						tile.V = 0;
				}


				if( version >= 48 && b.ReadBoolean() )
				{
					tile.TileColor = b.ReadByte();
				}
			}

			//skip obsolete hasLight
			if( version <= 25 )
				b.ReadBoolean();

			if( b.ReadBoolean() )
			{
				tile.Wall = b.ReadByte();
				if( version >= 48 && b.ReadBoolean() )
					tile.WallColor = b.ReadByte();
			}

			if( b.ReadBoolean() )
			{
				tile.LiquidType = LiquidType.Water;
				tile.LiquidAmount = b.ReadByte();
				if( b.ReadBoolean() ) tile.LiquidType = LiquidType.Lava;
				if( version >= 51 )
				{
					if( b.ReadBoolean() ) tile.LiquidType = LiquidType.Honey;
				}
			}

			if( version >= 33 )
			{
				tile.WireRed = b.ReadBoolean();
			}
			if( version >= 43 )
			{
				tile.WireGreen = b.ReadBoolean();
				tile.WireBlue = b.ReadBoolean();
			}

			if( version >= 41 )
			{
				bool isHalfBrick = b.ReadBoolean();

				if( tileProperty == null || !tileProperty.IsSolid )
					isHalfBrick = false;

				if( version >= 49 )
				{
					tile.BrickStyle = (BrickStyle)b.ReadByte();

					if( tileProperty == null || !tileProperty.IsSolid )
						tile.BrickStyle = 0;
				}
			}
			if( version >= 42 )
			{
				tile.Actuator = b.ReadBoolean();
				tile.InActive = b.ReadBoolean();
			}
			return tile;
		}

	}
}
