using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	public class RewardDistributor //since we can't use distributor or evaluator atm, its a currently in-use type.
	{
		IRewardModifier defaultRewardEvaluator;
		public Dictionary<string, RewardModifierMap> CurrencyRewardModifiers { get; private set; }
		ConcurrentQueue<RewardSource> rewardSourceQueue;

		public RewardDistributor()
		{
			defaultRewardEvaluator = new DefaultRewardModifier();
			CurrencyRewardModifiers = new Dictionary<string, RewardModifierMap>();

			rewardSourceQueue = new ConcurrentQueue<RewardSource>();
		}

		public void EnqueueRewardSource(RewardSource rewardSource)
		{
			rewardSourceQueue.Enqueue(rewardSource);
		}

		public bool TryDequeueRewardSource(out RewardSource rewardSource)
		{
			return rewardSourceQueue.TryDequeue(out rewardSource);
		}

		public void OnGameUpdate()
		{
			const int maxIterations = 64;
			var i = 0;

			while(TryDequeueRewardSource(out var rewardSource) && i++ < maxIterations)
			{
				evaluate(rewardSource);
			}
		}

		private void evaluate(RewardSource rewardSource)
		{
			var bank = BankingPlugin.Instance.Bank;
			//var playerAccountMap = bank[rewardSource.PlayerName];
			var multiPlayerRewardSource = rewardSource as MultipleRewardSource;

			rewardSource.OnPreEvaluate();

			foreach( var currency in BankingPlugin.Instance.Bank.CurrencyManager )
			{
				if( currency.GainBy.Contains(rewardSource.RewardReason) ||
					rewardSource.RewardReason == RewardReason.Death ||
					rewardSource.RewardReason == RewardReason.DeathPvP ||
					rewardSource.RewardReason == RewardReason.Undefined )
				{
					//allow external code a chance to modify the npc's value ( ie, leveling's NpcNameToExp tables... )
					var evaluator = GetRewardModifier(currency.InternalName, rewardSource.RewardReason);

					if(multiPlayerRewardSource!=null)
					{
						foreach( var pair in multiPlayerRewardSource.OnEvaluateMultiple(currency,evaluator) ) 
						{
							var playerName = pair.Item1;
							var rewardValue = pair.Item2;
							
							if( updateBankAccount(playerName,currency.InternalName,ref rewardValue))
							{
								trySendCombatText(playerName, currency, ref rewardValue);
							}
						}
					}
					else
					{
						var rewardValue = rewardSource.OnEvaluate(currency,evaluator);

						if( updateBankAccount(rewardSource.PlayerName, currency.InternalName, ref rewardValue) )
						{
							trySendCombatText(rewardSource.PlayerName, currency, ref rewardValue);
						}
					}
				}
			}
		}

		private bool updateBankAccount(string playerName, string accountName, ref decimal value)
		{
			if( value == 0m )
				return false;

			var account = BankingPlugin.Instance.Bank.GetBankAccount(playerName, accountName);
			Debug.Assert(account != null, $"Unable to find BankAccount '{accountName}' for player '{playerName}'.");

			if( value > 0m )
			{
				account.Deposit(value);
				return true;
			}
			else
			{
				return account.TryWithdraw(value);
			}
		}

		/// <summary>
		/// Attempts to send the rewarded value as a combat text. This combat text may get concatenated with other rewards, or ignored altogether.
		/// </summary>
		/// <param name="playerName"></param>
		/// <param name="currency"></param>
		/// <param name="value"></param>
		private void trySendCombatText(string playerName, CurrencyDefinition currency, ref decimal value)
		{
			if( !currency.SendCombatText )
				return;

#if !DEBUG
			//dont send text for fractional values.
			if( Math.Abs(value) < 1.0m )
				return;
#endif

			var player = TShockAPI.Utils.Instance.FindPlayer(playerName).FirstOrDefault();

			if( player != null )
			{
				var notification = new PlayerRewardNotification()
				{
					Player = player,
					Value = value,
					CurrencyDefinition = currency
				};

				BankingPlugin.Instance.PlayerCurrencyNotificationDistributor.Add(notification);
			}
		}

		public IRewardModifier GetRewardModifier(string currencyType, RewardReason reason)
		{
			if( CurrencyRewardModifiers.TryGetValue(currencyType, out var rewardEvaluatorMap) )
			{
				if( rewardEvaluatorMap.TryGetValue(reason, out var evaluator) )
					return evaluator;
			}

			//otherwise, get a default evaluator
			return defaultRewardEvaluator;
		}

		public void SetRewardModifier(string currencyType, RewardReason reason, IRewardModifier evaluator)
		{
			if( !CurrencyRewardModifiers.TryGetValue(currencyType, out var rewardEvaluatorMap) )
			{
				rewardEvaluatorMap = new RewardModifierMap();
				CurrencyRewardModifiers.Add(currencyType, rewardEvaluatorMap);
			}

			rewardEvaluatorMap[reason] = evaluator;
		}

		public void ClearRewardModifiers()
		{
			CurrencyRewardModifiers.Clear();
		}

		private class DefaultRewardModifier : IRewardModifier
		{
			public decimal ModifyBaseRewardValue(RewardReason reason, string playerName, string currencyType, string itemName, decimal rewardValue)
			{
				return rewardValue;
			}
		}
	}
}
