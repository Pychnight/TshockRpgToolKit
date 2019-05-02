using Newtonsoft.Json;
using System.Collections.Generic;

namespace CustomSkills
{
	/// <summary>
	/// Represents a grouping of related CustomSkills.
	/// </summary>
	[JsonConverter(typeof(CustomSkillCategoryJsonConverter))]
	public class CustomSkillCategory : Dictionary<string, CustomSkillDefinition>
	{
		public string Name { get; set; } = "NewCustomSkillCategory";

		public CustomSkillCategory() : base()
		{
		}

		public CustomSkillCategory(string name) : this()
		{
			Name = name;
		}
	}
}
