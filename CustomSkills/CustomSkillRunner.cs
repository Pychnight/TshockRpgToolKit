using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Manages the lifetime of CustomSkills.
	/// </summary>
	internal class CustomSkillRunner
	{
		internal static Dictionary<string, CustomSkill> ActiveSkills = new Dictionary<string, CustomSkill>();
                
        /// <summary>
        /// For skills that notify the player on cooldown, we keep this separate list.  
        /// </summary>
        internal static List<CustomSkill> CooldownNotificationList = new List<CustomSkill>();
        
		//each player can only have a single skill active!
		internal static bool AddActiveSkill(TSPlayer player, CustomSkillDefinition skillDefinition, int level)
		{
			if(ActiveSkills.ContainsKey(player.Name))
			{
				//cant add skill, the player already has an active skill running
				return false;
			}

			var session = Session.GetOrCreateSession(player);
			if(!session.CanAffordCastingSkill(skillDefinition.Name))
			{
				//cant afford it, abort
				return false;
			}

			var skill = new CustomSkill(player, skillDefinition, level);

			if(session.TryDeductCost(player, skill.LevelDefinition.CastingCost))
			{
				//Debug.Print("Player was unable to afford casting cost, after casting started. Ignoring.");
				skill.Phase = SkillPhase.Casting;
				skill.CastStartTime = DateTime.Now;

				ActiveSkills.Add(player.Name, skill);

				return true;
			}
			else
				return false;
		}

		internal static CustomSkillDefinition RemoveActiveSkill(string playerName)
		{
			if(ActiveSkills.TryGetValue(playerName, out var skill))
			{
				skill.Phase = SkillPhase.Cancelled;//let update handle all further termination. 
				return skill.Definition;
			}

			return null;
		}

		internal static void Update()
		{
			var removalList = new List<CustomSkill>();

			foreach(var skill in ActiveSkills.Values)
			{
				skill.Update();

				if(skill.Phase == SkillPhase.Completed || skill.Phase == SkillPhase.Failed)
					removalList.Add(skill);
			}

			//clean up active skills
			foreach(var skill in removalList)
			{
				ActiveSkills.Remove(skill.PlayerName);

				if(skill.NotifyUserOnCooldown)
					CooldownNotificationList.Add(skill);
			}

            //check for cooldown notifications...
            removalList.Clear();
            
			foreach(var skill in CooldownNotificationList)
			{
				if(skill.HasCooldownCompleted())
				{
					if(skill.Player.Active)
					{
						var message = skill.Definition.CooldownNotification;

						if(string.IsNullOrWhiteSpace(message))
							message = $"{skill.Definition.Name} is ready.";

						skill.Player.SendInfoMessage(message);
					}

					removalList.Add(skill);
				}
			}

			//clean up notifications
			foreach(var skill in removalList)
				CooldownNotificationList.Remove(skill);
		}
	}
}
