using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua.Exceptions;
using OTAPI.Tile;
using Terraria;
using TShockAPI;

namespace CustomNpcs
{
    internal static class Utils
    {
        private static readonly object LuaLock = new object();
        private static readonly Random Random = new Random();

        public static NPC NpcOrRealNpc(NPC npc)
        {
            var realId = npc.realLife;
            return realId < 0 ? npc : Main.npc[realId];
        }

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
            catch (LuaException ex)
            {
                TShock.Log.ConsoleError("[CustomNpcs] A Lua error occurred:");
                TShock.Log.ConsoleError(ex.ToString());
                if (ex.InnerException != null)
                {
                    TShock.Log.ConsoleError(ex.InnerException.ToString());
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
                // TODO: Update spawnRangeX and spawnRangeY based on 
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

                    if (tileY != Main.maxTilesY && CanNpcSpawnAtCoordinates(tileX, tileY))
                    {
                        succeeded = true;
                        break;
                    }
                }

                if (succeeded && !CanPlayersSeeCoordinates(tileX, tileY))
                {
                    action(player, tileX, tileY);
                }
            }
        }

        private static bool CanNpcSpawnAtCoordinates(int tileX, int tileY)
        {
            var minCheckX = Math.Max(0, tileX - NPC.spawnSpaceX / 2);
            var maxCheckX = Math.Min(Main.maxTilesX, tileX + NPC.spawnSpaceX / 2);
            var minCheckY = Math.Max(0, tileY - NPC.spawnSpaceY);
            for (var x = minCheckX; x < maxCheckX; ++x)
            {
                for (var y = minCheckY; y < tileY; ++y)
                {
                    var tile2 = Main.tile[x, y];
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

        private static bool IsSolid(this ITile tile) => tile.active() && Main.tileSolid[tile.type];
    }
}
