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
				rewards.TryDequeue(out var reward);
				reward.Give();
				reward.Notify();
			}
		}

		public void AddReward(IReward reward)
		{
			rewards.Enqueue(reward);
		}

		//public void AddNpcKill(string playerName, float damage, float npcValue)
		//{

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
		public string Currency { get; set; }

		public override void Give()
		{
			var account = BankingPlugin.Instance.BankAccountManager.GetBankAccount(PlayerName, Currency);

			Debug.Assert(account != null, $"Couldn't find {Currency} account.");

			account?.Deposit((decimal)Value);
		}

		public override void Notify()
		{
			if(!string.IsNullOrWhiteSpace(CombatText))
			{
				var player = TShock.Utils.FindPlayer(PlayerName).FirstOrDefault();

				if(player!=null)
				{
					player.SendInfoMessage(CombatText);
				}
			}
		}
	}
}
