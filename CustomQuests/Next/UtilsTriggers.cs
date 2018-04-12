using CustomQuests.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Next
{
	public static class UtilsTriggers
	{
		private static CustomQuests.Party party;//only a placeholder for development.

		internal static void AddTrigger(Trigger trigger)
		{
			throw new NotImplementedException("This is only a dummy stub for development.");
		}

		// Adds a chat response trigger.
		public static void QuickChatResponse(string response, bool onlyLeader, Action callback) {
			var trigger = new ChatResponse(party, response, onlyLeader);
			trigger.Action = callback;
			AddTrigger(trigger);
		}

		// Adds a condition trigger.
		public static void QuickCondition(Func<bool> condition, Action callback) {
			var trigger = new Condition(condition);
			trigger.Action = callback;
			AddTrigger(trigger);
		}

		// Adds a drop items trigger.
		public static void QuickDropItems(string name, int amount, Action callback) {
			var trigger = new DropItems(party, name, amount);
			trigger.Action = callback;
			AddTrigger(trigger);
		}

		// Adds a gather items trigger.
		public static void QuickGatherItems(string name, int amount, Action callback) {
			var trigger = new GatherItems(party, name, amount);
			trigger.Action = callback;
			AddTrigger(trigger);
		}

		// Adds an in area trigger.
		public static void QuickInArea(int x, int y, int x2, int y2, bool isEveryone, Action callback) {
			var trigger = new InArea(party, x, y, x2, y2, isEveryone);
			trigger.Action = callback;
			AddTrigger(trigger);
		}

		// Adds a kill NPCs trigger.
		public static void QuickKillNpcs(string names, int amount, Action callback) {
			var trigger = new KillNpcs(party, names, amount);
			trigger.Action = callback;
			AddTrigger(trigger);
		}

		// Adds a wait trigger.
		public static void QuickWait(int seconds, Action callback) {
			var trigger = new Wait(TimeSpan.FromSeconds(seconds));
			trigger.Action = callback;
			AddTrigger(trigger);
		}
	}
}
