using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	/// <summary>
	/// Rewards represent a player's monetary gains or losses from various in-game events. 
	/// </summary>
	public abstract class Reward
	{
		public RewardReason RewardReason { get; protected set; }
		public string PlayerName { get; protected set; }
		public string PlayerGroup { get; protected set; }
		
		/// <summary>
		/// Provides a spot to perform evaluation initialization, like computing and caching expensive values, etc.
		/// </summary>
		protected internal virtual void OnPreEvaluate()
		{
		}

		/// <summary>
		/// Computes a decimal value, representing the value of this reward for the given Currency. The value is in generic units, and maybe positive or negative.
		/// </summary>
		/// <param name="currency"></param>
		/// <param name="rewardModifier"></param> 
		/// <returns>Computed value, in generic units.</returns>
		/// <remarks>Implementations do not need to use the rewardModifier, but if they do, they should guard against null values.</remarks>
		protected internal abstract decimal OnEvaluate(CurrencyDefinition currency, IRewardModifier rewardModifier = null);
	}
}
