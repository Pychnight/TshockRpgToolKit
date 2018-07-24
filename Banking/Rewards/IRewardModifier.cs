using TShockAPI;

namespace Banking.Rewards
{
	/// <summary>
	///		Interface for types that modify the base values for a Banking Currency reward.
	/// </summary>
	public interface IRewardModifier
	{
		decimal ModifyBaseRewardValue(RewardReason reason, string playerName, string currencyType, string itemName, decimal rewardValue);
	}
}