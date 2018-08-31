using Banking.Rewards;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking
{
	/// <summary>
	/// Manages per player, PlayingRewards.
	/// </summary>
	public class PlayingRewardTracker
	{
		public const string PlayerDataKey = "Banking_PlayingReward";
		//List<CurrencyDefinition> enabledCurrencies;

		public PlayingRewardTracker()
		{
			//enabledCurrencies = new List<CurrencyDefinition>(FindPlayingCurrencys());
		}

		/// <summary>
		/// Finds all Currency's which GainBy RewardReason.Playing.
		/// </summary>
		/// <returns>IEnumerable of Currency's.</returns>
		public IEnumerable<CurrencyDefinition> FindPlayingCurrencys()
		{
			return BankingPlugin.Instance.Bank.CurrencyManager.Where(c => c.GainBy.Contains(RewardReason.Playing));
		}
		
		/// <summary>
		/// Tracks a TSPlayer.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		public void AddPlayer(TSPlayer player)
		{
			var now = DateTime.Now;
			var enabledCurrencies = FindPlayingCurrencys();
			var dict = new Dictionary<string, DateTime>();// (enabledCurrencies.Count);
			
			foreach( var currency in enabledCurrencies )
				dict[currency.InternalName] = now;
			
			player.SetData(PlayerDataKey, dict);
		}

		public void RemovePlayer(TSPlayer player)
		{
			//nop for now
		}

		public void OnGameUpdate()
		{
			var currencyMgr = BankingPlugin.Instance.Bank.CurrencyManager;
			var rewardDist = BankingPlugin.Instance.RewardDistributor;
			var now = DateTime.Now;
			
			foreach(var player in TShock.Players.Where( p => p?.Active == true ))
			{
				var times = player.GetData<Dictionary<string, DateTime>>(PlayerDataKey);

				if( times == null )//this shouldnt ever happen, but as a safeguard
					continue;

				foreach( var kvp in times.ToArray() )
				{
					var currency = currencyMgr.GetCurrencyByName(kvp.Key);
					if( currency != null )
					{
						if( now - kvp.Value >= currency.PlayingDuration )
						{
							//reset time
							times[currency.InternalName] = now;

							var reward = new PlayingReward(player.Name, player.Group.Name, currency);
							rewardDist.EnqueueReward(reward);
						}
					}
				}
			}
		}
	}
}
