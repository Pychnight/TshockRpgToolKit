using CustomQuests.Quests;
using CustomQuests.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Next
{
	public partial class BooQuest : Quest
	{
		internal Task Task { get; set; }
		public Party party { get; internal set; }
		public TeamManager teams => party.Teams;

		public BooQuest() : base()
		{
		}

		public BooQuest(QuestInfo definition) : base(definition)
		{
			//Definition = definition;
		}

		internal void Run()
		{
			Task = Task.Run(() => OnRun());
		}

		protected virtual void OnRun()
		{
			Console.WriteLine("Hello from TestQuest!");

			var wait = new Wait(TimeSpan.FromSeconds(5));
			wait.Action = () =>
			{
				Console.WriteLine("Okay, we out. Peace.");
				Complete(true);
			};

			AddTrigger(wait);
		}
	}
}
