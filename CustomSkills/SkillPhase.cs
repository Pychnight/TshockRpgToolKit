namespace CustomSkills
{
	/// <summary>
	/// Represents the various life cycle stages of a CustomSkill.
	/// </summary>
	internal enum SkillPhase
	{
		Failed,
		Completed,
		Cancelled,
		Casting,
		Charging,
		Firing,
		Cooldown
	}
}
