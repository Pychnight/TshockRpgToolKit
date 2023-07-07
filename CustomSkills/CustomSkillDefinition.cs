using Newtonsoft.Json;
using System.Collections.Generic;

namespace CustomSkills
{
	/// <summary>
	/// Properties that make up a CustomSkill.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class CustomSkillDefinition
	{
		/// <summary>
		/// Gets or sets a unique identifier to reference this skill type.
		/// </summary>
		[JsonProperty(Order = 0)]
		public string Name { get; set; } = "NewCustomSkill";
		//public string CustomID { get; set; }

		/// <summary>
		/// Gets or sets a user readable summary of this skill.
		/// </summary>
		[JsonProperty(Order = 1)]
		public string Description { get; set; } = "No description.";

		/// <summary>
		/// Gets or sets a list of permissions required to learn this skill.
		/// </summary>
		[JsonProperty(Order = 2)]
		public List<string> PermissionsToLearn { get; set; } = new List<string>();

		/// <summary>
		/// Gets or sets a list of permissions required to use this skill.
		/// </summary>
		[JsonProperty(Order = 3)]
		public List<string> PermissionsToUse { get; set; } = new List<string>();

		/// <summary>
		/// Gets or sets a list of words that when spoken by the player, will cast the skill.
		/// </summary>
		[JsonProperty(Order = 4)]
		public List<string> TriggerWords { get; set; } = new List<string>();

		/// <summary>
		/// Gets or sets whether to notify the user that this skill is ready to use again.
		/// </summary>
		[JsonProperty(Order = 5)]
		public bool NotifyUserOnCooldown { get; set; } = false;

		/// <summary>
		/// Gets or sets a string that will override the default cooldown notification, if notifications are enabled.
		/// </summary>
		[JsonProperty(Order = 6)]
		public string CooldownNotification { get; set; } = "";

		/// <summary>
		/// Gets or sets the list of CustomSkillLevelDefinitions for this skill.
		/// </summary>
		[JsonProperty(Order = 7)]
		public List<CustomSkillLevelDefinition> Levels { get; set; } = new List<CustomSkillLevelDefinition>();

		//helpers
		public bool HasTriggerWords => (TriggerWords?.Count > 0) == true;



		public bool CanLevelUp(int currentLevel) => currentLevel < Levels?.Count - 1;
	}
}
