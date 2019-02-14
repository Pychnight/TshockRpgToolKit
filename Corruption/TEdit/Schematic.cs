using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.TEdit
{
	public partial class Schematic
	{
		public string Name { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public Tile[,] Tiles { get; set; } // = new Tile[8,8];
		public List<Chest> Chests { get; set; } = new List<Chest>();
		public List<Sign> Signs { get; set; } = new List<Sign>();
		public bool IsGrabbed { get; set; }
		public int GrabbedX { get; set; }
		public int GrabbedY { get; set; }

		public Schematic(int width, int height, string name = "schematic")
		{
			Name = name;
			Width = width;
			Height = height;
			Tiles = new Tile[width, height];
		}
		
		public static Schematic Load(string fileName)
		{
			//string ext = Path.GetExtension(filename);
			//if( string.Equals(ext, ".jpg", StringComparison.InvariantCultureIgnoreCase) || string.Equals(ext, ".png", StringComparison.InvariantCultureIgnoreCase) || string.Equals(ext, ".bmp", StringComparison.InvariantCultureIgnoreCase) )
			//	return LoadFromImage(filename);

			try
			{
				using( var stream = new FileStream(fileName, FileMode.Open) )
				using( var b = new BinaryReader(stream) )
				{
					string name = b.ReadString();
					int version = b.ReadInt32();

					// check all the old versions
					if( version < 78 )
					{
						//return Load5(b, name, tVersion, version);
						var sch = new SchematicReaderV5().Read(b, name, version);
						return sch;
					}
					//else if( version == 4 )
					//{
					//	b.Close();
					//	stream.Close();
					//	return Load4(filename);
					//}
					//else if( version == 3 )
					//{
					//	b.Close();
					//	stream.Close();
					//	return Load3(filename);
					//}
					//else if( version == 2 )
					//{
					//	b.Close();
					//	stream.Close();
					//	return Load2(filename);
					//}
					//else if( version < 2 )
					//{
					//	b.Close();
					//	stream.Close();
					//	return LoadOld(filename);
					//}
					else
					{
						// not and old version, use new version
						//return LoadV2(b, name, tVersion, version);
						var sch = new SchematicReaderV2().Read(b, name, version); 
						return sch;
					}
				}
			}
			catch(Exception ex)
			{
				Debug.Print(ex.ToString());
				
				return null;
			}
		}
	}
}
