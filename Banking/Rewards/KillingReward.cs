using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Banking.Rewards
{
	/// <summary>
	/// Reward for killing npc's. Multiple players may share a single reward if they've all attacked the same NPC.
	/// </summary>
	public class KillingReward : MultipleRewardBase
	{
		PlayerStrikeInfo StrikeInfo;
		public string NpcGivenOrTypeName { get; }
		int npcHp;
		public bool NpcSpawnedFromStatue { get; }

		//cached values
		float totalDamage;
		float totalDamageDefended;

		public KillingReward(PlayerStrikeInfo strikeInfo, string npcGivenOrTypeName, int npcHp, bool npcSpawnedFromStatue)
		{
			RewardReason = RewardReason.Killing;
			StrikeInfo = strikeInfo;
			NpcGivenOrTypeName = npcGivenOrTypeName;
			this.npcHp = npcHp;
			NpcSpawnedFromStatue = npcSpawnedFromStatue;
		}

		protected internal override void OnPreEvaluate()
		{
			totalDamage = StrikeInfo.Values.Select(si => si.Damage).Sum();
			totalDamageDefended = StrikeInfo.Values.Select(si => si.DamageDefended).Sum();

			//if(TotalDamage>npcHp)
			//{
			//	TotalDamage = npcHp;
			//}
		}

		protected internal override IEnumerable<Tuple<string, decimal>> OnEvaluateMultiple(CurrencyDefinition currency)//, IRewardModifier rewardModifier = null)
		{
			if (NpcSpawnedFromStatue && !currency.EnableStatueNpcRewards)
				yield break;

			var npcBaseValue = currency.GetKillingValueOverride(NpcGivenOrTypeName) ?? npcHp;

			foreach (var kvp in StrikeInfo)
			{
				var playerName = kvp.Key;
				var weaponName = kvp.Value.WeaponName;
				var damagePercent = kvp.Value.Damage / totalDamage;
				var damageDefendedPercent = totalDamageDefended > 0 ? kvp.Value.DamageDefended / totalDamageDefended : 0;//avoid divide by 0

				//allow external code a chance to modify the npc's value ( ie, leveling's NpcNameToExp tables... )
				//var value = (float)rewardModifier.ModifyBaseRewardValue(RewardReason.Killing, playerName, currency.InternalName, NpcGivenOrTypeName, (decimal)NpcValue);
				//var value = NpcValue;

				var value = npcBaseValue;
				var defenseBonus = value * (decimal)(damageDefendedPercent * currency.DefenseBonusMultiplier);

				Debug.Print($"DefenseBonus: {defenseBonus}");

				value *= (decimal)damagePercent;

				//Weapons are implicitly at 1.0, unless modifier is found.
				float weaponMultiplier = 0;
				if (weaponName != null && currency.WeaponMultipliers?.TryGetValue(weaponName, out weaponMultiplier) == true)
				{
					value *= (decimal)weaponMultiplier;
				}

				value += defenseBonus;
				value *= (decimal)currency.Multiplier;

				yield return new Tuple<string, decimal>(playerName, value);
			}
		}
	}
}
