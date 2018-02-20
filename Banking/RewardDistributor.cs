using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking
{
	public class RewardDistributor
	{
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

		public void AddReward(IReward reward)
		{
			rewards.Enqueue(reward);
		}

		public void TryAddReward(string playerName, string gainedBy, float value)
		{
			if( value < 1f )
				return;

			var bankMgr = BankingPlugin.Instance.BankAccountManager;

			foreach(var currency in bankMgr.CurrencyManager)
			{
				if(currency==null)
				{
					Debug.Assert(currency!=null,"Currency should never be null!");
					continue;
				}

				if( currency.GainBy == gainedBy )
				{
					value *= currency.Multiplier;

					var account = bankMgr.GetBankAccount(playerName, currency.InternalName);
					Debug.Assert(account != null, $"Couldn't find {currency.InternalName} account for {playerName}.");
					
					account.Deposit((decimal)value);

					if( currency.SendCombatText )
					{
						var player = TShockAPI.Utils.Instance.FindPlayer(playerName).FirstOrDefault();

						if( player != null )
						{
							var color = Color.White;
							var money = currency.GetCurrencyConverter().ToStringAndColor((decimal)value,ref color);
							var combatText = $"{money}";
							
							BankingPlugin.Instance.CombatTextDistributor.AddCombatText(combatText, player, color);
						}
					}
				}
			}
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
	}

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
}
