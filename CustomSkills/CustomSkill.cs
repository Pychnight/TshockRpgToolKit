using Newtonsoft.Json;
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
		//public string CustomID { get; set; }
		//unique identifier.
		[JsonProperty(Order = 0)]
		public string Name { get; set; } = "NewCustomSkill";

		//user readable summary of skill.
		[JsonProperty(Order = 1)]
		public string Description { get; set; } = "No description.";
	}
}
