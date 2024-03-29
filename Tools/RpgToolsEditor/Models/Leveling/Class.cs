﻿//using Banking;
//using Leveling.Levels;
//using Leveling.Sessions;
using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
//using TerrariaApi.Server;

namespace RpgToolsEditor.Models.Leveling
{
	/// <summary>
	///     Represents a class definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Class : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New Class";

		/// <summary>
		///     Gets the name.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty("Name", Order = 0)]
		public string Name
		{
			get => name;
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		/// <summary>
		///     Gets the display name.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty("DisplayName", Order = 1)]
		public string DisplayName { get; set; }

		/// <summary>
		///     Gets the list of prerequisite levels.
		/// </summary>
		[Category("Prerequisites")]
		[JsonProperty("PrerequisiteLevels", Order = 2)]
		public BindingList<StringHolder> PrerequisiteLevelNames { get; set; } = new BindingList<StringHolder>();

		/// <summary>
		///     Gets the list of prerequisite permissions.
		/// </summary>
		[Category("Prerequisites")]
		[JsonProperty(Order = 3)]
		public BindingList<StringHolder> PrerequisitePermissions { get; set; } = new BindingList<StringHolder>();

		/// <summary>
		///		Gets the Currency cost to enter this class.
		/// </summary>
		[JsonProperty(Order = 6)]
		[Category("Currency")]
		public string Cost { get; set; }

		/// <summary>
		///     Gets a value indicating whether to allow switching the class after mastery.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 7)]
		public bool AllowSwitching { get; set; } = true;

		/// <summary>
		///     Gets a value indicating whether to allow switching the class before mastery.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 8)]
		public bool AllowSwitchingBeforeMastery { get; set; }

		/// <summary>
		///     Gets the EXP multiplier override.
		/// </summary>
		[JsonProperty(Order = 9)]
		[Category("Multipliers")]
		[Description("Optional Multiplier for changing exp rates per class. Leave blank to ignore.")]
		public double? ExpMultiplierOverride { get; set; }

		/// <summary>
		///     Gets the death penalty multiplier override.
		/// </summary>
		[JsonProperty(Order = 10)]
		[Category("Multipliers")]
		public double? DeathPenaltyMultiplierOverride { get; set; }

		/// <summary>
		///		Gets the list of commands to execute on first change to a class.
		/// </summary>
		[JsonProperty("CommandsOnClassChangeOnce", Order = 11)]
		[Category("Basic Properties")]
		public BindingList<StringHolder> CommandsOnClassChangeOnce { get; set; } = new BindingList<StringHolder>();

		/// <summary>
		///     Gets the list of level definitions.
		/// </summary>
		[JsonProperty("Levels", Order = 12)]
		[Browsable(false)]
		[Category("Basic Properties")]
		public List<Level> LevelDefinitions { get; set; } = new List<Level>();

		/// <summary>
		///     Gets the mapping of NPC names to EXP rewards.
		/// </summary>
		[JsonProperty("NpcToExpReward", Order = 13)]
		public Dictionary<string, string> NpcNameToExpReward = new Dictionary<string, string>();

		///// <summary>
		/////		Gets a mapping of NPC names to preparsed EXP values.
		///// </summary>
		//internal Dictionary<string, decimal> ParsedNpcNameToExpValues { get; set; } = new Dictionary<string, decimal>();

		///// <summary>
		/////		Currency neutral "backing" cost, for ExpCost and SEconomyCost.
		///// </summary>
		//internal double InternalCost { get; set; }

		////--- new stuff
		//public string DisplayInfo { get; internal set; }

		public Class()
		{
		}

		public Class(Class other)
		{
			Name = other.Name;
			DisplayName = other.DisplayName;
			PrerequisiteLevelNames = other.PrerequisiteLevelNames.DeepClone();
			PrerequisitePermissions = other.PrerequisitePermissions.DeepClone();
			Cost = other.Cost;
			AllowSwitching = other.AllowSwitching;
			AllowSwitchingBeforeMastery = other.AllowSwitchingBeforeMastery;
			ExpMultiplierOverride = other.ExpMultiplierOverride;
			DeathPenaltyMultiplierOverride = other.DeathPenaltyMultiplierOverride;
			CommandsOnClassChangeOnce = other.CommandsOnClassChangeOnce.DeepClone();
			LevelDefinitions = other.LevelDefinitions.Select(ld => new Level(ld)).ToList();
			NpcNameToExpReward = new Dictionary<string, string>(other.NpcNameToExpReward);
		}

		public override string ToString() => $"[ClassDefinition '{Name}']";

		public object Clone() => new Class(this);
	}

	public static class BindingListExtensions
	{
		public static BindingList<StringHolder> DeepClone(this BindingList<StringHolder> src)
		{
			var cloned = new BindingList<StringHolder>(src.Select(sh => (StringHolder)sh.Clone())
															.ToList());

			return cloned;
		}

		public static BindingList<TerrariaItemStringHolder> DeepClone(this BindingList<TerrariaItemStringHolder> src)
		{
			var cloned = new BindingList<TerrariaItemStringHolder>(src.Select(sh => (TerrariaItemStringHolder)sh.Clone())
																		.ToList());

			return cloned;
		}
	}
}
