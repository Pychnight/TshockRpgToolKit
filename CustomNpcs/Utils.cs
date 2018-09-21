using System;
using System.Collections.Generic;
using System.Linq;
using CustomNpcs.Npcs;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using Terraria;
using TShockAPI;
using System.Diagnostics;
using TerrariaApi.Server;
using Corruption.PluginSupport;

namespace CustomNpcs
{
    /// <summary>
    ///     Holds utility functions.
    /// </summary>
    internal static class Utils
    {
        private static readonly Random Random = new Random();

		public static void LogScriptRuntimeError(Exception ex)
		{
			CustomNpcsPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
			CustomNpcsPlugin.Instance.LogPrint("Disabling event callback.", TraceLevel.Error);
		}

		public static void LogScriptRuntimeError(string message)
		{
			CustomNpcsPlugin.Instance.LogPrint(message, TraceLevel.Error);
			CustomNpcsPlugin.Instance.LogPrint("Disabling event callback.", TraceLevel.Error);
		}

		/// <summary>
		///     Calculates spawn data for the specified player based on the global spawn data.
		/// </summary>
		/// <param name="player">The player, which must not be <c>null</c>.</param>
		/// <param name="maxSpawns">The number of maximum spawns.</param>
		/// <param name="spawnRate">The spawn rate.</param>
		public static void GetSpawnData(TSPlayer player, out double maxSpawns, out double spawnRate)
        {
            maxSpawns = Config.Instance.MaxSpawns;
            spawnRate = Config.Instance.SpawnRate;

            if (Main.hardMode)
            {
                ++maxSpawns;
            }

            // Handle height-specific modifiers.
            var tplayer = player.TPlayer;
            if (tplayer.ZoneOverworldHeight)
            {
                if (Main.eclipse)
                {
                    maxSpawns *= 1.9;
                    spawnRate *= 0.2;
                }
                else if (!Main.dayTime)
                {
                    maxSpawns *= Main.bloodMoon ? 2.34 : 1.3;
                    spawnRate *= Main.bloodMoon ? 0.18 : 0.6;
                }
            }
            else if (tplayer.ZoneDirtLayerHeight)
            {
                maxSpawns *= Main.hardMode ? 1.8 : 1.7;
                spawnRate *= Main.hardMode ? 0.45 : 0.5;
            }
            else if (tplayer.ZoneRockLayerHeight)
            {
                maxSpawns *= 1.9;
                spawnRate *= 0.4;
            }
            else if (tplayer.ZoneUnderworldHeight)
            {
                maxSpawns *= 2.0;
            }

            // Handle biome-specific modifiers.
            if (tplayer.ZoneDungeon)
            {
                maxSpawns *= 1.7;
                spawnRate *= 0.4;
            }
            else if (tplayer.ZoneSandstorm)
            {
                maxSpawns *= Main.hardMode ? 1.5 : 1.2;
                spawnRate *= Main.hardMode ? 0.4 : 0.9;
            }
            else if (tplayer.ZoneUndergroundDesert)
            {
                maxSpawns *= 2.0;
                spawnRate *= Main.hardMode ? 0.2 : 0.3;
            }
            else if (tplayer.ZoneJungle)
            {
                maxSpawns *= 1.5;
                spawnRate *= 0.4;
            }
            else if (tplayer.ZoneCorrupt || tplayer.ZoneCrimson)
            {
                maxSpawns *= 1.3;
                spawnRate *= 0.65;
            }
            else if (tplayer.ZoneMeteor)
            {
                maxSpawns *= 1.1;
                spawnRate *= 0.4;
            }

            var activeNpcs = tplayer.activeNPCs;
            if (activeNpcs < 0.2 * maxSpawns)
            {
                spawnRate *= 0.6;
            }
            else if (activeNpcs < 0.4 * maxSpawns)
            {
                spawnRate *= 0.7;
            }
            else if (activeNpcs < 0.6 * maxSpawns)
            {
                spawnRate *= 0.8;
            }
            else if (activeNpcs < 0.8 * maxSpawns)
            {
                spawnRate *= 0.9;
            }

            // Handle buff-related modifiers.
            if (tplayer.enemySpawns)
            {
                maxSpawns *= 2.0;
                spawnRate *= 0.5;
            }
            if (tplayer.calmed)
            {
                maxSpawns *= 0.7;
                spawnRate *= 1.3;
            }
            if (tplayer.sunflower)
            {
                maxSpawns *= 0.8;
                spawnRate *= 1.2;
            }
            if (tplayer.ZonePeaceCandle)
            {
                maxSpawns *= 0.7;
                spawnRate *= 1.3;
            }
            else if (tplayer.ZoneWaterCandle)
            {
                maxSpawns *= 1.3;
                spawnRate *= 0.7;
            }

            maxSpawns = Math.Min(maxSpawns, 3 * Config.Instance.MaxSpawns);
            spawnRate = Math.Max(spawnRate, 0.1 * Config.Instance.SpawnRate);
        }

        /// <summary>
        ///     Picks a random key from a dictionary using the values as weights.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="dictionary">The dictionary, which must not be <c>null</c>.</param>
        /// <returns>The key.</returns>
        public static TKey PickRandomWeightedKey<TKey>(IDictionary<TKey, int> dictionary)
        {
            var rand = Random.Next(dictionary.Values.Sum());
            var current = 0;
            foreach (var kvp in dictionary)
            {
                var weight = kvp.Value;
                if (current <= rand && rand < current + weight)
                {
                    return kvp.Key;
                }
                current += weight;
            }
            return default(TKey);
        }

