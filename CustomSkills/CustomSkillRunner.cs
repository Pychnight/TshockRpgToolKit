using System.Collections.Generic;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Manages the lifetime of CustomSkills.
	/// </summary>
	internal class CustomSkillRunner
	{
		internal static HashSet<CustomSkill> ActiveSkills { get; set; } = new HashSet<CustomSkill>();

		internal static void AddActiveSkill(TSPlayer player, CustomSkillDefinition skillDefinition, int level)
		{
			var skill = new CustomSkill(player, skillDefinition, level);

			skill.Phase = SkillPhase.Casting;
			ActiveSkills.Add(skill);
		}

		internal static void UpdateActiveSkills()
		{
			foreach(var skill in ActiveSkills)
				skill.Update();

			ActiveSkills.RemoveWhere(s => s.Phase == SkillPhase.Completed ||
										s.Phase == SkillPhase.Cancelled ||
										s.Phase == SkillPhase.Failed);
		}
	}
}
