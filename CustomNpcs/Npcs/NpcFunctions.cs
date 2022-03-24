using System;
using System.Collections.Generic;
using System.Linq;
using CustomNpcs.Npcs;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Localization;
using System.Collections.ObjectModel;
using Terraria.DataStructures;

namespace CustomNpcs
{
	/// <summary>
	///     Provides functions for NPC scripts.
	/// </summary>
	public static class NpcFunctions
	{
        public static IEntitySource source { get; private set; }

        /// <summary>
        ///     Finds all custom NPCs with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The array of custom NPCs.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        public static CustomNpc[] FindCustomNpcs(string name)
		{
			if( name == null )
			{
				throw new ArgumentNullException(nameof(name));
			}

			var customNpcs = new List<CustomNpc>();
			foreach( var npc in Main.npc.Where(n => n?.active == true) )
			{
				var customNpc = NpcManager.Instance?.GetCustomNpc(npc);
				if( name.Equals(customNpc?.Definition.Name, StringComparison.OrdinalIgnoreCase) )
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
		public static NPC[] FindNpcs(string name)
		{
			if( name == null )
			{
				throw new ArgumentNullException(nameof(name));
			}

			var npcs = new List<NPC>();
			foreach( var npc in Main.npc.Where(n => n?.active == true) )
			{
				if( NpcManager.Instance?.GetCustomNpc(npc) == null &&
					name.Equals(npc.TypeName, StringComparison.OrdinalIgnoreCase) )
				{
					npcs.Add(npc);
				}
			}
			return npcs.ToArray();
		}

		/// <summary>
		///     Spawns a custom NPC with the specified name at a position.
		/// </summary>
		/// <param name="name">The name, which must be a valid NPC name and not <c>null</c>.</param>
		/// <param name="position">The position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
		/// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
		public static CustomNpc SpawnCustomNpc(string name, Vector2 position)
		{
			if( name == null )
			{
				throw new ArgumentNullException(nameof(name));
			}

			var definition = NpcManager.Instance?.FindDefinition(name);
			if( definition == null )
			{
				throw new FormatException($"Invalid custom NPC name '{name}'.");
			}

			return NpcManager.Instance.SpawnCustomNpc(definition, (int)position.X, (int)position.Y);
		}

		/// <summary>
		///     Spawns a number of CustomNpc's around a given area.
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
		public static CustomNpc[] SpawnCustomNpc(string name, int x, int y, int radius, int amount)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			
			if (radius <= 0)
				throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
			
			if (amount <= 0)
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
			
			var definition = NpcManager.Instance?.FindDefinition(name);
			if (definition == null)
				throw new FormatException($"Invalid CustomNpc name '{name}'.");
			
			var customNpcs = new List<CustomNpc>();
			for (var i = 0; i < amount; ++i)
			{
				TShock.Utils.GetRandomClearTileWithInRange(x, y, radius, radius, out var spawnX, out var spawnY);
				var customNpc = NpcManager.Instance.SpawnCustomNpc(definition, 16 * spawnX, 16 * spawnY);
				if (customNpc != null)
					customNpcs.Add(customNpc);
			}
			return customNpcs.ToArray();
		}

		[Obsolete("SpawnCustomMob() is obsolete, and currently left in for backwards compatibility. Use SpawnCustomNpc() instead.")]
		public static CustomNpc[] SpawnCustomMob(string name, int x, int y, int radius = 10, int amount = 1) => SpawnCustomNpc(name, x, y, radius, amount);

		/// <summary>
		///     Spawns an NPC with the specified name or type at a position.
		/// </summary>
		/// <param name="nameOrType">The name or type, which must be a valid NPC name or type and not <c>null</c>.</param>
		/// <param name="position">The position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="nameOrType" /> is <c>null</c>.</exception>
		/// <exception cref="FormatException"><paramref name="nameOrType" /> is not a valid NPC name.</exception>
		/// <returns>The NPC, or <c>null</c> if spawning failed.</returns>
		public static NPC SpawnNpc(string nameOrType, Vector2 position)
		{
			if( nameOrType == null )
			{
				throw new ArgumentNullException(nameof(nameOrType));
			}

			//var npcType = GetNpcTypeFromNameOrType(nameOrType);
			var npcType = Corruption.NpcFunctions.GetNpcIdFromNameOrType(nameOrType);
			if( npcType == null )
			{
				throw new FormatException($"Invalid NPC name or ID '{nameOrType}'.");
			}

			var npcId = NPC.NewNPC(source,(int)position.X, (int)position.Y, (int)npcType);
			return npcId != Main.maxNPCs ? Main.npc[npcId] : null;
		}

		//Utility function for replacing with a type.
		public static bool IsType(NPC baseNpc, int type)
		{
			return baseNpc.netID == type;
		}

		public static CustomNpc SpawnNpcPart(CustomNpc npc, string customId)
		{
			if( npc == null )
				return null;

			var realNpc = npc.Npc;

			if( realNpc.active != true )
				return null;

			var pos = npc.Center;
			var part = SpawnCustomNpc(customId, pos);
						
			npc.AttachChild(part);
			
			return part;
		}
	}
}
