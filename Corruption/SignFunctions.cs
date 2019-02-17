using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace Corruption
{
	public static class SignFunctions
	{
		/// <summary>
		/// Creates a Sign into the specified slot and calls SendTileSquare(), but does not alter any tiles itself.
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="text"></param>
		internal static void CreateSignDirect(int slot, int x, int y, string text )
		{
			//if( slot == -1 )
			//	return false;

			var sign = new Sign();
			sign.x = x;
			sign.y = y;
			sign.text = text;

			Main.sign[slot] = sign;
			TSPlayer.All.SendTileSquare(x, y);
			
			//return true;
		}

		/// <summary>
		/// Trys to find an empty sign slot, and if so calls CreateSignDirect() on it.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="text"></param>
		/// <returns>Bool result.</returns>
		internal static bool TryCreateSignDirect(int x, int y, string text)
		{
			var newSignSlot = FindEmptySignSlot();

			if( newSignSlot > -1 )
			{
				CreateSignDirect(newSignSlot, x, y, text);
				
				return true;
			}

			return false;
		}

		public static bool CreateSign(int x, int y, int type, string text)
		{
			var style = 0;

			var newSignSlot = FindEmptySignSlot();

			if( newSignSlot > -1 )
			{
				var result = WorldGen.PlaceSign(x, y + 1, (ushort)type, style);

				if( result )
				{
					CreateSignDirect(newSignSlot, x, y, text);
				}

				return result;
			}

			return false;
		}

		public static bool CreateSign(int x, int y, int type)
		{
			return CreateSign(x, y, type, "");
		}

		public static bool CreateSign(int x, int y, SignTypes type)
		{
			return CreateSign(x, y, (int)type);
		}

		public static bool CreateSign(int x, int y)
		{
			return CreateSign(x, y, (int)SignTypes.Sign);
		}

		public static bool KillSign(int x, int y)
		{
			return KillSign(x, y, effectOnly: false, noItem: false);
		}

		public static bool KillSign(int x, int y, bool effectOnly, bool noItem)
		{
			//Terrarias built in kill sign func doesnt return status... lets roll our own version for debugging's sake.
			//Sign.KillSign(x, y);

			var result = false;

			for( int i = 0; i < Main.sign.Length; i++ )
			{
				var sign = Main.sign[i];

				if( sign != null && sign.x == x && sign.y == y )
				{
					Main.sign[i] = null;
					result = true;
					break;//terraria's version loops through ALL slots, for some unknown reason. We'll just break on first success.
				}
			}

			if(result)
			{
				//TileFunctions.KillTile(x, y);
				WorldGen.KillTile(x, y, false, effectOnly, noItem);//effectOnly = we don't want items to be produced from this. 
				TSPlayer.All.SendTileSquare(x, y, 3);
			}

			return result;
		}

		public static void SetSignText(int x, int y, string txt)
		{
			var id = FindSignId(x, y);

			if( id > -1 )
			{
				Sign.TextSign(id, txt);

				TSPlayer.All.SendTileSquare(x, y);
			}
		}

		//Terraria... lover of stupid linear searches!

		internal static int FindSignId(int x, int y)
		{
			for( int i = 0; i < Main.sign.Length; i++ )
			{
				if( Main.sign[i] != null && Main.sign[i].x == x && Main.sign[i].y == y )
				{
					return i;
				}
			}

			return -1;
		}

		internal static int FindEmptySignSlot()
		{
			for( int i = 0; i < Main.sign.Length; i++ )
			{
				if( Main.sign[i] == null )
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Finds all signs within the specified tile bounds.
		/// </summary>
		/// <param name="xMin"></param>
		/// <param name="yMin"></param>
		/// <param name="xMax"></param>
		/// <param name="yMax"></param>
		/// <returns>List of chest id's.</returns>
		internal static List<int> FindSigns(int xMin, int yMin, int xMax, int yMax)
		{
			var results = new List<int>();

			for( var i = 0; i < Main.sign.Length; i++ )
			{
				var sign = Main.sign[i];

				if( sign != null )
				{
					if( sign.x >= xMin && sign.x <= xMax )
					{
						if( sign.y >= yMin && sign.y <= yMax )
						{
							results.Add(i);
						}
					}
				}
			}

			return results;
		}

		/// <summary>
		/// Attempts to kill all signs within the specified tile bounds.
		/// </summary>
		/// <param name="xMin"></param>
		/// <param name="yMin"></param>
		/// <param name="xMax"></param>
		/// <param name="yMax"></param>
		/// <param name="effectOnly"></param>
		/// <returns>List of results, where all values > -1 are id's that were destroyed.</returns>
		internal static List<int> ClearSigns(int xMin, int yMin, int xMax, int yMax, bool effectOnly = true)
		{
			var results = FindSigns(xMin, yMin, xMax, yMax);

			for( var i = 0; i < results.Count; i++ )
			{
				var signId = results[i];
				var sign = Main.sign[signId];

				if( sign != null )
					results[i] = KillSign(sign.x, sign.y, effectOnly, noItem: true) ? results[i] : -1;
				else
					results[i] = -1;//we couldn't remove this sign, mark its id as failure/invalid.

				//Debug.Print($"ClearSigns: result[{i}] = {results[i]}");
			}
			
			return results;
		}
	}
}
