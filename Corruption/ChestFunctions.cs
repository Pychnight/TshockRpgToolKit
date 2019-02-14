using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;

namespace Corruption
{
	public static class ChestFunctions
	{
		/// <summary>
		///     Places a chest at the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="style">The style.</param>
		/// <param name="groundTile">Set to a tile type that will support the chest, or -1 to leave existing tile.</param>
		/// <returns>Id of new chest. If a chest could not be created, -1.</returns>
		public static int CreateChest(int x, int y, int style, int groundTile = -1)
		{
			var chestId = Chest.FindChest(x, y - 1);
			if( chestId != -1 )
			{
				//chest already exists here.
				return -1;
			}

			chestId = WorldGen.PlaceChest(x, y, style: style);
			if( chestId != -1 )
			{
				TSPlayer.All.SendData((PacketTypes)34, "", 0, x, y, style, chestId);
				return chestId;
			}

			//...add supporting tile/ground?
			for( var i = x; i < x + 2; ++i )
			{
				for( var j = y - 1; j < y + 2; ++j )
				{
					var tile = Main.tile[i, j];
					if( j == y + 1 )
					{
						tile.active(true);

						if( groundTile > -1 )
							tile.type = (ushort)groundTile;
					}
					else
					{
						tile.active(false);
					}
				}
			}
			TSPlayer.All.SendTileSquare(x, y, 3);

			chestId = WorldGen.PlaceChest(x, y, style: style);
			if( chestId != -1 )
			{
				TSPlayer.All.SendData((PacketTypes)34, "", 0, x, y, style, chestId);

				//var fx = Main.tile[x, y].frameX;
				//Debug.Print($"FrameX: {fx}");
			}

			return chestId;
		}

		/// <summary>
		///     Backwards compatible function for placing a chest at the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="style">The style.</param>
		/// <remarks>New code should use CreateChest() instead, which returns the chest id, or -1 on failure.</remarks>
		public static void PlaceChest(int x, int y, int style)
		{
			CreateChest(x, y, style, 0);// 0 = support with dirt
		}

		/// <summary>
		/// Destroys a Chest at the specified coordinates, if one exists. 
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <returns>Returns true on success, false otherwise.</returns>
		internal static bool DestroyChest(int x, int y)
		{
			var chestId = Chest.FindChest(x, y - 1);
			if( chestId == -1 )
			{
				return false;
			}

			Chest.DestroyChestDirect(x, y - 1, chestId);
			//use our lifted version instead of terraria's, so we can step through in debugger
			//DestroyChestDirect(x, y - 1, chestId);

			//do we need to send update? probably yes
			var packet = (PacketTypes)34;//bug in tshock or terraia? 32 should be place chest, not kill tile.
			var style = 0; //unused??

			TSPlayer.All.SendData(packet, "", 1, x, y - 1, style, chestId);

			return true;
		}

		///// <summary>
		///// Find
		///// </summary>
		///// <param name="x"></param>
		///// <param name="y"></param>
		///// <returns></returns>
		//public static int FindChest( int x, int y )
		//{
		//	return Chest.FindChest(x, y);
		//}

		/// <summary>
		///     Puts an item into the chest at the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="stack">The stack.</param>
		/// <param name="prefix">The prefix.</param>
		public static void PutItemIntoChest(int x, int y, int type, int stack = 1, byte prefix = 0)
		{
			var chestId = Chest.FindChest(x, y - 1);
			if( chestId == -1 )
			{
				return;
			}

			PutItemIntoChest(chestId, type, stack, prefix);
		}

		/// <summary>
		/// Fast version overload of PutItemIntoChest for Schematics. Avoids searching for a chest.
		/// </summary>
		/// <param name="chestId"></param>
		/// <param name="type"></param>
		/// <param name="stack"></param>
		/// <param name="prefix"></param>
		/// <remarks>This should probably be made public, and sibling methods made to operate on chest id's as well. But
		/// for time reasons this will have to do for now.</remarks>
		internal static void PutItemIntoChest(int chestId, int type, int stack = 1, byte prefix = 0)
		{
			var chest = Main.chest[chestId];
			for( var i = 0; i < Chest.maxItems; ++i )
			{
				var item = chest.item[i];
				if( item.netID == 0 )
				{
					item.netID = type;
					item.stack = stack;
					item.prefix = prefix;
					TSPlayer.All.SendData(PacketTypes.ChestItem, "", chestId, i);
					return;
				}
			}
		}

