using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace Corruption
{
	public static class AreaFunctions
	{
		/// <summary>
		///     Gets the region with the specified name.
		/// </summary>
		/// <param name="name">The name, which must not be <c>null</c>.</param>
		/// <returns>The region, or <c>null</c> if it does not exist.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="KeyNotFoundException"><paramref name="name" /> is an invalid region name.</exception>
		public static Region GetRegion(string name)
		{
			if( name == null )
				throw new ArgumentNullException(nameof(name));
			
			var result = TShock.Regions.GetRegionByName(name);

			if( result == null )
				throw new KeyNotFoundException($"Invalid Region '{name}'.");
			else
				return result;
		}

		// Utility public static bool for spawning when the player is around the region.
		public static bool AroundRegion(float x, float y, string name)
		{
			var region = GetRegion(name);
			return region != null && region.InArea((int)x, (int)y);
		}
		
		// Utility public static bool for spawning at the cavern level.
		public static bool AtCavernLevel(TSPlayer player)
		{
			return player.TPlayer.ZoneRockLayerHeight;
		}

		// Utility public static bool for spawning at the sky level.
		public static bool AtSkyLevel(TSPlayer player)
		{
			return player.TPlayer.ZoneSkyHeight;
		}

		// Utility public static bool for spawning at the surface level.
		public static bool AtSurfaceLevel(TSPlayer player)
		{
			return player.TPlayer.ZoneOverworldHeight;
		}

		// Utility public static bool for spawning at the underground level.
		public static bool AtUndergroundLevel(TSPlayer player)
		{
			return player.TPlayer.ZoneDirtLayerHeight;
		}

		// Utility public static bool for spawning at the underworld level.
		public static bool AtUnderworldLevel(TSPlayer player)
		{
			return player.TPlayer.ZoneUnderworldHeight;
		}

		// Utility public static bool for spawning in the specified region when the player is inside the region.
		public static bool InAndAroundRegion(TSPlayer player, float x, float y, string name)
		{
			return InRegion(player, name) && AroundRegion((int)x, (int)y, name);
		}

		// Utility for determining if the given tile is within a 10 tile radius of the spawn point.
		public static bool InSpawn(int tileX, int tileY)
		{
			var spx = Main.spawnTileX;
			var spy = Main.spawnTileY;

			if( tileX < spx - 10 )
				return false;

			if( tileX > spx + 10 )
				return false;

			if( tileY < spy - 10 )
				return false;

			if( tileY > spy + 10 )
				return false;

			return true;
		}

		// Utility public static bool for spawning in a beach biome.
		public static bool InBeach(TSPlayer player)
		{
			return player.TPlayer.ZoneBeach;
		}

		// Utility public static bool for spawning in a corruption biome.
		public static bool InCorruption(TSPlayer player)
		{
			return player.TPlayer.ZoneCorrupt;
		}

		// Utility public static bool for spawning in a crimson biome.
		public static bool InCrimson(TSPlayer player)
		{
			return player.TPlayer.ZoneCrimson;
		}

		// Utility public static bool for spawning in a desert biome.
		public static bool InDesert(TSPlayer player)
		{
			return player.TPlayer.ZoneDesert;
		}

		// Utility public static bool for spawning in the dungeon.
		public static bool InDungeon(TSPlayer player)
		{
			return player.TPlayer.ZoneDungeon;
		}

		// Utility public static bool for spawning in a forest biome (not a special biome).
		public static bool InForest(TSPlayer player)
		{
			var tplayer = player.TPlayer;
			return ! tplayer.ZoneBeach && ! tplayer.ZoneCorrupt && ! tplayer.ZoneCrimson && ! tplayer.ZoneDesert &&
				   ! tplayer.ZoneDungeon && ! tplayer.ZoneGlowshroom && ! tplayer.ZoneHoly && ! tplayer.ZoneSnow &&
				   ! tplayer.ZoneJungle && ! tplayer.ZoneMeteor && ! tplayer.ZoneOldOneArmy &&
				   ! tplayer.ZoneTowerSolar && ! tplayer.ZoneTowerVortex && ! tplayer.ZoneTowerNebula &&
				   ! tplayer.ZoneTowerStardust;
		}

		// Utility public static bool for spawning in a glowing mushroom biome.
		public static bool InGlowshroom(TSPlayer player)
		{
			return player.TPlayer.ZoneGlowshroom;
		}

		// Utility public static bool for spawning in a hallow biome.
		public static bool InHallow(TSPlayer player)
		{
			return player.TPlayer.ZoneHoly;
		}

		// Utility public static bool for spawning in an ice biome.
		public static bool InIce(TSPlayer player)
		{
			return player.TPlayer.ZoneSnow;
		}

		// Utility public static bool for spawning when the player is inside the specified region.
		public static bool InRegion(TSPlayer player, string name)
		{
			var region = GetRegion(name);
			return region != null && region.InArea(player.TileX, player.TileY);
		}

		// Utility public static bool for spawning in a jungle biome.
		public static bool InJungle(TSPlayer player)
		{
			return player.TPlayer.ZoneJungle;
		}

		// Utility public static bool for spawning in a meteor.
		public static bool InMeteor(TSPlayer player)
		{
			return player.TPlayer.ZoneMeteor;
		}

		// Utility public static bool for spawning in water.
		public static bool InWater(float x, float y)
		{
			var tile = TileFunctions.GetTile((int)x, (int)y);
			return tile.liquid > 0 && tile.liquidType() == 0;
		}
	}
}
