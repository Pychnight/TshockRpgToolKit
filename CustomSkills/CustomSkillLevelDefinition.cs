﻿using Newtonsoft.Json;
using System;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Configuration and stats for an individual level of progression in a CustomSkillDefinition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class CustomSkillLevelDefinition
	{
		/// <summary>
		/// Gets or sets a plugin folder relative path to a script file for this level of skill.
		/// </summary>
		[JsonProperty(Order = 0)]
		public string ScriptPath { get; set; }

		/// <summary>
		/// Gets or sets a one time cost to cast this level of skill.
		/// </summary>
		[JsonProperty(Order = 1)]
		public CustomSkillCost CastingCost { get; set; } = new CustomSkillCost();

		/// <summary>
		/// Gets or sets the duration of time required to cast this level of skill.
		/// </summary>
		[JsonProperty(Order = 2)]
		public TimeSpan CastingDuration { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Gets or sets an interval of time that must pass between repeat usages for this level of skill.
		/// </summary>
		[JsonProperty(Order = 3)]
		public TimeSpan CastingCooldown { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Gets or sets a cost deducted while charging this level of skill.
		/// </summary>
		[JsonProperty(Order = 4)]
		public CustomSkillCost ChargingCost { get; set; } = new CustomSkillCost();

		/// <summary>
		/// Gets or sets the duration of time required to charge this level of skill.
		/// </summary>
		[JsonProperty(Order = 5)]
		public TimeSpan ChargingDuration { get; set; } = TimeSpan.FromSeconds(3);

		/// <summary>
		/// Gets or sets the interval of time between charging cost deductions for this level of skill.
		/// </summary>
		[JsonProperty(Order = 6)]
		public TimeSpan ChargingCostInterval { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Gets or sets whether this skill runs continuously within OnCharge().
		/// </summary>
		[JsonProperty(Order = 7)]
		public bool IsLongRunning { get; set; } = false;

		/// <summary>
		/// Gets or sets whether this level of skill can be interrupted.
		/// </summary>
		[JsonProperty(Order = 8)]
		public bool CanInterrupt { get; set; } = false;

		/// <summary>
		/// Gets or sets whether the caster is allowed to move while this level of skill is charging or firing.
		/// </summary>
		[JsonProperty(Order = 9)]
		public bool CanCasterMove { get; set; } = false;

		/// <summary>
		/// Gets or sets a user readable string containing damage range estimates.
		/// </summary>
		[JsonProperty(Order = 10)]
		public string DamageRangeEstimate { get; set; } = "";

		/// <summary>
		/// Gets or sets the number of uses before this spell levels up. Less than 1 stops leveling.
		/// </summary>
		[JsonProperty(Order = 11)]
		public int UsesToLevelUp { get; set; } = 0;

		//helpers
		public bool CanLevelUp => UsesToLevelUp > 0;

		//callbacks

		//hook for notifying player about gaining new level, or learning a skill
		internal Action<TSPlayer> OnLevelUp { get; set; }

		internal Action<TSPlayer, SkillState> OnCancelled { get; set; }

		internal Func<TSPlayer, SkillState, bool> OnCast { get; set; }

		internal Func<TSPlayer, SkillState, bool> OnCharge { get; set; }

		internal Action<TSPlayer, SkillState> OnFire { get; set; }
	}
}
