using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Banking;
using Boo.Lang.Compiler;
using BooTS;
using Corruption.PluginSupport;
using Leveling.Levels;
using Leveling.Sessions;
using Newtonsoft.Json;
using TerrariaApi.Server;
using TShockAPI;

namespace Leveling.Classes
{
    /// <summary>
    ///     Represents a class definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ClassDefinition
    {
		/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty("Name", Order = 0)]
		public string Name { get; internal set; }

		/// <summary>
		///     Gets the display name.
		/// </summary>
		[JsonProperty("DisplayName", Order = 1)]
		public string DisplayName { get; internal set; }
		
		/// <summary>
		/// Gets or sets the ScriptPath.
		/// </summary>
		[JsonProperty("ScriptPath", Order = 2)]
		public string ScriptPath { get; set; }

		/// <summary>
		///     Gets the list of prerequisite levels.
		/// </summary>
		[JsonProperty("PrerequisiteLevels", Order = 3)]
		public IList<string> PrerequisiteLevelNames { get; internal set; } = new List<string>();

		/// <summary>
		///     Gets the list of prerequisite permissions.
		/// </summary>
		[JsonProperty(Order = 4)]
		public IList<string> PrerequisitePermissions { get; internal set; } = new List<string>();
		
		/// <summary>
		///		Gets or sets the Currency cost to enter this class.
		/// </summary>
		[JsonProperty(Order = 6, PropertyName = "Cost")]
		public string CostString { get; set; } = "";

		/// <summary>
		/// The parsed equivalent of CostString.
		/// </summary>
		public decimal Cost { get; set; }

		/// <summary>
		/// The Currency used to to enter this class. This is determined from CostString.
		/// </summary>
		public CurrencyDefinition CostCurrency { get; set; }

		/// <summary>
		///     Gets a value indicating whether to allow switching the class after mastery.
		/// </summary>
		[JsonProperty(Order = 7)]
		public bool AllowSwitching { get; internal set; } = true;

		/// <summary>
		///     Gets a value indicating whether to allow switching the class before mastery.
		/// </summary>
		[JsonProperty(Order = 8)]
		public bool AllowSwitchingBeforeMastery { get; internal set; }

		/// <summary>
		///     Gets the EXP multiplier override.
		/// </summary>
		[JsonProperty(Order = 9)]
		public double? ExpMultiplierOverride { get; internal set; }

		/// <summary>
		///     Gets the death penalty multiplier override.
		/// </summary>
		[JsonProperty(Order = 10)]
		public double? DeathPenaltyMultiplierOverride { get; internal set; }

		/// <summary>
		///		Gets the list of commands to execute on first change to a class.
		/// </summary>
		[JsonProperty("CommandsOnClassChangeOnce", Order = 11)]
		public IList<string> CommandsOnClassChangeOnce { get; internal set; } = new List<string>();
		
		/// <summary>
		///     Gets the list of level definitions.
		/// </summary>
		[JsonProperty("Levels", Order = 12)]
		public IList<LevelDefinition> LevelDefinitions { get; internal set; } = new List<LevelDefinition>();

		/// <summary>
		/// Gets or sets the Currency used for Leveling purposes.
		/// </summary>
		public CurrencyDefinition LevelingCurrency { get; set; }

		/// <summary>
		///     Gets the mapping of NPC names to EXP rewards.
		/// </summary>
		[JsonProperty("NpcToExpReward", Order = 13)]
		public Dictionary<string, string> NpcNameToExpReward = new Dictionary<string, string>();

		/// <summary>
		///		Gets a mapping of NPC names to preparsed EXP values.
		/// </summary>
		internal Dictionary<string, decimal> ParsedNpcNameToExpValues { get; set; } = new Dictionary<string, decimal>();
				
		//--- new stuff
		public string DisplayInfo { get; internal set; }

		//not sure how these should/would work
		//public Action<object> OnMaximumCurrency;
		//public Action<object> OnNegativeCurrency;
		
		//player, currentclass, currentLevelIndex
		public Action<TSPlayer,Class,int> OnLevelUp;

		//player, currentclass, currentLevelIndex
		public Action<TSPlayer,Class,int> OnLevelDown;

		//player, currentclass, oldclass
		public Action<TSPlayer,Class, Class> OnClassChange;
		
