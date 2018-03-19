using System;
using System.Collections.Generic;
using System.Diagnostics;
using Banking;
using Leveling.Levels;
using Leveling.Sessions;
using Newtonsoft.Json;
using TerrariaApi.Server;

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
		///     Gets the list of prerequisite levels.
		/// </summary>
		[JsonProperty("PrerequisiteLevels", Order = 2)]
		public IList<string> PrerequisiteLevelNames { get; internal set; } = new List<string>();

		/// <summary>
		///     Gets the list of prerequisite permissions.
		/// </summary>
		[JsonProperty(Order = 3)]
		public IList<string> PrerequisitePermissions { get; internal set; } = new List<string>();

		/// <summary>
		///     Gets the SEconomy cost to enter this class.
		/// </summary>
		/// <remarks>This member is obsolete, and left for backwards compatibility. Use ExpCost instead.</remarks>
		[JsonProperty(Order = 4)]
		public long SEconomyCost
		{
			get { return (long)InternalCost; }
			set { InternalCost = value; }
		}
				
		/// <summary>
		///		Gets the "Exp" Currency cost to enter this class.
		/// </summary>
		[JsonProperty(Order = 5)]
		public string ExpCost { get; set; }

		/// <summary>
		///     Gets a value indicating whether to allow switching the class after mastery.
		/// </summary>
		[JsonProperty(Order = 6)]
		public bool AllowSwitching { get; internal set; } = true;

		/// <summary>
		///     Gets a value indicating whether to allow switching the class before mastery.
		/// </summary>
		[JsonProperty(Order = 7)]
		public bool AllowSwitchingBeforeMastery { get; internal set; }

		/// <summary>
		///     Gets the EXP multiplier override.
		/// </summary>
		[JsonProperty(Order = 8)]
		public double? ExpMultiplierOverride { get; internal set; }

		/// <summary>
		///     Gets the death penalty multiplier override.
		/// </summary>
		[JsonProperty(Order = 9)]
		public double? DeathPenaltyMultiplierOverride { get; internal set; }

		/// <summary>
		///		Gets the list of commands to execute on first change to a class.
		/// </summary>
		[JsonProperty("CommandsOnClassChangeOnce", Order = 10)]
		public IList<string> CommandsOnClassChangeOnce { get; internal set; } = new List<string>();
		
		/// <summary>
		///     Gets the list of level definitions.
		/// </summary>
		[JsonProperty("Levels", Order = 11)]
		public IList<LevelDefinition> LevelDefinitions { get; internal set; } = new List<LevelDefinition>();

		/// <summary>
		///     Gets the mapping of NPC names to EXP rewards.
		/// </summary>
		[JsonProperty("NpcToExpReward", Order = 12)]
		public Dictionary<string, string> NpcNameToExpReward = new Dictionary<string, string>();

		/// <summary>
		///		Gets a mapping of NPC names to preparsed EXP values.
		/// </summary>
		internal Dictionary<string, decimal> ParsedNpcNameToExpValues { get; set; } = new Dictionary<string, decimal>();
		
		/// <summary>
		///		Currency neutral "backing" cost, for ExpCost and SEconomyCost.
		/// </summary>
		internal double InternalCost { get; set; }
		
		//--- new stuff
		public string DisplayInfo { get; internal set; }

		public Action<object> OnMaximumCurrency;
		public Action<object> OnNegativeCurrency;
		public Action<object> OnLevelUp;
		public Action<object> OnLevelDown;
		public Action<object> OnClassChange;
		public Action<object> OnClassMastered;

		public override string ToString()
		{
			return $"[ClassDefinition '{Name}']";
		}

		/// <summary>
		///		Preparse Reward strings to numeric values.
		/// </summary>
		/// <param name="currency">Banking.Currency used for Experience.</param>
		internal void PreParseRewardValues(CurrencyDefinition currency)
		{
			ParsedNpcNameToExpValues.Clear();

			foreach( var kvp in NpcNameToExpReward )
			{
				decimal unitValue;

				if( currency.GetCurrencyConverter().TryParse(kvp.Value, out unitValue) )
				{
					ParsedNpcNameToExpValues.Add(kvp.Key, unitValue);
				}
				else
				{
					Debug.Print($"Failed to parse Npc reward value '{kvp.Key}' for class '{Name}'. Setting value to 0.");
				}
			}
		}

		internal void ValidateAndFix()
		{
			var plugin = LevelingPlugin.Instance;
			var levelNames = new HashSet<string>();
			var duplicateLevelDefinitions = new List<LevelDefinition>();

			foreach( var def in LevelDefinitions )
			{
				if(!levelNames.Add(def.Name))
				{
					ServerApi.LogWriter.PluginWriteLine(plugin, $"Class '{Name}' already has a Level named '{def.Name}'. Disabling duplicate level.", TraceLevel.Error);
					duplicateLevelDefinitions.Add(def);
				}
			}

			foreach(var dupDef in duplicateLevelDefinitions)
				LevelDefinitions.Remove(dupDef);
		}
	}
}
