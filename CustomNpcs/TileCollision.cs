using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace CustomNpcs
{
	public static class TileCollision
	{
		public static ReadOnlyCollection<Point> GetOverlappedTiles(Rectangle bounds)
		{
			const int tileSize = 16;

			//are we in tilemap x bounds?
			if( !( bounds.Right < 0 || bounds.Left > Main.tile.Width * tileSize ) )
			{
				//...tilemap y bounds?
				if( !( bounds.Bottom < 0 || bounds.Top > Main.tile.Height * tileSize ) )
				{
					var min = bounds.TopLeft().ToTileCoordinates();
					var max = bounds.BottomRight().ToTileCoordinates();
					var tileCollisions = getNonEmptyTiles(min.X, min.Y, max.X, max.Y);

					return tileCollisions;
				}
			}

			return new ReadOnlyCollection<Point>(new Point[0]);
		}

		private static ReadOnlyCollection<Point> getNonEmptyTiles(int minColumn, int minRow, int maxColumn, int maxRow)
		{
			var totalColumns = maxColumn - minColumn + 1;
			var totalRows = maxRow - minRow + 1;
			var results = new List<Point>(totalColumns * totalRows);

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

					if( !isEmpty || tile.wall != 0 )
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
	}
}
