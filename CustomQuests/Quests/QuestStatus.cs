using Microsoft.Xna.Framework;

namespace CustomQuests.Quests
{
	/// <summary>
	/// Tracks a players state within a Quest.
	/// </summary>
	public class QuestStatus
	{
		/// <summary>
		/// User friendly string, which can let party members know what they've done, or how to progress.
		/// </summary>
		public string Text { get; set; } = "No status.";
		public Color Color { get; set; } = Color.White;
	}
}
