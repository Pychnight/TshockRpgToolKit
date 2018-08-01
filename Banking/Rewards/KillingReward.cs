using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	/// <summary>
	/// Reward for killing npc's. Multiple players may share a single reward if they've all attacked the same NPC.
	/// </summary>
	public class KillingReward : MultipleRewardBase
	{
		PlayerStrikeInfo StrikeInfo;
		string NpcGivenOrTypeName;
		float NpcValue;
		bool NpcSpawnedFromStatue;

		//cached values
		float TotalDamage;
		float TotalDamageDefended;

		public KillingReward(PlayerStrikeInfo strikeInfo, string npcGivenOrTypeName, float npcValue, bool npcSpawnedFromStatue)
		{
			RewardReason = RewardReason.Killing;
			StrikeInfo = strikeInfo;
			NpcGivenOrTypeName = npcGivenOrTypeName;
			NpcValue = npcValue;
			NpcSpawnedFromStatue = npcSpawnedFromStatue;
		}

		protected internal override void OnPreEvaluate()
		{
			TotalDamage = StrikeInfo.Values.Select(si => si.Damage).Sum();
			TotalDamageDefended = StrikeInfo.Values.Select(si => si.DamageDefended).Sum();
		}

		protected internal override IEnumerable<Tuple<string,decimal>> OnEvaluateMultiple(CurrencyDefinition currency, IRewardModifier rewardModifier = null)
		{
			if( NpcSpawnedFromStatue && !currency.EnableStatueNpcRewards )
				yield break;

			var npcBaseValue = currency.GetKillingValueOverride(NpcGivenOrTypeName) ?? (decimal)NpcValue;
			
			foreach( var kvp in StrikeInfo )
			{
				var playerName = kvp.Key;
				var weaponName = kvp.Value.WeaponName;
				var damagePercent = kvp.Value.Damage / TotalDamage;
				var damageDefendedPercent = TotalDamageDefended > 0 ? kvp.Value.DamageDefended / TotalDamageDefended : 0;//avoid divide by 0
							
				//allow external code a chance to modify the npc's value ( ie, leveling's NpcNameToExp tables... )
				//var value = (float)rewardModifier.ModifyBaseRewardValue(RewardReason.Killing, playerName, currency.InternalName, NpcGivenOrTypeName, (decimal)NpcValue);
				//var value = NpcValue;

				var value = npcBaseValue;
				var defenseBonus = value * (decimal)(damageDefendedPercent * currency.DefenseBonusMultiplier);

				Debug.Print($"DefenseBonus: {defenseBonus}");

				value *= (decimal)damagePercent;

				//Weapons are implicitly at 1.0, unless modifier is found.
				float weaponMultiplier = 0;
				if( weaponName != null && currency.WeaponMultipliers?.TryGetValue(weaponName, out weaponMultiplier) == true )
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
