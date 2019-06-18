using BooTS;
using Corruption.PluginSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Loads and stores CustomSkillDefinitions.
	/// </summary>
	internal class CustomSkillDefinitionLoader
	{
		internal const string DefaultCategoryName = "uncategorized";

		BooModuleManager booModuleManager = null;

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
			DefinitionFile<List<CustomSkillCategory>> fileDef = null;

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
				fileDef = JsonConvert.DeserializeObject<DefinitionFile<List<CustomSkillCategory>>>(json);
			}

			//convert file def into a customskillmanager...
			var mgr = new CustomSkillDefinitionLoader(fileDef.Data);
			
			return mgr;
		}
		
		private HashSet<string> GetScriptPaths()
		{
			var result = new HashSet<string>();

			foreach(var cat in Categories.Values)
			{
				foreach(var skill in cat.Values)
				{
					foreach(var level in skill.Levels)
					{
						if(!string.IsNullOrWhiteSpace(level.ScriptPath))
						{
							result.Add(CustomSkillsPlugin.Instance.PluginRelativePath(level.ScriptPath));
						}
					}
				}
			}

			return result;
		}
		
		private void LoadScripts()
		{
			var moduleManager = GetModuleManager();
			var scriptPaths = GetScriptPaths();

			foreach(var path in scriptPaths)
				moduleManager.Add(path);

			var results = moduleManager.Compile();

			foreach(var cat in Categories.Values)
			{
				foreach(var skill in cat.Values)
				{
					foreach(var level in skill.Levels)
					{
						if(results.TryGetValue(CustomSkillsPlugin.Instance.PluginRelativePath(level.ScriptPath),out var compilerContext))
						{
							CustomSkillsPlugin.Instance.LogPrintBooErrors(compilerContext);
							CustomSkillsPlugin.Instance.LogPrintBooWarnings(compilerContext);

							if(compilerContext.Errors.Count<1)
							{
								var linker = new BooModuleLinker(compilerContext.GeneratedAssembly, level.ScriptPath);

								if(level.OnCancelled == null)
									level.OnCancelled = linker.TryCreateDelegate<Action<TSPlayer,SkillState>>("OnCancelled");

								if(level.OnLevelUp==null)
									level.OnLevelUp = linker.TryCreateDelegate<Action<TSPlayer>>("OnLevelUp");

								if(level.OnCast == null)
									level.OnCast = linker.TryCreateDelegate<Func<TSPlayer,SkillState,bool>>("OnCast");

								if(level.OnCharge == null)
									level.OnCharge = linker.TryCreateDelegate<Func<TSPlayer,SkillState,bool>>("OnCharge");

								if(level.OnFire == null)
									level.OnFire = linker.TryCreateDelegate<Action<TSPlayer,SkillState>>("OnFire");
							}
						}
					}
				}
			}
		}
				
		private BooModuleManager GetModuleManager()
		{
			//if(booModuleManager == null)
			{
				var mgr = booModuleManager = new BooModuleManager(CustomSkillsPlugin.Instance,
																	ScriptHelpers.GetReferences(),
																	ScriptHelpers.GetDefaultImports(),
																	ScriptHelpers.GetEnsuredMethodSignatures());

				mgr.AssemblyNamePrefix = "skill_";

			}
			
			return booModuleManager;
		}

		private static DefinitionFile<List<CustomSkillCategory>> CreateDefaultDataDefinition()
		{
			var fileDef = new DefinitionFile<List<CustomSkillCategory>>()
			{
				Version = 0.1f,
				Metadata = new Dictionary<string, object>()
				{
					{ "Authors", "Autogenerated by CustomSkills plugin." },
					{ "Remarks", "This file format is under active development. Do not rely on any properties being available in future versions." }
				},
				Data = new List<CustomSkillCategory>()
				{
					new CustomSkillCategory(DefaultCategoryName)
					{
						{ "TestSkill", new CustomSkillDefinition()
							{
								Name = "TestSkill",
								Description = "This skill is just a placeholder!",
								NotifyUserOnCooldown = true,
								Levels = new List<CustomSkillLevelDefinition>()
								{
									new CustomSkillLevelDefinition()
									{
										ScriptPath = "script.boo",
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
