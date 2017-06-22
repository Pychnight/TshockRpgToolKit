using System;
using CustomNpcs.Npcs;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Localization;

namespace CustomNpcs
{
    /// <summary>
    ///     Provides functions for NPC scripts.
    /// </summary>
    public static class NpcFunctions
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
        ///     Gets the region with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The region, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static Region GetRegion([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return TShock.Regions.GetRegionByName(name);
        }

        /// <summary>
        ///     Determines whether the specified coordinates are in the region.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="regionName">The name, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the region exists and the coordinate are in the region; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="regionName" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static bool IsInRegion(int x, int y, [NotNull] string regionName)
        {
            if (regionName == null)
            {
                throw new ArgumentNullException(nameof(regionName));
            }

            return GetRegion(regionName)?.InArea(x, y) ?? false;
        }

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

            var definition = NpcManager.Instance.FindDefinition(name);
            if (definition == null)
            {
                throw new FormatException($"Invalid custom NPC name '{name}'.");
            }

            for (var i = 0; i < amount; ++i)
            {
                TShock.Utils.GetRandomClearTileWithInRange(x, y, radius, radius, out var spawnX, out var spawnY);
                NpcManager.Instance.SpawnCustomNpc(definition, 16 * spawnX, 16 * spawnY);
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
                if (npcName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return i;
                }
            }
            return null;
        }
    }
}
