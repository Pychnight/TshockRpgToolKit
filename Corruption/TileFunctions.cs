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
using TShockAPI.DB;

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
			return GetOverlappedTiles(bounds, DefaultTileFilterFunc);
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
					if( filterFunc(col, row) )
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
		///     Gets the tile located at the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <returns>The tile.</returns>
		public static ITile GetTile(int x, int y) => Main.tile[x, y];

		/// <summary>
		///     Gets the type of the specified tile.
		/// </summary>
		/// <param name="tile">The tile, which must not be <c>null</c>.</param>
		/// <returns>The type.</returns>
		/// <remarks>
		///     This method is required since we can't get the type property in Lua since it is an unsigned short.
		/// </remarks>
		public static int GetTileType(ITile tile) => tile.type;

		/// <summary>
		///     Sets the type of the specified tile.
		/// </summary>
		/// <param name="tile">The tile, which must not be <c>null</c>.</param>
		/// <param name="type">The type.</param>
		/// <remarks>
		///     This method is required, since we can't set the type property in Lua since it is an unsigned short.
		/// </remarks>
		public static void SetTileType(ITile tile, int type)
		{
			tile.type = (ushort)type;
		}

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
			catch( IndexOutOfRangeException rex )
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
			catch( IndexOutOfRangeException rex )
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

		public static int CountBlocks(int x, int y, int x2, int y2, int id)
		{
			var count = 0;
			var minX = Math.Min(x, x2);
			var maxX = Math.Max(x, x2);
			var minY = Math.Min(y, y2);
			var maxY = Math.Max(y, y2);

			for( var i = minX; i <= maxX; i++ )
			{
				for( var j = minY; j <= maxY; j++ )
				{
					if( MatchesBlock(i, j, id) )
						count = count + 1;
				}
			}
			return count;
		}

		// Counts the number of blocks in the area matching the ID and frames. (Replaced in Corruption.TileFunctions. )
		public static int CountBlocksWithFrames(int x, int y, int x2, int y2, int id, int frameX, int frameY)
		{
			var count = 0;
			var minX = Math.Min(x, x2);
			var maxX = Math.Max(x, x2);
			var minY = Math.Min(y, y2);
			var maxY = Math.Max(y, y2);
			
			for( var i = minX; i<= maxX; i++)
			{ 
				for( var j = minY; j<= maxY; j++)
				{
					//if( MatchesBlock(i, j, id)) //script error? no overload exists for 5 args -->  frameX, frameY) )
					if( MatchesBlockWithFrames(i, j, id, frameX, frameY) ) //script error? no overload exists for 5 args -->  frameX, frameY) )
						count = count + 1;
				}
			}
			return count;
		}

		// Counts the number of walls in the area matching the ID.
		public static int CountWalls(int x, int y, int x2, int y2, int id)
		{
			var count = 0;
			var minX = Math.Min(x, x2);
			var maxX = Math.Max(x, x2);
			var minY = Math.Min(y, y2);
			var maxY = Math.Max(y, y2);
			
			for( var i = minX; i <= maxX; i++ )
			{ 
				for( var j = minY; j<= maxY; j++ )
				{
					if( MatchesWall(i, j, id))
						count = count + 1;
				}
			}
			return count;
		}

		// Determines if the block at the coordinates matches the ID. ( From CustomQuests utils.lua )
		public static bool MatchesBlock(int x, int y, object id)
		{
			var stringId = id as string;
			var tile = GetTile(x, y);

			if( stringId == "air" )
				return !tile.active() && tile.liquid == 0;
			else if( stringId == "water" )
				return !tile.active() && tile.liquid > 0 && tile.liquidType() == 0;
			else if( stringId == "lava" )
				return !tile.active() && tile.liquid > 0 && tile.liquidType() == 1;
			else if( stringId == "honey" )
				return !tile.active() && tile.liquid > 0 && tile.liquidType() == 2;
			else if( id is int )
				return tile.active() && GetTileType(tile) == (int)id;
			else
				return false;
		}

		// Determines if the block at the coordinates matches the ID and frames. (from CustomQuests utils.lua )
		public static bool MatchesBlockWithFrames(int x, int y, int id, int frameX, int frameY)
		{
			var tile = GetTile(x, y);
			return tile.active() && GetTileType(tile) == id && tile.frameX == frameX && tile.frameY == frameY;
		}

		// Determines if the wall at the coordinates matches the ID. ( from CustomQuests utils.lua )
		public static bool MatchesWall(int x, int y, int id)
		{
			var tile = GetTile(x, y);
			return tile.type == id;
		}

		public static void ClearBlock(int x, int y)
		{
			if( Main.tile[x, y]?.active() == true )
			{
				WorldGen.KillTile(x, y);
			}
		}

		// Sets the blocks in the area.
		public static void ClearBlocks(int x, int y, int x2, int y2)
		{
			var minX = Math.Min(x, x2);
			var maxX = Math.Max(x, x2);
			var minY = Math.Min(y, y2);
			var maxY = Math.Max(y, y2);
			for( var i = minX; i <= maxX; i++ )
			{
				for( var j = minY; j <= maxY; j++ )
				{
					ClearBlock(i, j);
				}
			}
		}

		public static void ClearBlocks(Rectangle rect)
		{
			ClearBlocks(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static void ClearBlocks(Region region)
		{
			ClearBlocks(region.Area);
		}

		// Sets the block at the coordinates to the ID.
		public static void SetBlock(int x, int y, object id)
		{
			var stringId = id as string;

			var tile = GetTile(x, y);
			if( stringId == "air" )
			{
				tile.active(false);
				tile.liquid = 0;
				tile.type = 0;
			}
			else if( stringId == "water" )
			{
				tile.active(false);
				tile.liquid = 255;
				tile.liquidType(0);
				tile.type = 0;
			}
			else if( stringId == "lava")
			{
				tile.active(false);
				tile.liquid = 255;
				tile.liquidType(1);
				tile.type = 0;
			}
			else if( stringId == "honey")
			{
				tile.active(false);
				tile.liquid = 255;
				tile.liquidType(2);
				tile.type = 0;
			}
			else if(id is int)
			{ 
				tile.active(true);
				tile.liquid = 0;
				SetTileType(tile, (int)id);
			}
		}

		// Sets the blocks in the area.
		public static void SetBlocks(int x, int y, int x2, int y2, object id)
		{
			var minX = Math.Min(x, x2);
			var maxX = Math.Max(x, x2);
			var minY = Math.Min(y, y2);
			var maxY = Math.Max(y, y2);
			for( var i = minX; i <= maxX; i++ )
			{
				for( var j = minY; j <= maxY; j++ )
				{
					SetBlock(i, j, id);
				}
			}
		}

		public static void SetBlocks(Rectangle rect, object id)
		{
			SetBlocks(rect.Left, rect.Top, rect.Right, rect.Bottom, id);
		}

		public static void SetBlocks(Region region, object id)
		{
			SetBlocks(region.Area, id);
		}
		
		// Sets the walls at the coordinates.
		public static void SetWall(int x, int y, int id)
		{
			var tile = GetTile(x, y);
			tile.wall = (byte)id;
		}

		// Sets the walls in the area.
		public static void SetWalls(int x, int y, int x2, int y2, int id)
		{
			var minX = Math.Min(x, x2);
			var maxX = Math.Max(x, x2);
			var minY = Math.Min(y, y2);
			var maxY = Math.Max(y, y2);
			for( var i = minX; i <= maxX; i++ )
			{
				for( var j = minY; j <= maxY; j++ )
				{
					SetWall(i, j, id);
				}
			}
		}
		/// <summary>
		///     Places a 1x2 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place1x2(int x, int y, int type, int style)
		{
			WorldGen.Place1x2(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 1x2 object on top of something.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place1x2Top(int x, int y, int type, int style)
		{
			WorldGen.Place1x2Top(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 1xX object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place1xX(int x, int y, int type, int style)
		{
			WorldGen.Place1xX(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 2x1 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place2x1(int x, int y, int type, int style)
		{
			WorldGen.Place2x1(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 2x2 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place2x2(int x, int y, int type, int style)
		{
			WorldGen.Place2x2(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 2x3 object on a wall (usually a painting).
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place2x3Wall(int x, int y, int type, int style)
		{
			WorldGen.Place2x3Wall(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 2xX object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place2xX(int x, int y, int type, int style)
		{
			WorldGen.Place2xX(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 3x1 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place3x1(int x, int y, int type, int style)
		{
			WorldGen.Place3x1(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 3x2 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place3x2(int x, int y, int type, int style)
		{
			WorldGen.Place3x2(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 3x2 object on a wall (usually a painting).
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place3x2Wall(int x, int y, int type, int style)
		{
			WorldGen.Place3x2Wall(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 3x3 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place3x3(int x, int y, int type, int style)
		{
			WorldGen.Place3x3(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 3x3 object on a wall (usually a painting).
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place3x3Wall(int x, int y, int type, int style)
		{
			WorldGen.Place3x3Wall(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 3x4 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place3x4(int x, int y, int type, int style)
		{
			WorldGen.Place3x4(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 4x2 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place4x2(int x, int y, int type, int style)
		{
			WorldGen.Place4x2(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 4x3 object on a wall (usually a painting).
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place4x3Wall(int x, int y, int type, int style)
		{
			WorldGen.Place4x3Wall(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 5x4 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place5x4(int x, int y, int type, int style)
		{
			WorldGen.Place5x4(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 6x4 object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place6x3(int x, int y, int type, int style)
		{
			WorldGen.Place6x3(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places a 6x4 object on a wall (usually a painting).
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void Place6x4Wall(int x, int y, int type, int style)
		{
			WorldGen.Place6x4Wall(x, y, (ushort)type, style);
		}

		/// <summary>
		///     Places an object.
		/// </summary>
		/// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
		/// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
		/// <param name="type">The type.</param>
		/// <param name="style">The style.</param>
		public static void PlaceObject(int x, int y, int type, int style)
		{
			WorldGen.PlaceObject(x, y, (ushort)type, false, style);
		}
	}
}
