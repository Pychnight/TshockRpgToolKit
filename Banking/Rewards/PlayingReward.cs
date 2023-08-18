namespace Banking.Rewards
{
	/// <summary>
	/// Reward for playing a certain amount of time.
	/// </summary>
	public class PlayingReward : Reward
	{
		public CurrencyDefinition Currency { get; private set; }

		public PlayingReward(string playerName, string playerGroup, CurrencyDefinition currency)
		{
			PlayerName = playerName;
			PlayerGroup = playerGroup;
			RewardReason = RewardReason.Playing;
			Currency = currency;
		}

		protected internal override decimal OnEvaluate(CurrencyDefinition currency)//, IRewardModifier rewardModifier = null)
		{
			if (currency != Currency)
				return 0m;

			var value = currency.GetBasePlayingValue(PlayerGroup);

			value *= (decimal)currency.Multiplier;

			return value;
		}
	}
}
