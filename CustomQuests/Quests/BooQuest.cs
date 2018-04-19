using CustomQuests.Quests;
using CustomQuests.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomQuests.Quests
{
	public partial class BooQuest : Quest
	{
		internal Task Task { get; set; }

		public Party party { get; internal set; }
		public PartyMember leader => party.Leader;
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

		//internal void Abort(TSPlayer player)
		//{
		//	var index = party.IndexOf(player);

		//	if(index>-1)
		//	{
		//		var member = party[index];
		//		OnAbort(member);
		//	}
		//}

		//protected virtual void OnAbort(PartyMember member)
		//{
		//	Debug.Print("OnAbort for ${member.Name}");
		//}

		public void Abort()
		{
			OnAbort();
		}
				
		protected internal virtual void OnAbort()
		{
			Debug.Print($"OnAbort()! for {QuestInfo.Name}");
		}
		
		public void trigger(string threadName, Trigger trigger, Action action)
		{
			trigger.Action = action;

			base.AddTrigger(trigger, false, threadName);
		}

		//public void trigger(Trigger trigger, Action action)
		//{
		//	this.trigger("main", trigger, action);
		//}

		public void AddTrigger(string threadName, Trigger trigger, Action action)
		{
			trigger.Action = action;

			base.AddTrigger(trigger, false, threadName);
		}

		//For boo compatibility, since optional arguments aren't working in boo
		public void AddTrigger(Trigger trigger, bool prioritized )
		{
			AddTrigger(trigger, prioritized);
		}

		//For boo compatibility, since optional arguments aren't working in boo
		public void AddTrigger(Trigger trigger)
		{
			base.AddTrigger(trigger);
		}
	}
}