		//player, currentclass
		public Action<TSPlayer,Class> OnClassMastered;

		public static BooModuleManager ModuleManager { get; set; }

		public override string ToString()
		{
			return $"[ClassDefinition '{Name}']";
		}

		public static List<ClassDefinition> Load(string directoryPath)
		{
			var results = new List<ClassDefinition>();
			var filesAndDefs = new List<Tuple<string, ClassDefinition>>();//needed by LoadScripts.
			var files = Directory.EnumerateFiles(directoryPath, "*.class", SearchOption.AllDirectories);
			
			foreach( var f in files )
			{
				try
				{
					var json = File.ReadAllText(f);
					var def = JsonConvert.DeserializeObject<ClassDefinition>(json);

					if( def.Initialize() )
					{
						results.Add(def);

						var fd = new Tuple<string, ClassDefinition>(f, def);
						filesAndDefs.Add(fd);
					}
				}
				catch(Exception ex)
				{
					LevelingPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
				}
			}
			
			LoadScripts(filesAndDefs);
			
			//filter out duplicate names
			//var classNames = new HashSet<string>(results.Select(cd => cd.Name));
			//var booDefs = loadBooClassDefinitions(classDirectory)
			//				.Where(cd => !classNames.Contains(cd.Name))
			//				.Select(cd => cd);

			//classDefs.AddRange(booDefs);
			//classDefs.ForEach(cd => cd.ValidateAndFix());
			//_classDefinitions = classDefs;
			//_classes = _classDefinitions.Select(cd => new Class(cd)).ToList();

			//if default class file does not exist, we're in an error state
			if( results.Select(cd => cd.Name)
						.FirstOrDefault(n => n == Config.Instance.DefaultClassName) == null )
			{
				LevelingPlugin.Instance.LogPrint($"DefaultClassName: '{Config.Instance.DefaultClassName}' was not found. ", TraceLevel.Error);
			}
			
			return results;
		}
		
		internal bool Initialize()
		{
			ValidateAndFix();
			ResolveClassCurrency();
			ResolveLevelingCurrency();
			PreParseRewardValues();

			return true;//in future, we can check whether the class is actually valid, or needs to be rejected.
		}

		/// <summary>
		/// Attempts to determine the Currency's and Values for the class cost.
		/// </summary>
		private bool ResolveClassCurrency()
		{
			var currencyMgr = BankingPlugin.Instance.Bank.CurrencyManager;

			if( currencyMgr.TryFindCurrencyFromString(CostString, out var costCurrency) )
			{
				if(costCurrency.GetCurrencyConverter().TryParse(CostString, out var costValue))
				{
					Cost = costValue;
					CostCurrency = costCurrency;
					return true;
				}
			}
			
			LevelingPlugin.Instance.LogPrint($"Could not determine currency or value for switching to class '{Name}'.", TraceLevel.Warning);//not an error, in the strict sense of the word.
			LevelingPlugin.Instance.LogPrint($"Ensure that the 'Cost' property has a properly formatted currency string set.", TraceLevel.Info);

			return false;
		}

