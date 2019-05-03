using Newtonsoft.Json;
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
		[JsonProperty(Order = 0)]
		public string ScriptPath { get; set; }

		/// <summary>
		/// Gets or sets a one time cost to cast this level of spell.
		/// </summary>
		[JsonProperty(Order = 1)]
		public string CastingCost { get; set; }

		/// <summary>
		/// Gets or sets an interval of time that must pass between repeat usages for this level of spell.
		/// </summary>
		[JsonProperty(Order = 2)]
		public TimeSpan CastingCooldown { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Gets or sets a cost deducted while charging this level of spell.
		/// </summary>
		[JsonProperty(Order = 3)]
		public string ChargingCost { get; set; }

		/// <summary>
		/// Gets or sets the duration of time required to charge this level of spell.
		/// </summary>
		[JsonProperty(Order = 4)]
		public TimeSpan ChargingDuration { get; set; } = TimeSpan.FromSeconds(3);

		/// <summary>
		/// Gets or sets the interval of time between charging cost deductions for this level of spell.
		/// </summary>
		[JsonProperty(Order = 5)]
		public TimeSpan ChargingCostInterval { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Gets or sets whether this level of spell can be interrupted.
		/// </summary>
		[JsonProperty(Order = 6)]
		public bool CanInterrupt { get; set; } = false;

		/// <summary>
		/// Gets or sets whether the caster is allowed to move while this level of spell is charging or firing.
		/// </summary>
		[JsonProperty(Order = 7)]
		public bool CanCasterMove { get; set; } = false;

		/// <summary>
		/// Gets or sets the number of uses before this spell levels up.
		/// </summary>
		[JsonProperty(Order = 8)]
		public int UsesToLevelUp { get; set; } = 0;
		
		//callbacks
		internal Action<TSPlayer> OnCast { get; set; }

		internal Action<TSPlayer,float> OnCharge { get; set; }

		internal Action<TSPlayer> OnFire { get; set; }

		//hook to notify player about gaining new level?
		//internal Action<TSPlayer> OnLevelUp { get; set; }
	}
}
