using System;
using System.Collections.Generic;
using System.Linq;
using CustomNpcs.Npcs;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua;
using OTAPI.Tile;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Localization;

namespace CustomNpcs
{
    /// <summary>
    ///     Provides functions for NPC scripts.
    /// </summary>
    [PublicAPI]
    public static class NpcFunctions
    {
        private static readonly Random Random = new Random();

        /// <summary>
        ///     Broadcasts the specified message.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [LuaGlobal]
        public static void Broadcast([NotNull] string message, Color color)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            TShock.Utils.Broadcast(message, color);
        }

        /// <summary>
        ///     Creates a combat text with the specified color and position.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="position">The position.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text" /> is <c>null</c>.</exception>
        [LuaGlobal]
        public static void CreateCombatText([NotNull] string text, Color color, Vector2 position)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            TSPlayer.All.SendData((PacketTypes)119, text, (int)color.PackedValue, position.X, position.Y);
        }

        /// <summary>
        ///     Finds all custom NPCs with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The array of custom NPCs.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [LuaGlobal]
        public static CustomNpc[] FindCustomNpcs([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var customNpcs = new List<CustomNpc>();
            foreach (var npc in Main.npc.Where(n => n?.active == true))
            {
                var customNpc = NpcManager.Instance?.GetCustomNpc(npc);
                if (name.Equals(customNpc?.Definition.Name, StringComparison.OrdinalIgnoreCase))
                {
                    customNpcs.Add(customNpc);
                }
            }
            return customNpcs.ToArray();
        }

        /// <summary>
        ///     Finds all NPCs with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The array of NPCs.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [LuaGlobal]
        public static NPC[] FindNpcs([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var npcs = new List<NPC>();
            foreach (var npc in Main.npc.Where(n => n?.active == true))
            {
                if (NpcManager.Instance?.GetCustomNpc(npc) == null &&
                    name.Equals(npc.TypeName, StringComparison.OrdinalIgnoreCase))
                {
                    npcs.Add(npc);
                }
            }
            return npcs.ToArray();
        }

        /// <summary>
        ///     Gets the region with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The region, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [LuaGlobal]
        public static Region GetRegion([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return TShock.Regions.GetRegionByName(name);
        }

        /// <summary>
        ///     Gets the tile at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate, which must be in bounds.</param>
        /// <param name="y">The Y coordinate, which must be in bounds.</param>
        /// <returns>The tile.</returns>
        [LuaGlobal]
        public static ITile GetTile(int x, int y) => Main.tile[x, y];

        /// <summary>
        ///     Gets a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>The number.</returns>
        [LuaGlobal]
        public static double RandomDouble() => Random.NextDouble();

        /// <summary>
        ///     Gets a random integer between the specified values.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum, which must be at least <paramref name="min" />.</param>
        /// <returns>The integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="max" /> is less than <paramref name="min" />.</exception>
        [LuaGlobal]
        public static int RandomInt(int min, int max)
        {
            if (max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "Maximum must be at least the minimum.");
            }

            return Random.Next(min, max);
        }

        /// <summary>
        ///     Spawns a custom NPC with the specified name at a position.
        /// </summary>
        /// <param name="name">The name, which must be a valid NPC name and not <c>null</c>.</param>
        /// <param name="position">The position.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
        /// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
        [LuaGlobal]
        public static CustomNpc SpawnCustomNpc([NotNull] string name, Vector2 position)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var definition = NpcManager.Instance?.FindDefinition(name);
            if (definition == null)
            {
                throw new FormatException($"Invalid custom NPC name '{name}'.");
            }

            return NpcManager.Instance.SpawnCustomNpc(definition, (int)position.X, (int)position.Y);
        }

        /// <summary>
        ///     Spawns an NPC with the specified name or type at a position.
        /// </summary>
        /// <param name="nameOrType">The name or type, which must be a valid NPC name or type and not <c>null</c>.</param>
        /// <param name="position">The position.</param>
        /// <exception cref="ArgumentNullException"><paramref name="nameOrType" /> is <c>null</c>.</exception>
        /// <exception cref="FormatException"><paramref name="nameOrType" /> is not a valid NPC name.</exception>
        /// <returns>The NPC, or <c>null</c> if spawning failed.</returns>
        [LuaGlobal]
        public static NPC SpawnNpc([NotNull] string nameOrType, Vector2 position)
        {
            if (nameOrType == null)
            {
                throw new ArgumentNullException(nameof(nameOrType));
            }

            var npcType = GetNpcTypeFromNameOrType(nameOrType);
            if (npcType == null)
            {
                throw new FormatException($"Invalid NPC name or ID '{nameOrType}'.");
            }

            var npcId = NPC.NewNPC((int)position.X, (int)position.Y, (int)npcType);
            return npcId != Main.maxNPCs ? Main.npc[npcId] : null;
        }

        private static int? GetNpcTypeFromNameOrType(string nameOrType)
        {
            if (int.TryParse(nameOrType, out var id) && -65 <= id && id < Main.maxNPCTypes)
            {
                return id;
            }

            for (var i = -65; i < Main.maxNPCTypes; ++i)
            {
                var npcName = EnglishLanguage.GetNpcNameById(i);
                if (npcName?.Equals(nameOrType, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return i;
                }
            }
            return null;
        }
    }
}
