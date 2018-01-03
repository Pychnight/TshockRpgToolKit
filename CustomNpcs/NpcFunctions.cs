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
using System.Collections.ObjectModel;

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
        ///     Bans Players
        /// </summary>
        /// <param name="player">The message, which must not be <c>null</c>.</param>
        /// <param name="message">The color.</param>
        /// <param name="message2">The color.</param>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
        [LuaGlobal]
        public static void Ban([NotNull] TSPlayer player, string message, string message2)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            TShock.Utils.Ban(player, message, true, message2);
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

        ///// <summary>
        /////     Gets the tile at the specified coordinates.
        ///// </summary>
        ///// <param name="x">The X coordinate, which must be in bounds.</param>
        ///// <param name="y">The Y coordinate, which must be in bounds.</param>
        ///// <returns>The tile.</returns>
        //[LuaGlobal]
        //public static ITile GetTile(int x, int y) => Main.tile[x, y];

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

		public static ReadOnlyCollection<TSPlayer> FindPlayersInRadius(float x, float y, float radius)
		{
			var results = new List<TSPlayer>();
			var center = new Vector2(x, y);

			foreach( var player in TShock.Players )
			{
				if(player!=null && player.Active)
				{
					var pos = new Vector2(player.X, player.Y);
					var delta = pos - center;

					if( delta.LengthSquared() <= ( radius * radius ) )
						results.Add(player);
				}
			}

			return results.AsReadOnly();
		}

		[LuaGlobal]
		public static void RadialDamagePlayer(int x, int y, int radius, int damage, float falloff)
		{
			if( radius < 1 )
				return;

			var center = new Vector2(x, y);
			//falloff = Math.Min(falloff, 1.0f);//clip to 1
		
			foreach(var player in FindPlayersInRadius(x,y,radius))
			{
				var pos = new Vector2(player.X, player.Y);
				var delta = pos - center;
				var dist = delta.LengthSquared();
				var damageStep = dist / ( radius * radius );
				
				var adjustedDamage = damage * (1.0f - (damageStep * falloff));

				if(adjustedDamage>1)
					player.DamagePlayer((int)adjustedDamage);

				//...internally, DamagePlayer looks like this: 
				//NetMessage.SendPlayerHurt(this.Index, PlayerDeathReason.LegacyDefault(), damage, new Random().Next(-1, 1), false, false, 0, -1, -1);
			
				//might be able to do our knockback effect?	
			}
		}
    }
}
