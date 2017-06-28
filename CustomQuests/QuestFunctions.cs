using System;
using System.Collections.Generic;
using CustomNpcs.Npcs;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua;
using OTAPI.Tile;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;

// ReSharper disable InconsistentNaming

namespace CustomQuests
{
    /// <summary>
    ///     Provides functions for quest scripts.
    /// </summary>
    public static class QuestFunctions
    {
        private static readonly Random Random = new Random();

        /// <summary>
        ///     Broadcasts the specified message.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static void Broadcast([NotNull] string message, Color color)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            TShock.Utils.Broadcast(message, color);
        }

        /// <summary>
        ///     Broadcasts the specified message.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static void Broadcast([NotNull] string message, byte r, byte g, byte b)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            TShock.Utils.Broadcast(message, r, g, b);
        }

        /// <summary>
        ///     Executes the specified command as the server.
        /// </summary>
        /// <param name="command">The command string, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the command was executed successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static bool ExecuteCommand([NotNull] string command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return Commands.HandleCommand(TSPlayer.Server, command);
        }

        /// <summary>
        ///     Gets the tile located at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
        /// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
        /// <returns>The tile.</returns>
        [LuaGlobal]
        [Pure]
        [UsedImplicitly]
        public static ITile GetTile(int x, int y) => Main.tile[x, y];

        /// <summary>
        ///     Gets the type of the specified tile.
        /// </summary>
        /// <param name="tile">The tile, which must not be <c>null</c>.</param>
        /// <returns>The type.</returns>
        /// <remarks>
        ///     This method is required since we can't get the type property in Lua since it is an unsigned short.
        /// </remarks>
        [LuaGlobal]
        [UsedImplicitly]
        public static int GetTileType([NotNull] ITile tile) => tile.type;

        /// <summary>
        ///     Places a 1x2 object.
        /// </summary>
        /// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
        /// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
        /// <param name="type">The type.</param>
        /// <param name="style">The style.</param>
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
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
        [LuaGlobal]
        [UsedImplicitly]
        public static void Place6x4Wall(int x, int y, int type, int style)
        {
            WorldGen.Place6x4Wall(x, y, (ushort)type, style);
        }

        /// <summary>
        ///     Places a chest at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
        /// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
        /// <param name="style">The style.</param>
        [LuaGlobal]
        [UsedImplicitly]
        public static void PlaceChest(int x, int y, int style)
        {
            var chestId = Chest.FindChest(x, y - 1);
            if (chestId != -1)
            {
                return;
            }

            chestId = WorldGen.PlaceChest(x, y, style: style);
            if (chestId != -1)
            {
                TSPlayer.All.SendData((PacketTypes)34, "", 0, x, y, style, chestId);
                return;
            }

            for (var i = x; i < x + 2; ++i)
            {
                for (var j = y - 1; j < y + 2; ++j)
                {
                    var tile = Main.tile[i, j];
                    if (j == y + 1)
                    {
                        tile.active(true);
                        tile.type = 0;
                    }
                    else
                    {
                        tile.active(false);
                    }
                }
            }
            TSPlayer.All.SendTileSquare(x, y, 3);

            chestId = WorldGen.PlaceChest(x, y, style: style);
            if (chestId != -1)
            {
                TSPlayer.All.SendData((PacketTypes)34, "", 0, x, y, style, chestId);
            }
        }

        /// <summary>
        ///     Puts an item into the chest at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate, which must be within the bounds of the world.</param>
        /// <param name="y">The Y coordinate, which must be within the bounds of the world.</param>
        /// <param name="type">The type.</param>
        /// <param name="stack">The stack.</param>
        /// <param name="prefix">The prefix.</param>
        [LuaGlobal]
        [UsedImplicitly]
        public static void PutItemIntoChest(int x, int y, int type, int stack = 1, byte prefix = 0)
        {
            var chestId = Chest.FindChest(x, y - 1);
            if (chestId == -1)
            {
                return;
            }

            var chest = Main.chest[chestId];
            for (var i = 0; i < Chest.maxItems; ++i)
            {
                var item = chest.item[i];
                if (item.netID == 0)
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
        ///     Returns a random integer in the specified range.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum, which must be at least the minimum.</param>
        /// <returns>The random integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="max" /> is less than <paramref name="min" />.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static int RandomInt(int min, int max)
        {
            if (max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "Maximum is smaller than the minimum.");
            }

            return Random.Next(min, max);
        }

        /// <summary>
        ///     Sets the type of the specified tile.
        /// </summary>
        /// <param name="tile">The tile, which must not be <c>null</c>.</param>
        /// <param name="type">The type.</param>
        /// <remarks>
        ///     This method is required, since we can't set the type property in Lua since it is an unsigned short.
        /// </remarks>
        [LuaGlobal]
        [UsedImplicitly]
        public static void SetTileType([NotNull] ITile tile, int type)
        {
            tile.type = (ushort)type;
        }

        /// <summary>
        ///     Spawns the custom mob with the specified name, coordinates, and amount.
        /// </summary>
        /// <param name="name">The name, which must be a valid NPC name and not <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="radius">The radius, which must be positive.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <returns>The spawned NPCs.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Either <paramref name="radius" /> or <paramref name="amount" /> is not positive.
        /// </exception>
        /// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static CustomNpc[] SpawnCustomMob([NotNull] string name, int x, int y, int radius = 10, int amount = 1)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
            }

            var definition = NpcManager.Instance?.FindDefinition(name);
            if (definition == null)
            {
                throw new FormatException($"Invalid custom NPC name '{name}'.");
            }

            var customNpcs = new List<CustomNpc>();
            for (var i = 0; i < amount; ++i)
            {
                TShock.Utils.GetRandomClearTileWithInRange(x, y, radius, radius, out var spawnX, out var spawnY);
                var customNpc = NpcManager.Instance.SpawnCustomNpc(definition, 16 * spawnX, 16 * spawnY);
                if (customNpc != null)
                {
                    customNpcs.Add(customNpc);
                }
            }
            return customNpcs.ToArray();
        }

        /// <summary>
        ///     Spawns the mob with the specified name, coordinates, and amount.
        /// </summary>
        /// <param name="name">The name, which must be a valid NPC name and not <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="radius">The radius, which must be positive.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <returns>The spawned NPCs.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Either <paramref name="radius" /> or <paramref name="amount" /> is not positive.
        /// </exception>
        /// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static NPC[] SpawnMob([NotNull] string name, int x, int y, int radius = 10, int amount = 1)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
            }

            var npcId = GetNpcIdFromName(name);
            if (npcId == null)
            {
                throw new FormatException($"Invalid NPC name '{name}'.");
            }

            var npcs = new List<NPC>();
            for (var i = 0; i < amount; ++i)
            {
                TShock.Utils.GetRandomClearTileWithInRange(x, y, radius, radius, out var spawnX, out var spawnY);
                var npcIndex = NPC.NewNPC(16 * spawnX, 16 * spawnY, (int)npcId);
                if (npcIndex != Main.maxNPCs)
                {
                    npcs.Add(Main.npc[npcIndex]);
                }
            }
            return npcs.ToArray();
        }

        private static int? GetNpcIdFromName(string name)
        {
            for (var i = -65; i < Main.maxNPCTypes; ++i)
            {
                var npcName = EnglishLanguage.GetNpcNameById(i);
                if (npcName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return i;
                }
            }
            return null;
        }
    }
}
