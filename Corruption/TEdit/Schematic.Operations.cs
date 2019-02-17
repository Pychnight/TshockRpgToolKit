#define SCHEMATIC_ENABLE_SIGNS //toggles signs in paste/grab operations ( sign cleanup is not working as expected currently...prob not sending right packets )

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace Corruption.TEdit
{
	public partial class Schematic
	{
		/// <summary>
		/// Pastes a Schematic into the world.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Paste(int x, int y)
		{
			//clip
			var worldRect = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
			var schematicRect = new Rectangle(x, y, Width, Height);
			var clippedRect = schematicRect;

			worldRect.Intersects(ref clippedRect, out var intersects);

			if( !intersects )
				return;

			//starting position within schematic to read from.
			var readColumnStart = clippedRect.Left - x;
			var readRowStart = clippedRect.Top - y;

			//clear existing chests
			ChestFunctions.ClearChests(clippedRect.Left, clippedRect.Top, clippedRect.Right, clippedRect.Bottom);

#if SCHEMATIC_ENABLE_SIGNS
			//clear signs
			SignFunctions.ClearSigns(clippedRect.Left, clippedRect.Top, clippedRect.Right, clippedRect.Bottom, effectOnly: true);
#endif

			//copy tiles
			var readRow = readRowStart;

			for( var row = clippedRect.Top; row < clippedRect.Bottom; row++ )
			{
				var readColumn = readColumnStart;

				for( var column = clippedRect.Left; column < clippedRect.Right; column++ )
				{
					var readTile = Tiles[readColumn, readRow];
					Main.tile[column, row].CopyFrom(readTile);
					//TSPlayer.All.SendTileSquare(column, row, 1);
										
					readColumn++;
				}

				readRow++;
			}

			//try to send update, using the pasted schematics center point, and radius
			SendTileUpdates(TSPlayer.All, ref clippedRect);

			//paste chests
			foreach( var chest in Chests )
			{
				const int frameSize = 16 + 2; //presumably, this is the frame size.

				var tile = Tiles[chest.X,chest.Y];
				var chestStyle = tile.U / frameSize;
				var chestX = x + chest.X;
				var chestY = y + chest.Y + 1; // we have to add 1, because create chest uses an offset.
				var chestId = ChestFunctions.CreateChest(chestX, chestY, chestStyle);
				
				if( chestId != -1 )
				{
					foreach(var item in chest.Items)
					{
						ChestFunctions.PutItemIntoChest(chestId, item.NetId, item.StackSize, item.Prefix);
					}
				}
				else
				{
					//failed to create chest for some reason, maybe one already in the way.
					Debug.Print($"Failed to create schematic chest at {chestX}, {chestY}.");
				}
								
				//This is a bug in terraria server? or tshock?
				//PacketType 34 should be PlaceChest, not TileKill. Sigh.
				//TSPlayer.All.SendData(PacketTypes.TileKill, "", chestId, tileX, tileY, style, 0);//0 = ChestID to destroy, but we dont use this here..
			}


#if SCHEMATIC_ENABLE_SIGNS
			//paste signs
			foreach(var sign in Signs)
			{
				var signX = x + sign.X;
				var signY = y + sign.Y;

				var result = SignFunctions.TryCreateSignDirect(signX, signY, sign.Text);

				if( result )
					Debug.Print($"Pasted sign at {signX}, {signY}.");
				else
					Debug.Print($"Failed to paste sign at {signX}, {signY}.");
			}
#endif

		}
		
		/// <summary>
		/// Creates a Schematic, from existing tiles in the world. 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="columns"></param>
		/// <param name="rows"></param>
		/// <returns></returns>
		public static Schematic Grab(int x, int y, int columns, int rows)
		{
			//clip
			var worldRect = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
			var schematicRect = new Rectangle(x, y, columns, rows);
			var clippedRect = schematicRect;

			worldRect.Intersects(ref clippedRect, out var intersects);

			var result = new Schematic(clippedRect.Width,clippedRect.Height);

			result.IsGrabbed = true;
			result.GrabbedX = clippedRect.Left;
			result.GrabbedY = clippedRect.Top;

			if( !intersects )
				return result;

			//starting position within schematic to read from.
			var writeColumnStart = clippedRect.Left - x;
			var writeRowStart = clippedRect.Top - y;
			
			var writeRow = writeRowStart;

			for( var row = clippedRect.Top; row < clippedRect.Bottom; row++ )
			{
				var writeColumn = writeColumnStart;

				for( var column = clippedRect.Left; column < clippedRect.Right; column++ )
				{
					//var readTile = Tiles[readColumn, readRow];
					var itile = Main.tile[column, row];
					var tile = new Tile();

					tile.CopyFrom(itile);
					result.Tiles[writeColumn, writeRow] = tile;

					writeColumn++;
				}

				writeRow++;
			}

			//grab chests
			var chestIds = ChestFunctions.FindChests(clippedRect.Left, clippedRect.Top, clippedRect.Right, clippedRect.Bottom);
			foreach(var id in chestIds)
			{
				var srcChest = Main.chest[id];

				if(srcChest!=null)
				{
					Corruption.TEdit.Chest dst = new Corruption.TEdit.Chest(srcChest);

					//we have to offset dst into coords that are relative to the schematic, not the world tileset.
					dst.X = dst.X - x;
					dst.Y = dst.Y - y;

					result.Chests.Add(dst);
				}
			}


#if SCHEMATIC_ENABLE_SIGNS
			//grab signs

			var signIds = SignFunctions.FindSigns(clippedRect.Left, clippedRect.Top, clippedRect.Right, clippedRect.Bottom);
			foreach(var id in signIds)
			{
				var srcSign = Main.sign[id];

				if(srcSign!=null)
				{
					Corruption.TEdit.Sign dst = new Corruption.TEdit.Sign();

					//we have to offset dst into coords that are relative to the schematic, not the world tileset.
					dst.X = srcSign.x - x;
					dst.Y = srcSign.y - y;
					dst.Text = srcSign.text;

					result.Signs.Add(dst);
				}
			}
#endif
			return result;
		}

		/// <summary>
		/// Used to replace modified tiles after Paste operations.
		/// </summary>
		public void Restore()
		{
			if( IsGrabbed )
			{
				Paste(GrabbedX, GrabbedY);
			}
		}
		
		/// <summary>
		/// Shortcut that combines Grab() and Paste() into a single method.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Schematic GrabPaste(int x, int y)
		{
			var grabbed = Grab(x, y, Width, Height);

			Paste(x, y);

			return grabbed;
		}

		//should we expose this publicly?
		internal static void SendTileUpdates(TSPlayer player, ref Rectangle rectangle )
		{
			//try to send update, using the pasted schematics center point, and radius
			var updateX = rectangle.Center.X;
			var updateY = rectangle.Center.Y;
			var updateSize = rectangle.Width > rectangle.Height ? rectangle.Width : rectangle.Height;

			TSPlayer.All.SendTileSquare(updateX, updateY, updateSize);
			//TSPlayer.All.SendData(PacketTypes.TileSendSquare, "", updateSize, updateX, updateY, 0, 0);

			//Debug.Print("Sending tile section...");
			//Debug.Print($"{rectangle.X}, {rectangle.Y}, {rectangle.Width}, {rectangle.Height}");
			
			//TSPlayer.All.SendData(PacketTypes.TileSendSection, "", rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

			//TSPlayer.All.SendTileSquare(updateX, updateY, updateSize);
		}
	}
}
