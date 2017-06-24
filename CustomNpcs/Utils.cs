using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua.Exceptions;
using Terraria;
using TShockAPI;

namespace CustomNpcs
{
    internal static class Utils
    {
        private static readonly object LuaLock = new object();
        private static readonly Random Random = new Random();

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

        public static void TryExecuteLua([NotNull] Action action)
        {
            try
            {
                lock (LuaLock)
                {
                    action();
                }
            }
            catch (LuaException e)
            {
                TShock.Log.ConsoleError("A Lua error occurred:");
                TShock.Log.ConsoleError(e.ToString());
                if (e.InnerException != null)
                {
                    TShock.Log.ConsoleError(e.InnerException.ToString());
                }
            }
        }

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

        public static void TrySpawnForEachPlayer(Action<TSPlayer, int, int> action)
        {
            foreach (var player in TShock.Players.Where(p => p?.Active == true))
            {
                var succeeded = false;
                var tileX = -1;
                var tileY = -1;
                var spawnRangeX = (int)(NPC.sWidth / 16.0 * 0.7);
                var spawnRangeY = (int)(NPC.sHeight / 16.0 * 0.7);
                var minX = Math.Max(0, player.TileX - spawnRangeX);
                var maxX = Math.Min(Main.maxTilesX, player.TileX + spawnRangeX);
                var minY = Math.Max(0, player.TileY - spawnRangeY);
                var maxY = Math.Min(Main.maxTilesY, player.TileY + spawnRangeY);
                for (var i = 0; i < 50 && !succeeded; ++i)
                {
                    tileX = Random.Next(minX, maxX);
                    tileY = Random.Next(minY, maxY);
                    var tile = Main.tile[tileX, tileY];
                    if (tile.nactive() && Main.tileSolid[tile.type] || Main.wallHouse[tile.wall])
                    {
                        continue;
                    }

                    // Search downwards until we hit the ground.
                    while (++tileY < Main.maxTilesY)
                    {
                        var tile2 = Main.tile[tileX, tileY];
                        if (tile2.nactive() && Main.tileSolid[tile2.type])
                        {
                            succeeded = true;
                            break;
                        }
                    }

                    // Make sure the NPC has space to spawn.
                    if (succeeded)
                    {
                        var minCheckX = Math.Max(0, tileX - NPC.spawnSpaceX / 2);
                        var maxCheckX = Math.Min(Main.maxTilesX, tileX + NPC.spawnSpaceX / 2);
                        var minCheckY = Math.Max(0, tileY - NPC.spawnSpaceY);
                        for (var x2 = minCheckX; x2 < maxCheckX && succeeded; ++x2)
                        {
                            for (var y2 = minCheckY; y2 < tileY; ++y2)
                            {
                                // Don't allow the NPC to spawn within tiles.
                                var tile2 = Main.tile[x2, y2];
                                if (tile2.nactive() && Main.tileSolid[tile2.type] || tile2.lava())
                                {
                                    succeeded = false;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Don't allow the NPC to spawn within sight of any players.
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
                        succeeded = false;
                    }
                }

                if (succeeded)
                {
                    action(player, tileX, tileY);
                }
            }
        }
    }
}
