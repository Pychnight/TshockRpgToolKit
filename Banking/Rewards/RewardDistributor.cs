using Corruption.PluginSupport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking.Rewards
{
	/// <summary>
	/// Consumes and queues Rewards, for deferred evaluation.
	/// </summary>
	public class RewardDistributor
	{
		ConcurrentQueue<Reward> rewardSourceQueue;

		public RewardDistributor()
		{
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
					if(multiPlayerRewardSource!=null)
					{
						foreach( var pair in multiPlayerRewardSource.OnEvaluateMultiple(currency) ) 
						{
							var playerName = pair.Item1;
							var rewardValue = pair.Item2;

							PostEvaluateReward(reward, ref rewardValue, currency, playerName);
						}
					}
					else
					{
						var rewardValue = reward.OnEvaluate(currency);

						PostEvaluateReward(reward, ref rewardValue, currency, reward.PlayerName);
					}
				}
			}
		}

		/// <summary>
		/// Actions taken after a reward has been evaluted on a player.
		/// </summary>
		private void PostEvaluateReward(Reward reward, ref decimal rewardValue, CurrencyDefinition currency, string playerName)
		{
			currency.FirePreRewardEvents(reward, ref rewardValue, playerName);

			ScriptHookOnPreReward(playerName, reward, currency, ref rewardValue);

			if( TryUpdateBankAccount(playerName, currency.InternalName, ref rewardValue) )
			{
				trySendCombatText(playerName, currency, ref rewardValue);
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
				account.TryWithdraw(value);	//dont use the return status here,
				return true;				//always return true, so a combat text may get sent for negative numbers.
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

<<<<<<< Updated upstream
<<<<<<< Updated upstream
			var player = TSPlayer.FindByNameOrID(playerName).FirstOrDefault();
=======
			var player = TShockAPI.TSPlayer.FindByNameOrID(playerName).FirstOrDefault();
>>>>>>> Stashed changes
=======
			var player = TShockAPI.TSPlayer.FindByNameOrID(playerName).FirstOrDefault();
>>>>>>> Stashed changes

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
	}
}