		/// <summary>
		///     Puts an item into the chest at the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="itemType">The item type.</param>
		/// <param name="stack">The stack.</param>
		/// <param name="prefix">The prefix.</param>
		public static void PutItemIntoChest(int x, int y, string itemType, int stack = 1, byte prefix = 0)
		{
			var id = ItemFunctions.GetItemIdFromName(itemType);

			if( id == null )
			{
				//CustomQuestsPlugin.Instance.LogPrint($"Can't put item in chest. No id found for '{itemType}'.", TraceLevel.Error);
				return;
			}

			PutItemIntoChest(x, y, (int)id, stack, prefix);
		}

		/// <summary>
		///		Counts the quantiy of the specified Item that are in the Chest.
		/// </summary>
		/// <param name="x">Chest X location in tile coords.</param>
		/// <param name="y">Chest Y location in tile coords.</param>
		/// <param name="itemId">Item id.</param>
		/// <param name="prefix">Item prefix.</param>
		/// <returns>Total Item count.</returns>
		public static int CountChestItem(int x, int y, int itemId, byte prefix = 0)
		{
			var chestId = Chest.FindChest(x, y - 1);
			if( chestId == -1 )
			{
				return 0;
			}

			var count = 0;
			var chest = Main.chest[chestId];
			for( var i = 0; i < Chest.maxItems; i++ )
			{
				var item = chest.item[i];
				if( item.netID == itemId && item.prefix == prefix )
				{
					//Debug.Print($"Stack: {item.stack}");
					count += item.stack;
				}
			}

			return count;
		}

		/// <summary>
		///		Counts the quantiy of the specified Item that are in the Chest.
		/// </summary>
		/// <param name="x">Chest X location in tile coords.</param>
		/// <param name="y">Chest Y location in tile coords.</param>
		/// <param name="itemType">Item name.</param>
		/// <param name="prefix">Item prefix.</param>
		/// <returns>Total Item count.</returns>
		public static int CountChestItem(int x, int y, string itemType, byte prefix = 0)
		{
			var id = ItemFunctions.GetItemIdFromName(itemType);

			if( id == null )
				return 0;

			return CountChestItem(x, y, (int)id, prefix);
		}

		/// <summary>
		/// Finds all chests within the specified tile bounds.
		/// </summary>
		/// <param name="xMin"></param>
		/// <param name="yMin"></param>
		/// <param name="xMax"></param>
		/// <param name="yMax"></param>
		/// <returns>List of chest id's.</returns>
		internal static List<int> FindChests(int xMin, int yMin, int xMax, int yMax)
		{
			var results = new List<int>();

			for( var i = 0; i < Main.chest.Length; i++ )
			{
				var chest = Main.chest[i];

				if( chest != null )
				{
					if( chest.x >= xMin && chest.x <= xMax )
					{
						if( chest.y >= yMin && chest.y <= yMax )
						{
							results.Add(i);
						}
					}
				}
			}

			return results;
		}

		/// <summary>
		/// Attempts to destroy all chests within the specified tile bounds.
		/// </summary>
		/// <param name="xMin"></param>
		/// <param name="yMin"></param>
		/// <param name="xMax"></param>
		/// <param name="yMax"></param>
		/// <returns>List of results, where all values > -1 are id's that were destroyed.</returns>
		internal static List<int> ClearChests(int xMin, int yMin, int xMax, int yMax)
		{
			var results = FindChests(xMin, yMin, xMax, yMax);

			for( var i = 0; i < results.Count; i++ )
			{
				var chestId = results[i];
				var chest = Main.chest[chestId];

				if( chest != null ) // we have to offset y + 1... because this whole thing is madness. Thats why >_<
					results[i] = DestroyChest(chest.x, chest.y + 1) ? results[i] : -1;
				else
					results[i] = -1;//we never removed this chest, mark its id as failure/invalid.

				Debug.Print($"ClearChests: result[{i}] = {results[i]}");
			}

			return results;
		}

		// Rebuilt from decompiled terraria code so we can  easily debug, and see whats happening.
		//public static void DestroyChestDirect(int X, int Y, int id)
		//{
		//	if( id < 0 || id >= Main.chest.Length )
		//	{
		//		return;
		//	}
		//	try
		//	{
		//		Chest chest = Main.chest[id];
		//		if( chest != null )
		//		{
		//			if( chest.x == X && chest.y == Y )
		//			{
		//				Main.chest[id] = null;
		//				if( Main.player[Main.myPlayer].chest == id )
		//				{
		//					Main.player[Main.myPlayer].chest = -1;
		//				}
		//				Recipe.FindRecipes();
		//			}
		//		}
		//	}
		//	catch
		//	{
		//	}
		//}
	}
}
