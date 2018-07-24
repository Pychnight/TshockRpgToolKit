using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	public class KillingRewardSource : MultipleRewardSource
	{
		PlayerStrikeInfo StrikeInfo;
		string NpcGivenOrTypeName;
		float NpcValue;
		bool NpcSpawnedFromStatue;

		//cached values
		float TotalDamage;
		float TotalDamageDefended;

		public KillingRewardSource(PlayerStrikeInfo strikeInfo, string npcGivenOrTypeName, float npcValue, bool npcSpawnedFromStatue)
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
			
			foreach( var kvp in StrikeInfo )
			{
				var playerName = kvp.Key;
				var weaponName = kvp.Value.WeaponName;
				var damagePercent = kvp.Value.Damage / TotalDamage;
				var damageDefendedPercent = TotalDamageDefended > 0 ? kvp.Value.DamageDefended / TotalDamageDefended : 0;//avoid divide by 0
				
				//foreach( var currency in bank.CurrencyManager )
				{
					//allow external code a chance to modify the npc's value ( ie, leveling's NpcNameToExp tables... )
					var value = (float)rewardModifier.ModifyBaseRewardValue(RewardReason.Killing, playerName, currency.InternalName, NpcGivenOrTypeName, (decimal)NpcValue);
					//var value = NpcValue;

					var defenseBonus = value * damageDefendedPercent * currency.DefenseBonusMultiplier;

					Debug.Print($"DefenseBonus: {defenseBonus}");

					value *= damagePercent;

					//Weapons are implicitly at 1.0, unless modifier is found.
					float weaponMultiplier = 0;
					if( weaponName != null && currency.WeaponMultipliers?.TryGetValue(weaponName, out weaponMultiplier) == true )
					{
						value *= weaponMultiplier;
					}

					value += defenseBonus;
					value *= currency.Multiplier;
					
					var decimalValue = (decimal)value;
					
					yield return new Tuple<string, decimal>(playerName, decimalValue);
				}
			}
		}
	}
}
