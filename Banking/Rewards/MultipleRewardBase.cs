using System;
using System.Collections.Generic;

namespace Banking.Rewards
{
	/// <summary>
	/// Base class for Reward's that can reward multiple players or a single player with multiple rewards.
	/// </summary>
	public abstract class MultipleRewardBase : Reward
	{
		protected internal override decimal OnEvaluate(CurrencyDefinition currency)//, IRewardModifier rewardEvaluator = null)
=> throw new NotImplementedException();

		/// <summary>
		/// Computes an IEnumerable of Tuple's containing the player name, and reward amount, in generic units.
		/// </summary>
		/// <param name="currency"></param>
		/// <returns></returns>
		protected internal abstract IEnumerable<Tuple<string, decimal>> OnEvaluateMultiple(CurrencyDefinition currency);//, IRewardModifier rewardModifier = null);
	}
}
