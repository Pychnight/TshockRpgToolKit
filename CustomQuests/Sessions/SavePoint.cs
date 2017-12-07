using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace CustomQuests.Quests
{
	/// <summary>
	/// Tracks a players state within a Quest.
	/// </summary>
	public class SavePoint
	{
		//public DateTime LastSaved { get; set; }
				
		public string PartyName { get; set; }

		//general purpose, tag field??
		public string SaveData { get; set; }

		/// <summary>
		/// User friendly string, which can let party members know what they've done, or how to progress.
		/// </summary>
		public string QuestStatus { get; set; }
		public Color QuestStatusColor { get; set; }
	}
}
