using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace Corruption.PluginSupport
{
	/// <summary>
	/// Provides fast access to Tile and Wall Id's from name strings. 
	/// </summary>
	public class TileNameMap
	{
		Dictionary<string, ushort> tileIds;
		Dictionary<string, byte> wallIds;

		static TileNameMap instance;
		public static TileNameMap Instance
		{
			get { return instance ?? ( instance = new TileNameMap()); }
			//set { instance = value; }
		}

		public TileNameMap()
		{
			tileIds = getNameMap<ushort>(typeof(TileID));
			wallIds = getNameMap<byte>(typeof(WallID));
		}

		private Dictionary<string, TValue> getNameMap<TValue>(Type type)
		{
			var dict = new Dictionary<string, TValue>();

			foreach(var fi in getConstants(type))
			{
				var key = fi.Name.ToLower();
				var value = (TValue)fi.GetValue(null);

				dict.Add(key, value);
			}

			return dict;
		}

		private FieldInfo[] getConstants(Type type)
		{
			var bindingFlags = BindingFlags.Static | BindingFlags.Public;
			var fields = type.GetFields(bindingFlags);

			return fields;
		}

		private string getKeyString(string name)
		{
			return name.ToLower().Replace(" ", "");
		}
						
		public bool TryGetTileId(string tileName, out ushort tileId)
		{
			var key = getKeyString(tileName);
			return tileIds.TryGetValue(key, out tileId);
		}

		public bool TryGetWallId(string wallName, out byte wallId)
		{
			var key = getKeyString(wallName);
			return wallIds.TryGetValue(key, out wallId);
		}

		//public bool TryGetTileName(ushort id, out string tileName)
		//{
		//}
		
		//public bool TryGetWallName(byte wallId, out string wallName )
		//{
		//}
	}
}
