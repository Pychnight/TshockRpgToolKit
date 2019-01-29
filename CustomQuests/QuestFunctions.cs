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

// ReSharper disable InconsistentNaming

namespace CustomQuests
{
    /// <summary>
    ///     Provides functions for quest scripts.
    /// </summary>
    public static class QuestFunctions
    {
        private static readonly Random Random = new Random();

		public static void DebugLog(string txt)
		{
			Debug.Print(txt);
		}

        /// <summary>
        ///     Executes the specified command as the server.
        /// </summary>
        /// <param name="command">The command string, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the command was executed successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command" /> is <c>null</c>.</exception>
        public static bool ExecuteCommand(string command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return Commands.HandleCommand(TSPlayer.Server, command);
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
		
		/// <summary>
		///     Returns a random integer in the specified range.
		/// </summary>
		/// <param name="min">The minimum.</param>
		/// <param name="max">The maximum, which must be at least the minimum.</param>
		/// <returns>The random integer.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="max" /> is less than <paramref name="min" />.</exception>
		public static int RandomInt(int min, int max)
        {
            if (max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "Maximum is smaller than the minimum.");
            }

            return Random.Next(min, max);
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
        public static CustomNpc[] SpawnCustomMob(string name, int x, int y, int radius = 10, int amount = 1)
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
        /// <param name="nameOrType">The name or type, which must be a valid NPC name and not <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="radius">The radius, which must be positive.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <returns>The spawned NPCs.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="nameOrType" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Either <paramref name="radius" /> or <paramref name="amount" /> is not positive.
        /// </exception>
        /// <exception cref="FormatException"><paramref name="nameOrType" /> is not a valid NPC name.</exception>
        public static NPC[] SpawnMob(string nameOrType, int x, int y, int radius = 10, int amount = 1)
        {
            if (nameOrType == null)
				throw new ArgumentNullException(nameof(nameOrType));
                        
			//var npcId = GetNpcIdFromName(name);
			var npcId = Corruption.NpcFunctions.GetNpcIdFromNameOrType(nameOrType);
            if (npcId == null)
				throw new FormatException($"Invalid NPC name '{nameOrType}'.");
            
			return SpawnMob((int)npcId, x, y, radius, amount);
        }

		/// <summary>
		///     Spawns the mob with the specified name, coordinates, and amount.
		/// </summary>
		/// <param name="type">The type, which must be a valid NPC type.</param>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		/// <param name="radius">The radius, which must be positive.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <returns>The spawned NPCs.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///     Either <paramref name="radius" /> or <paramref name="amount" /> is not positive.
		/// </exception>
		/// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
		public static NPC[] SpawnMob(int type, int x, int y, int radius = 10, int amount = 1)
		{
			if( radius <= 0 )
				throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
			
			if( amount <= 0 )
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
						
			var npcs = new List<NPC>();
			for( var i = 0; i < amount; ++i )
			{
				TShock.Utils.GetRandomClearTileWithInRange(x, y, radius, radius, out var spawnX, out var spawnY);
				var npcIndex = NPC.NewNPC(16 * spawnX, 16 * spawnY, type);
				if( npcIndex != Main.maxNPCs )
				{
					npcs.Add(Main.npc[npcIndex]);
				}
			}
			return npcs.ToArray();
		}

		/// <summary>
		///		Spawns a Projectile. 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="speedX"></param>
		/// <param name="speedY"></param>
		/// <param name="type"></param>
		/// <param name="damage"></param>
		/// <param name="knockBack"></param>
		/// <param name="owner"></param>
		/// <param name="ai0"></param>
		/// <param name="ai1"></param>
		/// <param name="amount"></param>
		/// <returns>Projectile array.</returns>
		public static Projectile[] SpawnProjectile(float x, float y, float speedX, float speedY, int type, int damage, float knockBack, int owner, float ai0, float ai1, int amount)
		{
			var projectiles = new Projectile[amount];

			for(var i = 0;i<projectiles.Length;i++ )
			{
				var projectileId = Projectile.NewProjectile(x, y, speedX, speedY, type, damage, knockBack, owner, ai0, ai1);
				TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectileId);
				projectiles[i] = Main.projectile[projectileId];
			}
			
			return projectiles;
		}
		
		/// <summary>
		///     Spawns a custom projectile with the specified name at a position.
		/// </summary>
		/// <param name="name">The name, which must be a valid projectile name and not <c>null</c>.</param>
		/// <param name="position">The position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="FormatException"><paramref name="name" /> is not a valid NPC name.</exception>
		/// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
		public static Projectile[] SpawnCustomProjectile(int owner, string name, float x, float y, float xSpeed, float ySpeed, int amount )
		{
			var projectiles = new Projectile[amount];
						
			for(var i = 0;i<projectiles.Length; i++)
			{
				var cp = CustomNpcs.ProjectileFunctions.SpawnCustomProjectile(owner, name, x, y, xSpeed, ySpeed);

				if( cp != null )
				{
					projectiles[i] = cp.Projectile;
				}
			}
			
			return projectiles;
		}
	}
}