		/// <summary>
		/// Attempts to determine the Currency's and Values for the levels within the class.
		/// </summary>
		private bool ResolveLevelingCurrency()
		{
			//determine leveling currency
			var currencyMgr = BankingPlugin.Instance.Bank.CurrencyManager;
			
			foreach( var lvl in LevelDefinitions )
			{
				if( string.IsNullOrWhiteSpace(lvl.CurrencyRequired) )
					continue;//no currency value was set

				if( currencyMgr.TryFindCurrencyFromString(lvl.CurrencyRequired, out var lvlCurrency) )
				{
					if( lvlCurrency.GetCurrencyConverter().TryParse(lvl.CurrencyRequired, out var requiredValue) )
					{
						if( LevelingCurrency == null )
						{
							//no leveling currency has been set yet...
							LevelingCurrency = lvlCurrency;
							lvl.ExpRequired = (long)requiredValue;
						}
						else
						{
							//a leveling currency has been set, so we should flag any currency's that do not match the set currency.
							if(lvlCurrency==LevelingCurrency)
								lvl.ExpRequired = (long)requiredValue;
							else
							{
								LevelingPlugin.Instance.LogPrint($"Currency '{lvlCurrency.InternalName}' used in 'CurrencyRequired' in level '{lvl.Name}' in class '{Name}' does not match previously set currency '{LevelingCurrency.InternalName}'. Falling back to 'ExpLevel'.", TraceLevel.Error);
							}
						}
					}
					else
					{
						LevelingPlugin.Instance.LogPrint($"Couldn't parse 'CurrencyRequired' in level '{lvl.Name}' in class '{Name}'. Using 'ExpLevel' instead.", TraceLevel.Error);
					}
				}
				else
				{
					LevelingPlugin.Instance.LogPrint($"Couldn't determine currency type in level '{lvl.Name}' in class '{Name}'. Using 'ExpLevel' instead.", TraceLevel.Error);
					LevelingPlugin.Instance.LogPrint($"Ensure that the 'CurrencyRequired' property has a properly formatted currency string set, or that 'ExpLevel' is set instead.", TraceLevel.Info);
				}
			}

			if(LevelingCurrency==null)
			{
				LevelingPlugin.Instance.LogPrint($"Could not determine a LevelingCurrency for class '{Name}'. Members of this class will be unable to change levels.", TraceLevel.Error);
				LevelingPlugin.Instance.LogPrint($"Ensure that at least one Level has a 'CurrencyRequired' property with a properly formatted currency string set.", TraceLevel.Info);
			}

			return true;//just pass it for now, future iterations can handle this better.
		}

		private void PreParseRewardValues()
		{
			ParsedNpcNameToExpValues.Clear();

			foreach( var kvp in NpcNameToExpReward )
			{
				decimal unitValue;

				//if( currency.GetCurrencyConverter().TryParse(kvp.Value, out unitValue) )
				//{
				//	ParsedNpcNameToExpValues.Add(kvp.Key, unitValue);
				//}
				//else
				//{
				//	Debug.Print($"Failed to parse Npc reward value '{kvp.Key}' for class '{Name}'. Setting value to 0.");
				//}
			}
		}

		///// <summary>
		/////		Preparse Reward strings to numeric values.
		///// </summary>
		///// <param name="currency">Banking.Currency used for Experience.</param>
		//internal void PreParseRewardValues(CurrencyDefinition currency)
		//{
		//	ParsedNpcNameToExpValues.Clear();

		//	foreach( var kvp in NpcNameToExpReward )
		//	{
		//		decimal unitValue;

		//		if( currency.GetCurrencyConverter().TryParse(kvp.Value, out unitValue) )
		//		{
		//			ParsedNpcNameToExpValues.Add(kvp.Key, unitValue);
		//		}
		//		else
		//		{
		//			Debug.Print($"Failed to parse Npc reward value '{kvp.Key}' for class '{Name}'. Setting value to 0.");
		//		}
		//	}
		//}

		/// <summary>
		/// Checks that the ClassDefinition is valid, and it not, attempts to bring it into a valid state.
		/// </summary>
		private void ValidateAndFix()
		{
			var levelNames = new HashSet<string>();
			var duplicateLevelDefinitions = new List<LevelDefinition>();

			foreach( var def in LevelDefinitions )
			{
				if(!levelNames.Add(def.Name))
				{
					LevelingPlugin.Instance.LogPrint($"Class '{Name}' already has a Level named '{def.Name}'. Disabling duplicate level.", TraceLevel.Error);
					duplicateLevelDefinitions.Add(def);
				}
			}

			foreach(var dupDef in duplicateLevelDefinitions)
				LevelDefinitions.Remove(dupDef);
		}

