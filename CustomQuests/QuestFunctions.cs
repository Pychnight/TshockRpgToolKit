using System;
using System.Collections.Generic;
using CustomNpcs.Npcs;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;
using TShockAPI.DB;
using System.Diagnostics;
using Corruption;
using Corruption.PluginSupport;

namespace CustomQuests
{
    /// <summary>
    ///     Provides functions for quest scripts.
    /// </summary>
    public static class QuestFunctions
    {
    	public static void DebugLog(string txt)
		{
			Debug.Print(txt);
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
