using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace Corruption
{
	public static class TileFunctions
	{
		public const int TileSize = 16;
		public const int HalfTileSize = TileSize / 2;

		public static bool DefaultTileFilterFunc(int column, int row)
		{
			var tile = Main.tile[column, row];
			var isActive = tile.active();
			//if( isActive &&
			//	( tile.type >= Tile.Type_Solid && tile.type <= Tile.Type_SlopeUpLeft || tile.wall != 0 ) ) // 0 - 5
			//{
			//	results.Add(new Point(col, row));
			//}

			if( !isActive )
				return false;

			var isEmpty = WorldGen.TileEmpty(column, row);

			if( !isEmpty || tile.wall != 0 || tile.liquid != 0 )
			{
				return true;

				//if(WorldGen.SolidOrSlopedTile(col,row))
				//{
				//	results.Add(new Point(col, row)); 
				//}
				//else
				//{

				//}
			}

			return false;
		}
		
		public static List<Point> GetOverlappedTiles(Rectangle bounds)
		{
			return GetOverlappedTiles(bounds,DefaultTileFilterFunc);
		}

		public static List<Point> GetOverlappedTiles(Rectangle bounds, Func<int, int, bool> filterFunc)
		{
			var min = bounds.TopLeft().ToTileCoordinates();
			var max = bounds.BottomRight().ToTileCoordinates();
			var tileCollisions = GetNonEmptyTiles(min.X, min.Y, max.X, max.Y, filterFunc);

			return tileCollisions;
		}

		public static List<Point> GetNonEmptyTiles(int minColumn, int minRow, int maxColumn, int maxRow)
		{
			return GetNonEmptyTiles(minColumn, minRow, maxColumn, maxRow, DefaultTileFilterFunc);
		}

		public static List<Point> GetNonEmptyTiles(int minColumn, int minRow, int maxColumn, int maxRow, Func<int, int, bool> filterFunc)
		{
			var results = new List<Point>();

			if( filterFunc == null )
				return results;

			//clip to tileset
			minColumn = Math.Max(minColumn, 0);
			minRow = Math.Max(minRow, 0);
			maxColumn = Math.Min(maxColumn, Main.tile.Width - 1);
			maxRow = Math.Min(maxRow, Main.tile.Height - 1);

			for( var row = minRow; row <= maxRow; row++ )
			{
				for( var col = minColumn; col <= maxColumn; col++ )
				{
					if(filterFunc(col,row))
					{
						results.Add(new Point(col, row));
					}
				}
			}

			return results;//.AsReadOnly();
		}
		
		public static bool InTileMapBounds(int column, int row)
		{
			if( column < 0 || column > Main.maxTilesX )
				return false;

			if( row < 0 || row > Main.maxTilesY )
				return false;

			return true;
		}
				
		public static int TileX(float worldX)
		{
			return (int)( worldX / TileSize );
		}
				
		public static int TileY(float worldY)
		{
			return (int)( worldY / TileSize );
		}

		public static int WorldX(int tileX)
		{
			return tileX * TileSize;
		}

		public static int WorldY(int tileY)
		{
			return tileY * TileSize;
		}

		/// <summary>
		///     Gets the tile at the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate, which must be in bounds.</param>
		/// <param name="y">The Y coordinate, which must be in bounds.</param>
		/// <returns>The tile.</returns>
		public static ITile GetTile(int x, int y) => Main.tile[x, y];

		//
		//public static bool SolidTile(ITile tile)
		//{
		//	return tile.active() && tile.type < Main.maxTileSets && Main.tileSolid[tile.type];
		//}
				
		public static bool IsSolidTile(int column, int row)
		{
			return WorldGen.SolidTile(column, row);
		}
				
		public static bool IsSolidOrSlopedTile(int column, int row)
		{
			return WorldGen.SolidOrSlopedTile(column, row);
		}
				
		public static bool IsWallTile(int column, int row)
		{
			var tile = GetTile(column, row);
			return tile.wall > 0;
		}

		
		public static bool IsLiquidTile(int column, int row)
		{
			var tile = GetTile(column, row);
			return tile.liquid > 0;
		}

		//public static bool IsWaterTile(int column, int row)
		//{
		//	var tile = GetTile(column, row);
		//	return tile.liquid > 0 && tile.liquidType() == 0;
		//}

		//public static bool IsTileLiquid(ITile tile)
		//{
		//	return tile.active() && tile.liquid 
		//}
				
		public static void SetTile(int column, int row, int type)
		{
			try
			{
				if( Main.tile[column, row]?.active() == true )
				{
					Main.tile[column, row].ResetToType((ushort)type);
					TSPlayer.All.SendTileSquare(column, row);
				}
			}
			catch(IndexOutOfRangeException rex)
			{
				//CustomNpcsPlugin.Instance.LogPrint($"Tried to SetTile on an invalid index.", TraceLevel.Error);
			}
		}
				
		public static void KillTile(int column, int row)
		{
			try
			{
				if( Main.tile[column, row]?.active() == true )
				{
					WorldGen.KillTile(column, row);
					TSPlayer.All.SendTileSquare(column, row);
				}
			}
			catch(IndexOutOfRangeException rex)
			{
				//CustomNpcsPlugin.Instance.LogPrint($"Tried to KillTile on an invalid index.", TraceLevel.Error);
			}
		}
				
		public static void RadialKillTile(int x, int y, int radius)
		{
			var box = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
			var hits = GetOverlappedTiles(box);
			var tileCenterOffset = new Vector2(HalfTileSize, HalfTileSize);
			var center = new Vector2(x, y);

			foreach(var hit in hits)
			{
				var tile = Main.tile[hit.X, hit.Y];

				//ignore walls and trees and such.
				if( tile.collisionType < 1 )
					continue;

				var tileCenter = new Vector2(hit.X * TileSize,hit.Y * TileSize);
				tileCenter += tileCenterOffset;

				var dist = tileCenter - center;
								
				if( dist.LengthSquared() <= (radius * radius))
				{
					KillTile(hit.X, hit.Y);
				}
			}
		}

		//
		//public static void RadialKillTile(Vector2 position, int radius)
		//{
		//	RadialKillTile((int)position.X, (int)position.Y, radius);
		//}
				
		public static void RadialSetTile(int x, int y, int radius, int type)
		{
			var box = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
			var hits = GetOverlappedTiles(box);
			var tileCenterOffset = new Vector2(HalfTileSize, HalfTileSize);
			var center = new Vector2(x, y);

			foreach( var hit in hits )
			{
				var tile = Main.tile[hit.X, hit.Y];

				//ignore walls and trees and such.
				if( tile.collisionType < 1 )
					continue;

				var tileCenter = new Vector2(hit.X * TileSize, hit.Y * TileSize);
				tileCenter += tileCenterOffset;

				var dist = tileCenter - center;

				if( dist.LengthSquared() <= ( radius * radius ) )
				{
					SetTile(hit.X, hit.Y, type);
				}
			}
		}
	}
}
