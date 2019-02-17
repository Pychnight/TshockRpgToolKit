using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.TEdit
{
	public class SchematicReaderV5 : SchematicReader
	{
		public override Schematic Read(BinaryReader b, string name, int version)
		{
			var tVersion = (uint)version;
			var sizeX = b.ReadInt32();
			var sizeY = b.ReadInt32();
			var schematic = new Schematic(sizeX, sizeY, name);

			for( int x = 0; x < sizeX; ++x )
			{
				for( int y = 0; y < sizeY; y++ )
				{
					var tile = World.ReadTileDataFromStreamV1(b, tVersion);
					// read complete, start compression
					schematic.Tiles[x, y] = tile;

					int rle = b.ReadInt16();
					if( rle < 0 )
						throw new ApplicationException("Invalid Tile Data!");

					if( rle > 0 )
					{
						for( int k = y + 1; k < y + rle + 1; k++ )
						{
							var tcopy = (Tile)tile.Clone();
							schematic.Tiles[x, k] = tcopy;
						}
						y = y + rle;
					}
				}
			}

			//throw new NotImplementedException("Chests disabled.");
			//schematic.Chests.Clear();
			//schematic.Chests.AddRange(World.ReadChestDataFromStreamV1(b, tVersion));

			//throw new NotImplementedException("Signs disabled.");
			//schematic.Signs.Clear();
			//foreach( Sign sign in World.ReadSignDataFromStreamV1(b) )
			//{
			//	if( schematic.Tiles[sign.X, sign.Y].IsActive && Tile.IsSign(schematic.Tiles[sign.X, sign.Y].Type) )
			//		schematic.Signs.Add(sign);
			//}

			string verifyName = b.ReadString();
			int verifyVersion = b.ReadInt32();
			int verifyX = b.ReadInt32();
			int verifyY = b.ReadInt32();
			if( schematic.Name == verifyName &&
				version == verifyVersion &&
				schematic.Width == verifyX &&
				schematic.Height == verifyY )
			{
				// valid;
				return schematic;
			}
			b.Close();
			return null;
		}
	}
}
