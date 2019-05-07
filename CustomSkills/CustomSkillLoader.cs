﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomSkills
{
	/// <summary>
	/// Loads and stores CustomSkillDefinitions.
	/// </summary>
	internal class CustomSkillDefinitionLoader
	{
		internal const string DefaultCategoryName = "uncategorized";

		internal Dictionary<string, CustomSkillCategory> Categories { get; private set; }
		internal Dictionary<string, CustomSkillDefinition> TriggeredDefinitions { get; private set; }
		//internal Dictionary<string, CustomSkillDefinition> TriggerWordsToDefinitions { get; private set; }

		internal CustomSkillDefinitionLoader()
		{
			Categories = new Dictionary<string, CustomSkillCategory>()
			{
				{ DefaultCategoryName, new CustomSkillCategory(DefaultCategoryName) }
			};
		}

		private CustomSkillDefinitionLoader(List<CustomSkillCategory> customSkillCategories) : this()
		{
			foreach(var srcCategory in customSkillCategories)
			{
				if(Categories.TryGetValue(srcCategory.Name, out var dstCategory))
				{
					//copy to existing
					foreach(var kvp in srcCategory)
						dstCategory.Add(kvp.Key, kvp.Value);
				}
				else
				{
					//just add incoming category
					Categories.Add(srcCategory.Name, srcCategory);
				}
			}

			//TriggerWordsToDefinitions = MapTriggerWords();
			TriggeredDefinitions = MapTriggeredDefinitions();
			LoadScripts();
		}

		private Dictionary<string,CustomSkillDefinition> MapTriggeredDefinitions()
		{
			var result = new Dictionary<string, CustomSkillDefinition>();

			foreach(var cat in Categories.Values)
			{
				foreach(var skill in cat.Values)
				{
					if(skill.HasTriggerWords)
					{
						result[skill.Name] = skill;
					}
				}
			}
			
			return result;
		}

		//private Dictionary<string,CustomSkillDefinition> MapTriggerWords()
		//{
		//	var result = new Dictionary<string, CustomSkillDefinition>();

		//	foreach(var cat in Categories.Values)
		//	{
		//		foreach(var skill in cat.Values)
		//		{
		//			if(skill.HasTriggerWords)
		//			{
		//				foreach(var word in skill.TriggerWords)
		//				{
		//					if(!string.IsNullOrWhiteSpace(word))
		//						result[word] = skill;
		//				}
		//			}
		//		}
		//	}

		//	return result;
		//}

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

		internal static CustomSkillDefinitionLoader Load(string filePath, bool createIfNeeded = true)
		{
			DataDefinitionFile<List<CustomSkillCategory>> fileDef = null;

			if(!File.Exists(filePath))
			{
				if(createIfNeeded)
				{
					fileDef = CreateDefaultDataDefinition();

					var json = JsonConvert.SerializeObject(fileDef, Formatting.Indented);
					File.WriteAllText(filePath, json);
				}
			}
			else
			{
				var json = File.ReadAllText(filePath);
				fileDef = JsonConvert.DeserializeObject<DataDefinitionFile<List<CustomSkillCategory>>>(json);
			}

			//convert file def into a customskillmanager...
			var mgr = new CustomSkillDefinitionLoader(fileDef.Data);
			
			return mgr;
		}
		
		private void LoadScripts()
		{
			foreach(var cat in Categories.Values)
			{
				foreach(var skill in cat.Values)
				{
					foreach(var level in skill.Levels)
					{
						if(!string.IsNullOrWhiteSpace(level.ScriptPath))
						{
							if(level.ScriptPath.StartsWith("[dev]/"))
							{
								var name = level.ScriptPath.Replace("[dev]/", "");

								switch(name)
								{
									case "WindBreaker1":
										level.OnCast = DevScripts.WindBreaker.OnCast;
										level.OnCharge = DevScripts.WindBreaker.OnCharging;
										level.OnFire = DevScripts.WindBreaker.OnFire;
										break;

									case "WindBreaker2":
										level.OnLevelUp = DevScripts.WindBreaker2.OnLevelUp;
										level.OnCast = DevScripts.WindBreaker2.OnCast;
										level.OnCharge = DevScripts.WindBreaker2.OnCharging;
										level.OnFire = DevScripts.WindBreaker2.OnFire;
										break;

									default:
										level.OnCast = DevScripts.TestSkill.OnCast;
										level.OnCharge = DevScripts.TestSkill.OnCharging;
										level.OnFire = DevScripts.TestSkill.OnFire;
										break;
								}
							}
						}
					}
				}
			}
		}

		private static DataDefinitionFile<List<CustomSkillCategory>> CreateDefaultDataDefinition()
		{
			var fileDef = new DataDefinitionFile<List<CustomSkillCategory>>()
			{
				Version = 0.1f,
				Metadata = new Dictionary<string, object>()
				{
					{ "Authors", "Autogenerated by CustomSkills plugin." },
					{"Remarks", "This file format is under active development. Do not rely on any properties being available in future versions." }
				},
				Data = new List<CustomSkillCategory>()
				{
					new CustomSkillCategory(DefaultCategoryName)
					{
						{ "TestSkill", new CustomSkillDefinition()
							{
								Name = "TestSkill",
								Description = "This skill is just for testing!",
								NotifyUserOnCooldown = true,
								Levels = new List<CustomSkillLevelDefinition>()
								{
									new CustomSkillLevelDefinition()
									{
										ScriptPath = "[dev]/Test",
										CanInterrupt = true,
										CanCasterMove = true,
										UsesToLevelUp = 0
									}
								}
							}
						}
					}
				}
			};

			return fileDef;
		}
	}


}
