using System;
using CustomNPC;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua;
using OTAPI.Tile;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;

namespace CustomQuests
{
    /// <summary>
    ///     Provides functions for quest scripts.
    /// </summary>
    public static class QuestFunctions
    {
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
        /// <param name="str">The command string, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the command was executed successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="str" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static bool ExecuteCommand([NotNull] string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return Commands.HandleCommand(TSPlayer.Server, str);
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
        ///     Spawns the custom mob with the specified name, coordinates, and amount.
        /// </summary>
        /// <param name="name">The name, which must be a valid NPC name and not <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="radius">The radius, which must be positive.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Either <paramref name="radius" /> or <paramref name="amount" /> is not positive.
        /// </exception>
        /// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static void SpawnCustomMob([NotNull] string name, int x, int y, int radius = 10, int amount = 1)
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

            var npcDef = NPCManager.Data.GetNPCbyID(name);
            if (npcDef == null)
            {
                throw new FormatException($"Invalid custom NPC name '{name}'.");
            }

            for (var i = 0; i < amount; ++i)
            {
                TShock.Utils.GetRandomClearTileWithInRange(x, y, radius, radius, out var spawnX, out var spawnY);
                NPCManager.SpawnNPCAtLocation(16 * spawnX, 16 * spawnY, npcDef);
            }
        }

        /// <summary>
        ///     Spawns the mob with the specified name, coordinates, and amount.
        /// </summary>
        /// <param name="name">The name, which must be a valid NPC name and not <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="radius">The radius, which must be positive.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Either <paramref name="radius" /> or <paramref name="amount" /> is not positive.
        /// </exception>
        /// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static void SpawnMob([NotNull] string name, int x, int y, int radius = 10, int amount = 1)
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

            for (var i = 0; i < amount; ++i)
            {
                TShock.Utils.GetRandomClearTileWithInRange(x, y, radius, radius, out var spawnX, out var spawnY);
                NPC.NewNPC(16 * spawnX, 16 * spawnY, (int)npcId);
            }
        }

        private static int? GetNpcIdFromName(string name)
        {
            for (var i = -65; i < Main.maxNPCTypes; ++i)
            {
                var npcName = EnglishLanguage.GetNpcNameById(i);
                if (npcName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return null;
        }
    }
}
