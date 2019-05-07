using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
		/// Gets or sets a comma separated list of permissions required to learn this skill.
		/// </summary>
		[JsonProperty(Order = 2)]
		public string PermissionsToLearn { get; set; } = "";

		/// <summary>
		/// Gets or sets a comma separated list of permissions required to use this skill.
		/// </summary>
		[JsonProperty(Order = 3)]
		public string PermissionsToUse { get; set; } = "";
		
		/// <summary>
		/// Gets or sets whether to notify the user that this skill is ready to use again.
		/// </summary>
		[JsonProperty(Order = 4)]
		public bool NotifyUserOnCooldown { get; set; } = false;

		/// <summary>
		/// Gets or sets the list of CustomSkillLevelDefinitions for this skill.
		/// </summary>
		[JsonProperty(Order = 5)]
		public List<CustomSkillLevelDefinition> Levels { get; set; } = new List<CustomSkillLevelDefinition>();

		//helpers
		public bool CanLevelUp(int currentLevel) => currentLevel < Levels?.Count - 1;
	}
}
