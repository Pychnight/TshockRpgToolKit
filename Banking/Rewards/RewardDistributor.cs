using Corruption.PluginSupport;
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
	/// Consumes and queues Rewards, for deferred evaluation.
	/// </summary>
	public class RewardDistributor
	{
		IRewardModifier defaultRewardEvaluator;
		public Dictionary<string, RewardModifierMap> CurrencyRewardModifiers { get; private set; }
		ConcurrentQueue<Reward> rewardSourceQueue;

		public RewardDistributor()
		{
			defaultRewardEvaluator = new DefaultRewardModifier();
			CurrencyRewardModifiers = new Dictionary<string, RewardModifierMap>();

			rewardSourceQueue = new ConcurrentQueue<Reward>();
		}

		public void EnqueueReward(Reward reward)
		{
			rewardSourceQueue.Enqueue(reward);
		}

		public bool TryDequeueReward(out Reward reward)
		{
			return rewardSourceQueue.TryDequeue(out reward);
		}

		public void OnGameUpdate()
		{
			const int maxIterations = 64;
			var i = 0;

			while(TryDequeueReward(out var reward) && i++ < maxIterations)
			{
				evaluate(reward);
			}
		}

		private void evaluate(Reward reward)
		{
			var bank = BankingPlugin.Instance.Bank;
			//var playerAccountMap = bank[rewardSource.PlayerName];
			var multiPlayerRewardSource = reward as MultipleRewardBase;

			reward.OnPreEvaluate();

			foreach( var currency in BankingPlugin.Instance.Bank.CurrencyManager )
			{
				if( currency.GainBy.Contains(reward.RewardReason) ||
					reward.RewardReason == RewardReason.Death ||
					reward.RewardReason == RewardReason.DeathPvP ||
					reward.RewardReason == RewardReason.Undefined )
				{
					//allow external code a chance to modify the npc's value ( ie, leveling's NpcNameToExp tables... )
					var evaluator = GetRewardModifier(currency.InternalName, reward.RewardReason);

					if(multiPlayerRewardSource!=null)
					{
						foreach( var pair in multiPlayerRewardSource.OnEvaluateMultiple(currency,evaluator) ) 
						{
							var playerName = pair.Item1;
							var rewardValue = pair.Item2;

							ScriptHookOnPreReward(playerName, reward, currency, ref rewardValue);
							if( TryUpdateBankAccount(playerName,currency.InternalName,ref rewardValue))
							{
								trySendCombatText(playerName, currency, ref rewardValue);
							}
						}
					}
					else
					{
						var rewardValue = reward.OnEvaluate(currency,evaluator);

						ScriptHookOnPreReward(reward.PlayerName, reward, currency, ref rewardValue);
						if( TryUpdateBankAccount(reward.PlayerName, currency.InternalName, ref rewardValue) )
						{
							trySendCombatText(reward.PlayerName, currency, ref rewardValue);
						}
					}
				}
			}
		}

		private void ScriptHookOnPreReward(string playerName, Reward reward, CurrencyDefinition currency, ref decimal value )
		{
			try
			{
				var scriptHookOnPreReward = BankingPlugin.Instance.Bank.ScriptOnPreReward;
				if(scriptHookOnPreReward!=null)
				{
					var result = scriptHookOnPreReward(playerName, reward, currency,value);
					value = result;
				}
			}
			catch(Exception ex)
			{
				BankingPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
			}
		}

		/// <summary>
		/// Attempts to issue the award to the players BankAccount.
		/// </summary>
		/// <param name="playerName"></param>
		/// <param name="accountName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private bool TryUpdateBankAccount(string playerName, string accountName, ref decimal value)
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

				BankingPlugin.Instance.PlayerRewardNotificationDistributor.Add(notification);
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
