using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	/// <summary>
	/// Used to queue and concatenate player rewards/losses.
	/// </summary>
	internal class PlayerRewardNotificationDistributor
	{
		ConcurrentQueue<PlayerRewardNotification> incomingNotificationsQueue;//previous version filled this from a network thread(?), but drained on the game thread.
																				//this is no longer the case, and it does both in GameUpate().
																				//so this can probably be changed to a normal Queue<of T>.
		Dictionary<NotificationKey, PlayerRewardNotification> accumulatingNotifications;
		DateTime lastSendTime;
		TimeSpan accumulateDuration;

		int yOffsetTicker;// used to displace the y offset of sent notifications. Its set to a number, and ticked down on each send, eventually reaching 0 and resetting.
		Random xOffsetRandom; // used to randomize the x offset of sent notifications 

		internal PlayerRewardNotificationDistributor()
		{
			const int startingCapacity = 32;

			incomingNotificationsQueue = new ConcurrentQueue<PlayerRewardNotification>();
			accumulatingNotifications = new Dictionary<NotificationKey, PlayerRewardNotification>(startingCapacity);

			DateTime lastSendTime = DateTime.Now;
			accumulateDuration = TimeSpan.FromMilliseconds(650);

			xOffsetRandom = new Random();
		}

		//no need for a Clear() method, since onLoad() always rebuilds everything anew.
		
		internal void Add(PlayerRewardNotification notification)
		{
			incomingNotificationsQueue.Enqueue(notification);
		}

		//accumulate/concatenate incoming entries into a dictionary, keyed by entry( player + global + currency ).
		//These will then get dispatched in a later step.
		private void accumulateNotifications()
		{
			const int maxIterations = 64;//keep the looping bounded
			int counter = 0;
			var now = DateTime.Now;
			var volleySpan = TimeSpan.FromMilliseconds(1000);//if another matching notification is accumulated within this time frame,
															//we "extend" the life of the accumulated notification, in case more
															// matching notifications are coming. This volley effect in essence,
															// keeps the accumulation rolling.

			while( incomingNotificationsQueue.Count > 0 && counter++ < maxIterations )
			{
				if( incomingNotificationsQueue.TryDequeue(out var notification) )
				{
					var notificationKey = new NotificationKey(notification);

					if( !accumulatingNotifications.TryGetValue(notificationKey, out var accumulatingNotification) )
					{
						accumulatingNotifications.Add(notificationKey, notification);
						notification.TimeStamp = DateTime.Now;
					}
					else
					{
						//should we extend the lifetime of this notification?
						if( now - accumulatingNotification.TimeStamp > volleySpan )
						{
							accumulatingNotification.TimeStamp = now + volleySpan;
						}

						accumulatingNotification.Accumulate(notification);
					}
				}
			}
		}

		private void dispatchNotifications(int dispatchIntervalMS)
		{
			var dispatchInterval = new TimeSpan(0, 0, 0, 0, dispatchIntervalMS);
			var now = DateTime.Now;
			
			if( ( now - lastSendTime ) >= dispatchInterval )
			{
				var key = new NotificationKey();
				PlayerRewardNotification notification = null;
				var removeNotification = false;
				
				foreach( var kvp in accumulatingNotifications )
				{
					key = kvp.Key;
					notification = kvp.Value;

					if( ( now - notification.TimeStamp ) >= accumulateDuration )
					{
						lastSendTime = now;
						getCombatTextOffsets(out var xOffset, out var yOffset);
						notification.Send(xOffset, yOffset);
						removeNotification = true;
						break;
					}
				}

				//remove
				if( removeNotification )
				{
					accumulatingNotifications.Remove(key);
				}
			}
		}

		//used to minimize combat text's obscuring each other on the same player
		private void getCombatTextOffsets(out float xOffset, out float yOffset)
		{
			yOffsetTicker--;

			if( yOffsetTicker < 0 )
				yOffsetTicker = 3;

			xOffset = xOffsetRandom.Next(-1,1) * xOffsetRandom.Next(1,2) * 16;
			yOffset = -(yOffsetTicker * 16);
		}

		public void Send(int delayMS = 125)
		{
			accumulateNotifications();
			dispatchNotifications(delayMS);
		}

		/// <summary>
		/// 3 component key created from PlayerCurrencyNotifications.
		/// </summary>
		private struct NotificationKey : IEquatable<NotificationKey>
		{
			internal readonly int PlayerHash;
			internal readonly int IsGlobalHash;
			internal readonly int CurrencyDefinitionHash;

			internal NotificationKey(PlayerRewardNotification notification)
			{
				PlayerHash = notification.Player.GetHashCode();
				IsGlobalHash = notification.IsGlobal.GetHashCode();
				CurrencyDefinitionHash = notification.CurrencyDefinition.GetHashCode();
			}

			public bool Equals(NotificationKey other)
			{
				return PlayerHash == other.PlayerHash &&
						IsGlobalHash == other.IsGlobalHash &&
						CurrencyDefinitionHash == other.CurrencyDefinitionHash;
			}

			public override bool Equals(object obj)
			{
				if( obj is NotificationKey )
					return Equals((NotificationKey)obj);

				return false;
			}

			public override int GetHashCode()
			{
				return PlayerHash ^ IsGlobalHash ^ CurrencyDefinitionHash;
			}
		}
	}
}
