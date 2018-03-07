namespace Banking
{
	public delegate decimal RewardModifier(string currencyType, string gainedBy, string itemName, decimal rewardValue);
}