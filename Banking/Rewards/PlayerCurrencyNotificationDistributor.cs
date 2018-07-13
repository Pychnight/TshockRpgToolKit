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
	internal class PlayerCurrencyNotificationDistributor
	{
		ConcurrentQueue<PlayerCurrencyNotification> incomingNotificationsQueue;
		Dictionary<NotificationKey, PlayerCurrencyNotification> accumulatingNotifications;
		DateTime lastSendTime;
		TimeSpan accumulateDuration; 

		internal PlayerCurrencyNotificationDistributor()
		{
			const int startingCapacity = 32;

			incomingNotificationsQueue = new ConcurrentQueue<PlayerCurrencyNotification>();
			accumulatingNotifications = new Dictionary<NotificationKey, PlayerCurrencyNotification>(startingCapacity);

			DateTime lastSendTime = DateTime.Now;
			accumulateDuration = TimeSpan.FromMilliseconds(650);
		}

		internal void Add(PlayerCurrencyNotification notification)
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
			var volleySpan = TimeSpan.FromMilliseconds(900);//if another matching notification is accumulated within this time frame,
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
				PlayerCurrencyNotification notification = null;
				var removeNotification = false;
				
				foreach( var kvp in accumulatingNotifications )
				{
					key = kvp.Key;
					notification = kvp.Value;

					if( ( now - notification.TimeStamp ) >= accumulateDuration )
					{
						lastSendTime = now;
						notification.Send();
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

		public void Send(int delayMS = 200)
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

			internal NotificationKey(PlayerCurrencyNotification notification)
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
