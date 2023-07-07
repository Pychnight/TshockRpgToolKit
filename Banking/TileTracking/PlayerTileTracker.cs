using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Terraria;

namespace Banking.TileTracking
{
	/// <summary>
	/// Tracks tile placement or tile destruction, on a per-player basis.
	/// </summary>
	/// <remarks>For saving of data, you must call OnPlayerLeave() when players have left the game.</remarks>
	public class PlayerTileTracker
	{
		public string DataDirectoryPath { get; private set; }
		string worldDataDirectoryPath;

		Dictionary<string, TileAccessMap> playerTileAccess;
		//Dictionary<string, DateTime> playerLastSeen;
		//public TimeSpan CacheDataDuration { get; set; } = new TimeSpan(3, 0, 0, 0, 0);

		public PlayerTileTracker(string dataDirectoryPath)
		{
			playerTileAccess = new Dictionary<string, TileAccessMap>();
			//playerLastSeen = new Dictionary<string, DateTime>();

			worldDataDirectoryPath = Path.Combine(dataDirectoryPath, $"tile-access-{Main.worldID}");
			Directory.CreateDirectory(worldDataDirectoryPath);
		}

		/// <summary>
		/// Gets the flag(status) of a tile relative to a player.  
		/// </summary>
		/// <param name="playerName">Player name.</param>
		/// <param name="column">X coord.</param>
		/// <param name="row">Y coord.</param>
		/// <returns>Flag.</returns>
		public bool HasModifiedTile(string playerName, int column, int row)
		{
			var accessMap = getOrCreateTileAccessMap(playerName);
			return accessMap[column, row];
		}

		/// <summary>
		/// Sets a tile's flag(status) for a player. 
		/// </summary>
		/// <param name="playerName">Name of player.</param>
		/// <param name="column">X coord.</param>
		/// <param name="row">Y coord.</param>
		/// <param name="value">Set or unset the flag. Default is true.</param>
		public void ModifyTile(string playerName, int column, int row, bool value = true)
		{
			var accessMap = getOrCreateTileAccessMap(playerName);
			accessMap[column, row] = value;
		}

		private TileAccessMap getOrCreateTileAccessMap(string playerName)
		{
			if (!playerTileAccess.TryGetValue(playerName, out var result))
			{
				var filePath = getPathForPlayerName(playerName);

				if (File.Exists(filePath))
				{
					try
					{
						result = TileAccessMap.Load(filePath);
					}
					catch (Exception ex)
					{
						Debug.Print("Failed to load TileAccessMap {filePath}");
						Debug.Print(ex.ToString());
						result = null;
					}
				}

				if (result == null)
					result = new TileAccessMap(Main.maxTilesX, Main.maxTilesY);

				playerTileAccess.Add(playerName, result);
				//playerLastSeen.Add(playerName, DateTime.Now);
			}

			return result;
		}

		private string getPathForPlayerName(string playerName)
		{
			var result = Path.Combine(worldDataDirectoryPath, $"{playerName}.bin");
			return result;
		}

		/// <summary>
		/// Removes in-memory tile data, and attempts to save to disk.
		/// </summary>
		/// <param name="playerName">Player name.</param>
		public void OnPlayerLeave(string playerName)
		{
			if (playerTileAccess.TryGetValue(playerName, out var tileAccessMap))
			{
				//we only get here, if a previous accessMap was created for the player.
				try
				{
					var filePath = getPathForPlayerName(playerName);
					tileAccessMap.Save(filePath);
				}
				catch (Exception ex)
				{
					Debug.Print(ex.ToString());
				}

				playerTileAccess.Remove(playerName);
			}
		}
	}
}
