using Microsoft.Xna.Framework;
using NLua;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace CustomNpcs
{
	public static class TileFunctions
	{
		const int tileSize = 16;

		public static ReadOnlyCollection<Point> GetOverlappedTiles(Rectangle bounds)
		{
			var min = bounds.TopLeft().ToTileCoordinates();
			var max = bounds.BottomRight().ToTileCoordinates();
			var tileCollisions = GetNonEmptyTiles(min.X, min.Y, max.X, max.Y);

			return tileCollisions;
		}

		public static ReadOnlyCollection<Point> GetNonEmptyTiles(int minColumn, int minRow, int maxColumn, int maxRow)
		{
			var results = new List<Point>();

			//clip to tileset
			minColumn = Math.Max(minColumn, 0);
			minRow = Math.Max(minRow, 0);
			maxColumn = Math.Min(maxColumn, Main.tile.Width - 1);
			maxRow = Math.Min(maxRow, Main.tile.Height - 1);

			for( var row = minRow; row <= maxRow; row++ )
			{
				for( var col = minColumn; col <= maxColumn; col++ )
				{
					var tile = Main.tile[col, row];
					var isActive = tile.active();
					//if( isActive &&
					//	( tile.type >= Tile.Type_Solid && tile.type <= Tile.Type_SlopeUpLeft || tile.wall != 0 ) ) // 0 - 5
					//{
					//	results.Add(new Point(col, row));
					//}

					var isEmpty = WorldGen.TileEmpty(col, row);

					if( !isEmpty || tile.wall != 0 || tile.liquid !=0 )
					{
						results.Add(new Point(col, row));

						//if(WorldGen.SolidOrSlopedTile(col,row))
						//{
						//	results.Add(new Point(col, row)); 
						//}
						//else
						//{

						//}
					}
				}
			}

			return results.AsReadOnly();
		}

		/// <summary>
		///     Gets the tile at the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate, which must be in bounds.</param>
		/// <param name="y">The Y coordinate, which must be in bounds.</param>
		/// <returns>The tile.</returns>
		[LuaGlobal]
		public static ITile GetTile(int x, int y) => Main.tile[x, y];

		//[LuaGlobal]
		//public static bool SolidTile(ITile tile)
		//{
		//	return tile.active() && tile.type < Main.maxTileSets && Main.tileSolid[tile.type];
		//}

		[LuaGlobal]
		public static bool IsSolidTile(int column, int row)
		{
			return WorldGen.SolidTile(column, row);
		}

		[LuaGlobal]
		public static bool IsSolidOrSlopedTile(int column, int row)
		{
			return WorldGen.SolidOrSlopedTile(column, row);
		}

		[LuaGlobal]
		public static bool IsWallTile(int column, int row)
		{
			var tile = GetTile(column, row);
			return tile.wall > 0;
		}

		[LuaGlobal]
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
	}
}
