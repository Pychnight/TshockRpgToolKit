using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.TEdit
{
	public static class ITileExtensions
	{
		/// <summary>
		/// Copies members from a TEdit style Tile to an OTAPI.Tile.ITile. 
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="src"></param>
		public static void CopyFrom(this OTAPI.Tile.ITile dst, Tile src)
		{
			//still needs support for wires.

			dst.type = src.Type;
			dst.wall = src.Wall;
			dst.liquid = (byte)src.LiquidType;

			//dst.sTileHeader = src.

			dst.active(src.IsActive);
			dst.inActive(src.InActive);
			dst.actuator(src.Actuator);
			dst.color(src.TileColor);
			dst.wallColor(src.WallColor);

			switch( src.BrickStyle )
			{
				case BrickStyle.Full:
					dst.halfBrick(false);
					break;

				case BrickStyle.HalfBrick:
					dst.halfBrick(true);
					break;

				case BrickStyle.SlopeTopRight:
				case BrickStyle.SlopeTopLeft:
				case BrickStyle.SlopeBottomRight:
				case BrickStyle.SlopeBottomLeft:
					dst.slope((byte)src.BrickStyle);
					break;
			}

			dst.frameX = src.U;
			dst.frameY = src.V;
		}

		/// <summary>
		/// Copies members from an ITile to a TEdit style Tile.
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="src"></param>
		public static void CopyFrom(this Tile dst, OTAPI.Tile.ITile src )
		{
			//still needs support for wires.
			dst.Type = src.type;
			dst.Wall = src.wall;
			dst.LiquidType = (LiquidType)src.liquid;

			dst.IsActive = src.active();
			dst.InActive = src.inActive();
			dst.Actuator = src.actuator();
			dst.TileColor = src.color();
			dst.WallColor = src.wallColor();

			if(!src.halfBrick())
			{
				dst.BrickStyle = BrickStyle.Full;
			}
			else
			{
				switch(src.slope())
				{
					case 2:
						dst.BrickStyle = BrickStyle.SlopeTopRight;
						break;
					case 3:
						dst.BrickStyle = BrickStyle.SlopeTopLeft;
						break;
					case 4:
						dst.BrickStyle = BrickStyle.SlopeBottomRight;
						break;
					case 5:
						dst.BrickStyle = BrickStyle.SlopeBottomLeft;
						break;
					default:
						dst.BrickStyle = BrickStyle.HalfBrick;
						break;
				}
			}
			
			dst.U = src.frameX;
			dst.V = src.frameY;
		}
	}
}
