namespace CustomSkills
{
	/// <summary>
	/// Represents the various life cycle stages of a CustomSkill.
	/// </summary>
	internal enum SkillPhase
	{
		Failed = -1,
		None = 0,
		Casting,
		Charging,
		Firing,
		Cooldown,
		Completed = 0,
		Cancelled = 0,
	}
}
