﻿namespace Banking.Rewards
{
	/// <summary>
	/// Reward for fishing and catching items.
	/// </summary>
	public class FishingReward : Reward
	{
		public int StackSize { get; set; } // StackSize wont work-- it will be the sum of the previous slot + new items.
		public byte Prefix { get; set; }
		public int ItemId { get; set; }

		public FishingReward(string playerName, int stackSize, byte prefix, int itemId)
		{
			PlayerName = playerName;
			RewardReason = RewardReason.Fishing;
			StackSize = stackSize;
			Prefix = prefix;
			ItemId = itemId;
		}

		protected internal override decimal OnEvaluate(CurrencyDefinition currency)//, IRewardModifier rewardModifier = null)
=> currency.GetBaseFishingValue(ItemId, Prefix);// StackSize wont work-- it will be the sum of the previous slot + new items.
	}
}
