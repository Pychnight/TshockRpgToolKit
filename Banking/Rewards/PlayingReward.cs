using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	public class PlayingReward : Reward
	{
		public CurrencyDefinition Currency { get; private set; }
		
		public PlayingReward(string playerName, CurrencyDefinition currency)
		{
			PlayerName = playerName;
			RewardReason = RewardReason.Playing;
			Currency = currency;
		}
		
		protected internal override decimal OnEvaluate(CurrencyDefinition currency, IRewardModifier rewardModifier = null)
		{
			if( currency != Currency )
				return 0m;

			var value = currency.DefaultPlayingValue;

			value *= (decimal)currency.Multiplier;

			return value;
		}
	}
}