        /// <summary>
        ///     Spawns a vanilla or custom NPC at the specified tile coordinates.
        /// </summary>
        /// <param name="npcNameOrType">The NPC name or type.</param>
        /// <param name="tileX">The X tile coordinate.</param>
        /// <param name="tileY">The Y tile coordinate.</param>
        public static void SpawnVanillaOrCustomNpc(string npcNameOrType, int tileX, int tileY)
        {
            if (int.TryParse(npcNameOrType, out var npcType))
            {
                NPC.NewNPC(16 * tileX + 8, 16 * tileY, npcType);
                return;
            }

            var definition = NpcManager.Instance?.FindDefinition(npcNameOrType);
            if (definition != null)
            {
                NpcManager.Instance.SpawnCustomNpc(definition, 16 * tileX + 8, 16 * tileY);
            }
        }
		
		/// <summary>
        ///     Tries to pick a random key from a dictionary using the values as chances.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="dictionary">The dictionary, which must not be <c>null</c>.</param>
        /// <returns>The key, or a default value if nothing was picked.</returns>
        public static TKey TryPickRandomKey<TKey>(IDictionary<TKey, double> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                if (Random.NextDouble() < kvp.Value)
                {
                    return kvp.Key;
                }
            }
            return default(TKey);
        }

        /// <summary>
        ///     Tries to run a spawning algorithm for each player.
        /// </summary>
        /// <param name="action">The spawning algorithm, which must not be <c>null</c>.</param>
        public static void TrySpawnForEachPlayer(Action<TSPlayer, int, int> action)
        {
            foreach (var player in TShock.Players.Where(p => p?.Active == true))
            {
                // TODO: Update spawnRangeX and spawnRangeY based on scope/stuff
                var spawnRangeX = (int)(NPC.sWidth / 16.0 * 0.7);
                var spawnRangeY = (int)(NPC.sHeight / 16.0 * 0.7);
                var minX = Math.Max(0, player.TileX - spawnRangeX);
                var maxX = Math.Min(Main.maxTilesX, player.TileX + spawnRangeX);
                var minY = Math.Max(0, player.TileY - spawnRangeY);
                var maxY = Math.Min(Main.maxTilesY, player.TileY + spawnRangeY);
				
                var succeeded = false;
                var tileX = -1;
                var tileY = -1;
                for (var i = 0; i < 50; ++i)
                {
                    tileX = Random.Next(minX, maxX);
                    tileY = Random.Next(minY, maxY);
                    var tile = Main.tile[tileX, tileY];
                    if (tile.IsSolid() || Main.wallHouse[tile.wall])
                    {
                        continue;
                    }

                    while (++tileY < Main.maxTilesY && !Main.tile[tileX, tileY].IsSolid())
                    {
                    }

                    if (tileY != Main.maxTilesY && CanNpcSpawnAtCoordinates(tileX, tileY, 2, 2))
                    {
                        succeeded = true;
                        break;
                    }
                }

                if(succeeded && !CanPlayersSeeCoordinates(tileX, tileY))
                {
					action(player, tileX, tileY);
                }
            }
        }

		/// <summary>
		/// Checks that a given tile is suitable to spawn an NPC.
		/// </summary>
		/// <param name="tileX">Tile x.</param>
		/// <param name="tileY">Tile y.</param>
		/// <param name="extraSpawnSpaceX">Optional extra space.</param>
		/// <param name="extraSpawnSpaceY">Optional extra space.</param>
		/// <returns>True if the space is big enough.</returns>
        private static bool CanNpcSpawnAtCoordinates(int tileX, int tileY, int extraSpawnSpaceX = 0, int extraSpawnSpaceY = 0)
        {
			//try to minimize invasion npcs spawning in tight areas by adding extra space...
			var spawnSpaceX = NPC.spawnSpaceX + extraSpawnSpaceX;
			var spawnSpaceY = NPC.spawnSpaceY + extraSpawnSpaceY;

            var minCheckX = Math.Max(0, tileX - spawnSpaceX / 2);
            var maxCheckX = Math.Min(Main.maxTilesX, tileX + spawnSpaceX / 2);
            var minCheckY = Math.Max(0, tileY - spawnSpaceY);
            for (var x = minCheckX; x < maxCheckX; ++x)
            {
                for (var y = minCheckY; y < tileY; ++y)
                {
                    var tile2 = Main.tile[x, y];

					//if(tile2.liquid>0 && tile2.liquidType() == 0)
					//{
					//	//Debug.Print($"Can spawn npc at {tileX},{tileY}");
					//	//return true;
					//	continue;
					//}

                    if (tile2.IsSolid() || tile2.lava())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CanPlayersSeeCoordinates(int tileX, int tileY)
        {
            var spawnRectangle = new Rectangle(16 * tileX, 16 * tileY, 16, 16);
            var safeRangeX = (int)(NPC.sWidth / 16.0 * 0.52);
            var safeRangeY = (int)(NPC.sHeight / 16.0 * 0.52);
            foreach (var player2 in TShock.Players.Where(p => p?.Active == true))
            {
                var playerCenter = player2.TPlayer.Center;
                var playerSafeRectangle = new Rectangle((int)(playerCenter.X - NPC.sWidth / 2.0 - safeRangeX),
                    (int)(playerCenter.Y - NPC.sHeight / 2.0 - safeRangeY),
                    NPC.sWidth + 2 * safeRangeX, NPC.sHeight + 2 * safeRangeY);
                if (spawnRectangle.Intersects(playerSafeRectangle))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsSolid(this ITile tile) =>
            tile.active() && tile.type < Main.maxTileSets && Main.tileSolid[tile.type];
    }
}
