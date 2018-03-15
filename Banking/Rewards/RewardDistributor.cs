using Banking.Configuration;
using Microsoft.Xna.Framework;
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
	/// Distributes monetary rewards to players.
	/// </summary>
	public class RewardDistributor
	{
		IRewardEvaluator defaultRewardEvaluator;
		public Dictionary<string, RewardEvaluatorMap> CurrencyRewardEvaluators { get; private set; }

		public RewardDistributor()
		{
			defaultRewardEvaluator = new DefaultRewardEvaluator();
			CurrencyRewardEvaluators = new Dictionary<string, RewardEvaluatorMap>();
		}
		
		public void TryAddReward(string playerName, RewardReason gainedBy, string itemName, float defaultValue = 1.0f, bool npcSpawnedFromStatue = false )
		{
			var bank = BankingPlugin.Instance.Bank;
			var playerAccountMap = bank[playerName];

			foreach(var currency in bank.CurrencyManager)
			{
				if(currency==null)
				{
					Debug.Assert(currency!=null,"Currency should never be null!");
					continue;
				}

				if( !currency.GainBy.Contains(gainedBy) )
					continue;

				if(npcSpawnedFromStatue&&!currency.EnableStatueNpcRewards)
				{
					continue;
				}

				//var rewardAccount = playerAccountMap.GetAccountForCurrencyReward(currency.InternalName);
				var rewardAccount = playerAccountMap.TryGetBankAccount(currency.InternalName);

				if( rewardAccount == null )
				{
					Debug.Print($"Transaction skipped. Couldn't find {currency.InternalName} account for {playerName}.");
					continue;
				}

				var evaluator = GetRewardEvaluator(currency.InternalName, gainedBy);
				
				switch(gainedBy)
				{
					case RewardReason.Killing:
					case RewardReason.Mining:
					case RewardReason.Placing:
						decimal value =	evaluator.GetRewardValue(gainedBy,playerName, currency.InternalName, itemName, (decimal)defaultValue);

						value *= (decimal)currency.Multiplier;

						if( value == 0.0m )
							continue;

						if( value > 0.0m )
							rewardAccount.Deposit(value);
						else
							rewardAccount.TryWithdraw(value);

						trySendCombatText(playerName, currency, ref value);
						break;

					case RewardReason.Death:
						//decimal value = evaluator.GetRewardValue(playerName, currency.InternalName, itemName, (decimal)defaultValue);//<---not sure how or if to slot this in.
						decimal loss = Math.Round(Math.Max((decimal)currency.DeathPenaltyPvPMultiplier * rewardAccount.Balance, (decimal)currency.DeathPenaltyMinimum));

						if( loss == 0.0m )
							continue;

						if( string.IsNullOrWhiteSpace(itemName) )//itemName will hold other players name, if available.
						{
							if( rewardAccount.TryWithdraw(loss, false) )
								trySendCombatText(playerName, currency, ref loss);
						}
						else
						{
							var other = bank.GetBankAccount(itemName, currency.InternalName);

							if( other == null )
							{
								Debug.Print($"Unable to find player {itemName}'s BankAccount for DeathPvP.");
								continue;
							}

							if( rewardAccount.TryTransferTo(other, loss, false) )
							{
								loss = -loss;//make negative
								trySendCombatText(playerName, currency, ref loss);

								loss = -loss;//make positive
								trySendCombatText(itemName, currency, ref loss);
							}
						}
						break;

					case RewardReason.DeathPvP:
						var factor = 1.0m; // ( session.Class.DeathPenaltyMultiplierOverride ?? 1.0 );

						decimal pvpLoss = Math.Round(Math.Max((decimal)currency.DeathPenaltyMultiplier * factor * rewardAccount.Balance,
																	(decimal)currency.DeathPenaltyMinimum));
						if( pvpLoss == 0.0m )
							continue;

						if( rewardAccount.TryWithdraw(pvpLoss, false) )
						{
							pvpLoss = -pvpLoss;//make negative
							trySendCombatText(playerName, currency, ref pvpLoss);
						}
						break;
				}
			}
		}

		private void trySendCombatText(string playerName, CurrencyDefinition currency, ref decimal value)
		{
			if( !currency.SendCombatText )
				return;

			var player = TShockAPI.Utils.Instance.FindPlayer(playerName).FirstOrDefault();

			if( player != null )
			{
				var color = Color.White;
				var money = currency.GetCurrencyConverter().ToStringAndColor(value, ref color);
				var combatText = $"{money}";

				BankingPlugin.Instance.CombatTextDistributor.AddCombatText(combatText, player, color);
			}
		}
		
		public IRewardEvaluator GetRewardEvaluator(string currencyType, RewardReason reason)
		{
			if( CurrencyRewardEvaluators.TryGetValue(currencyType, out var rewardEvaluatorMap) )
			{
				if( rewardEvaluatorMap.TryGetValue(reason, out var evaluator) )
					return evaluator;
			}

			//otherwise, get a default evaluator
			return defaultRewardEvaluator;
		}
		
		public void SetRewardEvaluator(string currencyType, RewardReason reason, IRewardEvaluator evaluator)
		{
			if( !CurrencyRewardEvaluators.TryGetValue(currencyType, out var rewardEvaluatorMap) )
			{
				rewardEvaluatorMap = new RewardEvaluatorMap();
				CurrencyRewardEvaluators.Add(currencyType, rewardEvaluatorMap);
			}

			rewardEvaluatorMap[reason] = evaluator;
		}

		public void ClearRewardEvaluators()
		{
			CurrencyRewardEvaluators.Clear();
		}

		//public void AddNpcKill(string playerName, float damage, float npcValue)
		//{
		//	if( npcValue < 1f )
		//		return;

		//	if( dustCurrency != null )
		//	{
		//		var value = npcValue;
		//		var currencyType = "Dust";
		//		var account = BankingPlugin.Instance.BankAccountManager.GetBankAccount(playerName, currencyType);
		//		Debug.Assert(account != null, $"Couldn't find {currencyType} account for {playerName}.");

		//		account?.Deposit((decimal)value);

		//		if(dustCurrency.SendCombatText)
		//		{
		//			var player = TShockAPI.Utils.Instance.FindPlayer(playerName).FirstOrDefault();

		//			if(player!=null)
		//			{
		//				var txt = $"+{value}";

		//				//BankingPlugin.Instance.CombatTextDistributor.AddCombatText(txt, player, color);
		//			}
		//		}
		//	}
		//}

		//public void AddBlockMined(string playerName, float blockValue)
		//{

		//}

		//public void AddTimePlayed(string playerName, float value)
		//{

		//}

		public void TryAddVoteReward(string playerName)
		{
			var bank = BankingPlugin.Instance.Bank;
			var playerAccountMap = bank[playerName];
			var config = Config.Instance.Voting;

			foreach( var kvp in config.Rewards )
			{
				var currencyName = kvp.Key;
				var currency = bank.CurrencyManager[currencyName];

				if( currency == null )
				{
					Debug.Assert(currency != null, "Currency should never be null!");
					continue;
				}
								
				var rewardAccount = playerAccountMap.TryGetBankAccount(currency.InternalName);

				if( rewardAccount == null )
				{
					Debug.Print($"Transaction skipped. Couldn't find {currency.InternalName} account for {playerName}.");
					continue;
				}

				if(currency.GetCurrencyConverter().TryParse(kvp.Value, out var value))
				{
					value *= (decimal)currency.Multiplier;
					
					if( value > 0.0m )
					{
						rewardAccount.Deposit(value);
						trySendCombatText(playerName, currency, ref value);
					}
				}
				else
				{
					Debug.Print($"Transaction skipped. Couldn't parse '{value}' as a valid {currency.InternalName} value for {playerName}.");
				}
			}
		}

		private class DefaultRewardEvaluator : IRewardEvaluator
		{
			public decimal GetRewardValue(RewardReason reason, string playerName, string currencyType, string itemName, decimal rewardValue)
			{
				return rewardValue;
			}
		}

		//private class DefaultRewardEvaluator : IRewardEvaluator
		//{
		//	public decimal GetRewardValue(RewardReason reason, string playerName, string currencyType, string itemName, decimal rewardValue)
		//	{
		//		var currency = BankingPlugin.Instance.Bank.CurrencyManager[currencyType];

		//		if( currency?.Rewards.TryGetValue(reason, out var rewardDef) )
		//		{
		//			decimal value;

		//			//if( rewardDef.Ignore.Contains(itemName) )
		//			//	continue;

		//			if( !rewardDef.ParsedValues.TryGetValue(itemName, out value) )
		//				value = rewardValue;

		//		}
		//	}
		//}
	}

