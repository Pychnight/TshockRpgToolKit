using TShockAPI;

namespace Banking.Rewards
{
	/// <summary>
	///		Interface for types that calculate the values for a Banking Currency reward.
	/// </summary>
	public interface IRewardEvaluator
	{
		decimal GetRewardValue(RewardReason reason, string playerName, string currencyType, string itemName, decimal rewardValue);
	}
}