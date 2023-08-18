using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Banking.Rewards
{
	/// <summary>
	/// Negative reward for the Player dying, either due to "natural" causes, or from getting killed by another Player.
	/// </summary>
	public class DeathReward : MultipleRewardBase
	{
		public string OtherPlayerName { get; set; }

		public DeathReward(string playerName, RewardReason rewardReason = RewardReason.Death, string otherPlayerName = "")
		{
			Debug.Assert(rewardReason == RewardReason.Death || rewardReason == RewardReason.DeathPvP,
							$"DeathRewards must use either RewardReason.Death, or RewardReason.DeathPvP.");

			PlayerName = playerName;
			RewardReason = rewardReason;
			OtherPlayerName = otherPlayerName;
		}

		//we cant easily use bankAccount.TransferTo() here, hence why we've gone with an iterator
		protected internal override IEnumerable<Tuple<string, decimal>> OnEvaluateMultiple(CurrencyDefinition currency)//, IRewardModifier rewardModifier = null)
		{
			var rewardAccount = BankingPlugin.Instance.Bank.GetBankAccount(PlayerName, currency.InternalName);

			if (RewardReason == RewardReason.Death)
			{
				var factor = 1.0m; // ( session.Class.DeathPenaltyMultiplierOverride ?? 1.0 );

				decimal loss = Math.Round(Math.Max((decimal)currency.DeathPenaltyMultiplier * factor * rewardAccount.Balance,
															(decimal)currency.DeathPenaltyMinimum), 2);
				if (loss == 0.0m)
					yield break;

				loss = -loss;//this will cause a withdrawal
				yield return new Tuple<string, decimal>(PlayerName, loss);
			}
			else //RewardReason.DeathPvP:
			{
				//decimal value = evaluator.GetRewardValue(playerName, currency.InternalName, itemName, (decimal)defaultValue);//<---not sure how or if to slot this in.
				//decimal loss = Math.Round(Math.Max((decimal)currency.DeathPenaltyPvPMultiplier * rewardAccount.Balance, (decimal)currency.DeathPenaltyMinimum));
				decimal lossPvP = Math.Round(Math.Max((decimal)currency.DeathPenaltyPvPMultiplier * rewardAccount.Balance, (decimal)currency.DeathPenaltyMinimum), 2);

				if (lossPvP == 0.0m)
					yield break;

				if (string.IsNullOrWhiteSpace(OtherPlayerName))//no info on other guy(?)
				{
					lossPvP = -lossPvP;//withdrawal
					yield return new Tuple<string, decimal>(PlayerName, lossPvP);
				}
				else
				{
					//we cant easily use bankAccount.TransferTo() here, hence why we've gone with an iterator

					lossPvP = -lossPvP;//withdraw from dead player's account.
					yield return new Tuple<string, decimal>(PlayerName, lossPvP);

					lossPvP = -lossPvP;//deposit into winning player's account.
					yield return new Tuple<string, decimal>(OtherPlayerName, lossPvP);
				}
			}
		}
	}
}
