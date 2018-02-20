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

		CurrencyDefinition expCurrency;
		CurrencyDefinition dustCurrency;
		
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

		public void OnLoad(CurrencyManager currencies)
		{
			expCurrency = currencies["Exp"];
			dustCurrency = currencies["Dust"];
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

		public void AddReward(string playerName, string currencyType, float value, string combatTextFormatString="{0}")
		{
			if( value < 1f )
				return;

			var account = BankingPlugin.Instance.BankAccountManager.GetBankAccount(playerName, currencyType);
			Debug.Assert(account != null, $"Couldn't find {currencyType} account for {playerName}.");

			if(account!=null)
			{
				account.Deposit((decimal)value);

				var currency = BankingPlugin.Instance.BankAccountManager.CurrencyManager[currencyType];
				Debug.Assert(account != null, $"Couldn't find currency type {currencyType} for {playerName}.");

				if( currency != null && currency.SendCombatText )
				{
					var player = TShockAPI.Utils.Instance.FindPlayer(playerName).FirstOrDefault();
					
					if( player != null )
					{
						var money = currency.GetCurrencyConverter().ToString((decimal)value);
						var combatText = string.Format(combatTextFormatString, money);
						//var color = currency.;
						var color = Color.Green;

						BankingPlugin.Instance.CombatTextDistributor.AddCombatText(combatText, player, color);
					}
				}
			}
		}

		public void AddNpcKill(string playerName, float damage, float npcValue)
		{
			if( npcValue < 1f )
				return;

			if( dustCurrency != null )
			{
				//var reward = new CurrencyReward()
				//{
				//	CurrencyType = "Exp",
				//	PlayerName = playerName,
				//	Value = val,
				//	CombatText = $"+{val}!"
				//};

				//AddReward(reward);

				var value = npcValue;
				var currencyType = "Dust";
				var account = BankingPlugin.Instance.BankAccountManager.GetBankAccount(playerName, currencyType);
				Debug.Assert(account != null, $"Couldn't find {currencyType} account for {playerName}.");

				account?.Deposit((decimal)value);
				
				if(dustCurrency.SendCombatText)
				{
					var player = TShockAPI.Utils.Instance.FindPlayer(playerName).FirstOrDefault();

					if(player!=null)
					{
						var txt = $"+{value}";

						//BankingPlugin.Instance.CombatTextDistributor.AddCombatText(txt, player, color);
					}
				}
			}
			
			//val = npcValue * 1f;

			//reward = new CurrencyReward()
			//{
			//	CurrencyType = "Dust",
			//	PlayerName = playerName,
			//	Value = val,
			//	CombatText = $"+{val}!"
			//};

			//AddReward(reward);
		}

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
