using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Manages the lifetime of CustomSkills.
	/// </summary>
	internal class CustomSkillRunner
	{
		//internal static HashSet<CustomSkill> ActiveSkills { get; set; } = new HashSet<CustomSkill>();
		internal static Dictionary<string, CustomSkill> ActiveSkills = new Dictionary<string, CustomSkill>();
		//internal static List<CustomSkill> CoolingDownSkills = new List<CustomSkill>(); 

		internal static bool AddActiveSkill(TSPlayer player, CustomSkillDefinition skillDefinition, int level)
		{
			if(ActiveSkills.ContainsKey(player.Name))
			{
				//cant add skill, the player already has an active skill running
				return false;
			}
									
			var skill = new CustomSkill(player, skillDefinition, level);

			skill.Phase = SkillPhase.Casting;
			ActiveSkills.Add(player.Name,skill);

			return true;
		}

		internal static void UpdateActiveSkills()
		{
			var removalList = new List<CustomSkill>();

			foreach(var skill in ActiveSkills.Values)
			{
				skill.Update();

				if(skill.Phase == SkillPhase.Completed || skill.Phase == SkillPhase.Failed)
					removalList.Add(skill);
			}

			foreach(var skill in removalList)
			{
				ActiveSkills.Remove(skill.PlayerName);
			}
		}
	}
}
