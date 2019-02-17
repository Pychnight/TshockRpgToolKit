using System;
using System.IO;

namespace Corruption.TEdit
{
	public class SchematicReaderV2 : SchematicReader
	{
		public override Schematic Read(BinaryReader b, string name, int version)
		{
			var sizeX = b.ReadInt32();
			var sizeY = b.ReadInt32();
			var sch = new Schematic(sizeX, sizeY);

			sch.Name = name;
			sch.Tiles = World.LoadTileData(b, sizeX, sizeY);
			sch.Chests.AddRange(World.LoadChestData(b));
			sch.Signs.AddRange(World.LoadSignData(b));

			string verifyName = b.ReadString();
			int verifyVersion = b.ReadInt32();
			int verifyX = b.ReadInt32();
			int verifyY = b.ReadInt32();
			if( sch.Name == verifyName &&
				version == verifyVersion &&
				sch.Width == verifyX &&
				sch.Height == verifyY )
			{
				// valid;
				return sch;
			}
			b.Close();

			return null;
		}
	}
}