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

namespace CustomNpcs
{
	/// <summary>
	///     Provides functions for NPC scripts.
	/// </summary>
	public static class NpcFunctions
	{
		private static readonly Random Random = new Random();
		
		/// <summary>
		///     Bans Players
		/// </summary>
		/// <param name="player">The message, which must not be <c>null</c>.</param>
		/// <param name="message">The color.</param>
		/// <param name="message2">The color.</param>
		/// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
		public static void Ban(TSPlayer player, string message, string message2)
		{
			if( message == null )
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
		public static void CreateCombatText(string text, Color color, Vector2 position)
		{
			if( text == null )
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



		///// <summary>
		/////     Gets the tile at the specified coordinates.
		///// </summary>
		///// <param name="x">The X coordinate, which must be in bounds.</param>
		///// <param name="y">The Y coordinate, which must be in bounds.</param>
		///// <returns>The tile.</returns>
		//
		//public static ITile GetTile(int x, int y) => Main.tile[x, y];

		/// <summary>
		///     Gets a random number between 0.0 and 1.0.
		/// </summary>
		/// <returns>The number.</returns>
		public static double RandomDouble() => Random.NextDouble();

		/// <summary>
		///     Gets a random integer between the specified values.
		/// </summary>
		/// <param name="min">The minimum.</param>
		/// <param name="max">The maximum, which must be at least <paramref name="min" />.</param>
		/// <returns>The integer.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="max" /> is less than <paramref name="min" />.</exception>
		public static int RandomInt(int min, int max)
		{
			if( max < min )
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

			var npcId = NPC.NewNPC((int)position.X, (int)position.Y, (int)npcType);
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
