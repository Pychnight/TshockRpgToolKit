using Banking.Rewards;
using System;

namespace Banking
{
	/// <summary>
	/// EventArgs sent during Currency.PreReward event.
	/// </summary>
	public class RewardEventArgs : EventArgs
	{
		public Reward Reward { get; }
		public RewardReason RewardReason => Reward.RewardReason;
		public string PlayerName { get; } //=> Reward.PlayerName; // <-- dont use member! this might be set from a multi reward.
		//public string PlayerGroup //=> Reward.PlayerGroup;
		public bool IsMultiReward => Reward is MultipleRewardBase;
		public decimal RewardValue { get; set; }
		public CurrencyDefinition Currency { get; }

		public RewardEventArgs(Reward reward, ref decimal rewardValue, CurrencyDefinition currency, string playerName)
		{
			Reward = reward;
			RewardValue = rewardValue;
			Currency = currency;
			PlayerName = playerName;
		}
	}
}