		private bool LinkToScriptAssembly(Assembly assembly)
		{
			if( assembly == null )
				return false;

			if( string.IsNullOrWhiteSpace(ScriptPath) )
				return false;

			var linker = new BooModuleLinker(assembly, ScriptPath, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			//try to link to callbacks...

			//...but these are disabled since I have no idea how these should work
			//def.OnMaximumCurrency = linker["OnMaximumCurrency"]?.TryCreateDelegate<Action<object>>();
			//def.OnNegativeCurrency = linker["OnNegativeCurrency"]?.TryCreateDelegate<Action<object>>();

			OnLevelUp			= linker["OnLevelUp"]?.TryCreateDelegate<Action<TSPlayer,Class,int>>();
			OnLevelDown			= linker["OnLevelDown"]?.TryCreateDelegate<Action<TSPlayer,Class,int>>();
			OnClassChange		= linker["OnClassChange"]?.TryCreateDelegate<Action<TSPlayer,Class,Class>>();
			OnClassMastered		= linker["OnClassMastered"]?.TryCreateDelegate<Action<TSPlayer,Class>>();

			return true;
		}

		//private static void LoadScripts(string basePath, List<ClassDefinition> classDefs)
		private static void LoadScripts(List<Tuple<string,ClassDefinition>> filesAndDefs)
		{
			const string AssemblyNamePrefix = "ClassDef_";

			LevelingPlugin.Instance.LogPrint("Compiling Class scripts...",TraceLevel.Info);

			//get script files paths
			var scriptedDefs = filesAndDefs.Where(d => !string.IsNullOrWhiteSpace(d.Item2.ScriptPath));
			var booScripts = scriptedDefs.Select(d => Path.Combine(Path.GetDirectoryName(d.Item1), d.Item2.ScriptPath))
											.ToList();

			//var scriptedDefs	= classDefs.Where(d => !string.IsNullOrWhiteSpace(d.ScriptPath));
			//var booScripts		= scriptedDefs.Select(d => Path.Combine(basePath, d.ScriptPath))
			//									.ToList();

			//var booScripts = classDefs.Where(d => !string.IsNullOrWhiteSpace(d.ScriptPath))
			//							 .Select(d => Path.Combine(basePath, d.ScriptPath))
			//							 .ToList();

			var newModuleManager = new BooModuleManager(LevelingPlugin.Instance,
													ScriptHelpers.GetReferences(),
													ScriptHelpers.GetDefaultImports(),
													GetEnsuredMethodSignatures());

			newModuleManager.AssemblyNamePrefix = AssemblyNamePrefix;

			foreach( var f in booScripts )
				newModuleManager.Add(f);

			Dictionary<string, CompilerContext> results = null;

			if( ModuleManager != null )
				results = newModuleManager.IncrementalCompile(ModuleManager);
			else
				results = newModuleManager.Compile();

			ModuleManager = newModuleManager;

			//link!
			foreach( var def in scriptedDefs )
			{
				//var fileName = Path.Combine(basePath, def.ScriptPath);
				var fileName = Path.Combine(Path.GetDirectoryName(def.Item1), def.Item2.ScriptPath);

				//if newly compile assembly, examine the context, and try to link to the new assembly
				if( results.TryGetValue(fileName, out var context) )
				{
					var scriptAssembly = context.GeneratedAssembly;

					if( scriptAssembly != null )
					{
						var result = def.Item2.LinkToScriptAssembly(scriptAssembly);

						//if(!result)
						//	//	CustomNpcsPlugin.Instance.LogPrint($"Failed to link {kvp.Key}.", TraceLevel.Info);
					}
				}
				else
				{
					var scriptAssembly = ModuleManager[fileName];

					if( scriptAssembly != null )
					{
						var result = def.Item2.LinkToScriptAssembly(scriptAssembly);

						//if(!result)
						//	//	CustomNpcsPlugin.Instance.LogPrint($"Failed to link {kvp.Key}.", TraceLevel.Info);
					}
				}
			}
		}

		internal static IEnumerable<EnsuredMethodSignature> GetEnsuredMethodSignatures()
		{
			var sigs = new List<EnsuredMethodSignature>()
			{
				new EnsuredMethodSignature("OnLevelDown")
					.AddParameter("player",typeof(TSPlayer))
					.AddParameter("klass",typeof(Class))
					.AddParameter("levelIndex",typeof(int)),

				new EnsuredMethodSignature("OnLevelUp")
					.AddParameter("player",typeof(TSPlayer))
					.AddParameter("klass",typeof(Class))
					.AddParameter("levelIndex",typeof(int)),

				new EnsuredMethodSignature("OnClassChange")
					.AddParameter("player",typeof(TSPlayer))
					.AddParameter("newClass",typeof(Class))
					.AddParameter("oldClass",typeof(Class)),

				new EnsuredMethodSignature("OnClassMastered")
					.AddParameter("player",typeof(TSPlayer))
					.AddParameter("klass",typeof(Class))
			};

			return sigs;
		}
	}
}