#if ENABLE_IREWARD_EXPERIMENT
		ConcurrentQueue<IReward> rewards;
		
		public RewardDistributor()
		{
			rewards = new ConcurrentQueue<IReward>();
		}

		public void Clear()
		{
			var count = rewards.Count;//only clear what was in queue at time of call

			for( var i = 0; i < count; i++ )
				rewards.TryDequeue(out var unused);
		}
		
		public void OnGameUpdate()
		{
			var count = rewards.Count;//only process what was in queue at time of call

			for( var i = 0; i < count; i++ )
			{
				if(rewards.TryDequeue(out var reward))
				{
					reward.Give();
					reward.Notify();
				}
			}
		}

		//this would go in above class...
		public void AddReward(IReward reward)
		{
			rewards.Enqueue(reward);
		}

	//all experimental stuff that's been temporarily shelved. Ideally we could use this in place of string creation/comparison, but for now 
	//this will remain as an interesting idea that needs more time.

	public interface IReward
	{
		string PlayerName { get; set; }
		float Value { get; set; }
		void Give();
		void Notify();
	}

	public class Reward : IReward
	{
		public string PlayerName { get; set; }
		public float Value { get; set; }
		public string CombatText { get; set; }

		public virtual void Give()
		{
			Debug.Print($"Giving player a reward of {Value}.");
		}

		public virtual void Notify()
		{
			Debug.Print("This is a basic reward notification.");
		}
	}

	public class CurrencyReward : Reward
	{
		public string CurrencyType { get; set; }
		
		public override void Give()
		{
			var account = BankingPlugin.Instance.BankAccountManager.GetBankAccount(PlayerName, CurrencyType);
			Debug.Assert(account != null, $"Couldn't find {CurrencyType} account for {PlayerName}.");

			account?.Deposit((decimal)Value);
		}

		public override void Notify()
		{
			//this should use the currency's config to determine if we even send anything...
			if(!string.IsNullOrWhiteSpace(CombatText))
			{
				var currencyMgr = BankingPlugin.Instance.BankAccountManager.CurrencyManager;
				var currency = currencyMgr != null ? currencyMgr[CurrencyType] : null;

				Debug.Assert(currency != null, $"Couldn't find {CurrencyType} for {PlayerName}.");

				var player = TShockAPI.Utils.Instance.FindPlayer(PlayerName).FirstOrDefault();

				if( player != null )
				{
					//currency.SendCombatText;

					//var color = CurrencyType == "Exp" ? Color.OrangeRed : Color.Gold;//this should use the currency's config for colors.
					//BankingPlugin.Instance.CombatTextDistributor.AddCombatText(CombatText, player, color);
				}
			}
		}
	}

#endif
}
