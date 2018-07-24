using Banking.Configuration;
using Banking.Rewards;
using System.Diagnostics;

namespace Banking
{
	/// <summary>
	/// RewardSource for when a player votes on the server.
	/// </summary>
	public class VoteRewardSource : RewardSource
	{
		public VoteRewardSource(string playerName)
		{
			RewardReason = RewardReason.Undefined;
			PlayerName = playerName;
		}

		protected internal override decimal OnEvaluate(CurrencyDefinition currency, IRewardModifier rewardModifier = null)
		{
			var bank = BankingPlugin.Instance.Bank;
			var playerAccountMap = bank[PlayerName];
			var votingConfig = Config.Instance.Voting;

			if(votingConfig.Rewards.TryGetValue(currency.InternalName, out var moneyString))
			{
				if( currency.GetCurrencyConverter().TryParse(moneyString, out var value) )
				{
					value *= (decimal)currency.Multiplier;
					return value;
				}

				Debug.Print($"Transaction skipped. Couldn't parse '{value}' as a valid {currency.InternalName} value for {PlayerName}.");
			}
						
			return 0m;
		}
	}
}