using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Banking
{
	public class PlayerFishingTracker
	{
		const long waitForItemDuration = 250;//in MS

		Stopwatch stopwatch;
		ConcurrentDictionary<int, FishingInfo> playerToFishingInfos;        //tracks in progress fishing operations 
		ConcurrentDictionary<int, FishingInfo> playerToPendingFishingInfos; //fishing infos get moved here when done, waiting for possible item reward for n time.

		/// <summary>
		/// Checks if a projectile type is a fishing bobber.
		/// </summary>
		/// <param name="type">Projectile type</param>
		/// <returns>True if a fishing bobber.</returns>
		public bool IsFishingProjectile(int type) => type >= 360 && type <= 366;

		/// <summary>
		/// Checks if there are any players waiting on a potential fishing item.
		/// </summary>
		/// <returns>True, if there are players waiting.</returns>
		public bool IsWaitingOnFishingItem() => playerToPendingFishingInfos.Count > 0;

		public PlayerFishingTracker()
		{
			playerToFishingInfos = new ConcurrentDictionary<int, FishingInfo>();
			playerToPendingFishingInfos = new ConcurrentDictionary<int, FishingInfo>();
			stopwatch = Stopwatch.StartNew();
		}

		public void TryBeginFishing(int playerId, int projectileId, int projectileType)
		{
			if (!IsFishingProjectile(projectileType))
				return;

			if (!playerToFishingInfos.TryGetValue(playerId, out var fishingInfo))
			{
				Debug.Print($"Player #{playerId} has started fishing.");

				fishingInfo = new FishingInfo()
				{
					ProjectileId = projectileId
					//ProjectileType = projectileType
				};

				playerToFishingInfos.TryAdd(playerId, fishingInfo);
			}
			//else this was just a projectile update, so ignore
		}

		public void TryEndFishing(int playerId, int projectileId)
		{
			if (playerToFishingInfos.TryGetValue(playerId, out var fishingInfo))
			{
				if (fishingInfo.ProjectileId == projectileId)
				{
					playerToFishingInfos.TryRemove(playerId, out var unused);

					fishingInfo.finishTime = stopwatch.ElapsedMilliseconds;

					playerToPendingFishingInfos.TryAdd(playerId, fishingInfo);

					Debug.Print($"Player #{playerId} has stopped fishing.");
				}
			}
		}

		public bool IsItemFromFishing(int playerId)//, int stack, byte prefix, int itemId)
		{
			if (!IsWaitingOnFishingItem())
				return false;

			if (playerToPendingFishingInfos.TryGetValue(playerId, out var fishingInfo))
			{
				//player probably(**hopefully**) got this item through fishing.
				playerToPendingFishingInfos.TryRemove(playerId, out var unused);
				Debug.Print($"Player #{playerId} has caught an item through fishing.");

				return true;
			}

			return false;
		}

		/// <summary>
		/// This method clears out any old internal data, and should be called on each game update.
		/// </summary>
		public void OnGameUpdate()
		{
			var now = stopwatch.ElapsedMilliseconds;

			foreach (var k in playerToPendingFishingInfos.Keys.ToArray())
			{
				if (playerToPendingFishingInfos.TryGetValue(k, out var fishingInfo))
				{
					if (now - fishingInfo.finishTime >= waitForItemDuration)
					{
						playerToPendingFishingInfos.TryRemove(k, out var unused);
					}
				}
			}
		}

		private class FishingInfo
		{
			internal int ProjectileId;
			//internal int ProjectileType;
			internal long finishTime;//time this fishing op completed, in MS. This allows us to check for an incoming item for a certain amount of time. 
		}
	}
}
