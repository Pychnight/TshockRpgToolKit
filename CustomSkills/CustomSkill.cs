using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomSkills
{
	internal class CustomSkillManager
	{
		internal const string DefaultCategoryName = "uncategorized";

		internal Dictionary<string, CustomSkillCategory> Categories { get; private set; }

		internal CustomSkillManager()
		{
			Categories = new Dictionary<string, CustomSkillCategory>()
			{
				{ DefaultCategoryName, new CustomSkillCategory(DefaultCategoryName) }
			};
		}

		internal CustomSkillCategory TryGetCategory(string categoryName = null)
		{
			categoryName = string.IsNullOrWhiteSpace(categoryName) ? DefaultCategoryName : categoryName;

			Categories.TryGetValue(categoryName, out var category);

			return category;
		}

		internal CustomSkillDefinition TryGetDefinition(string skillName, string categoryName = null)
		{
			var category = TryGetCategory(categoryName);

			//find skill def
			category.TryGetValue(skillName, out var skillDefinition);

			return skillDefinition;
		}
	}

	//internal class CustomSkillManager
	//{
	//	internal Dictionary<string, CustomSkillDefinition> CustomSkillDefinitions { get; private set; }
	//	internal Dictionary<string, CustomSkillCategory> CustomSkillCategories { get; private set; }
	//	internal CustomSkillDefinition this[string skillName]
	//	{
	//		get
	//		{
	//			CustomSkillDefinitions.TryGetValue(skillName, out var result);
	//			return result;
	//		}
	//	}
				
	//	internal CustomSkillManager()
	//	{
	//		CustomSkillDefinitions = new Dictionary<string, CustomSkillDefinition>();
	//		CustomSkillCategories = new Dictionary<string, CustomSkillCategory>();
	//	}

	//	internal void Add(CustomSkillDefinition definition, string categoryName = "")
	//	{
	//		CustomSkillDefinitions.Add(definition.Name, definition);
			
	//		//handle categorization
	//		if(!string.IsNullOrWhiteSpace(categoryName))
	//		{
	//			if(!CustomSkillCategories.TryGetValue(categoryName,out var category))
	//			{
	//				category = new CustomSkillCategory(categoryName);
	//				CustomSkillCategories.Add(categoryName,category);
	//			}

	//			category.Add(definition.Name, definition);
	//		}
	//	}
	//}

	public class CustomSkillCategory : Dictionary<string, CustomSkillDefinition>
	{
		public string Name { get; private set; }

		public CustomSkillCategory(string name)
		{
			Name = name;
		}
	}

	public class CustomSkillDefinition
	{
		//public string CustomID { get; set; }
		//unique identifier.
		public string Name { get; set; } = "NewCustomSkill";
		
		//user readable summary of skill.
		public string Description { get; set; } = "No description.";
	}
}
