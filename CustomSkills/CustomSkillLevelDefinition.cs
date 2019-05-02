using Newtonsoft.Json;

namespace CustomSkills
{
	[JsonObject(MemberSerialization.OptIn)]
	public class CustomSkillLevelDefinition
	{
		//public int Level { get; set; }

		/// <summary>
		/// Gets or sets a one time cost to cast this level of spell.
		/// </summary>
		[JsonProperty(Order = 0)]
		public string CastingCost { get; set; }

		/// <summary>
		/// Gets or sets an interval of time that must pass between repeat usages for this level of spell.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int CastingCooldown { get; set; }

		/// <summary>
		/// Gets or sets a cost deducted while charging this level of spell.
		/// </summary>
		[JsonProperty(Order = 2)]
		public string ChargingCost { get; set; }

		/// <summary>
		/// Gets or sets the duration of time required to charge this level of spell.
		/// </summary>
		[JsonProperty(Order = 3)]
		public int ChargingDuration { get; set; }

		/// <summary>
		/// Gets or sets the interval of time between charging cost deductions for this level of spell.
		/// </summary>
		[JsonProperty(Order = 4)]
		public int ChargingCostInterval { get; set; }

		/// <summary>
		/// Gets or sets whether this level of spell can be interrupted.
		/// </summary>
		[JsonProperty(Order = 5)]
		public bool CanInterrupt { get; set; } = false;

		/// <summary>
		/// Gets or sets whether the caster is allowed to move while this level of spell is charging or firing.
		/// </summary>
		[JsonProperty(Order = 6)]
		public bool CanCasterMove { get; set; } = false;

		/// <summary>
		/// Gets or sets the number of uses before this spell levels up.
		/// </summary>
		[JsonProperty(Order = 7)]
		public int UsesToLevelUp { get; set; } = 0;
	}
}
