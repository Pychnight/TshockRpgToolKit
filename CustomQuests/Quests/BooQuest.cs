using CustomQuests.Quests;
using CustomQuests.Triggers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomQuests.Quests
{
	public partial class BooQuest : Quest
	{
		private Task questTask { get; set; }
		private CancellationTokenSource CancellationTokenSource;
		protected CancellationToken CancellationToken => CancellationTokenSource.Token;
		//private ConcurrentBag<Trigger> triggers;
		private ConcurrentDictionary<int, Trigger> triggers;
		int nextTriggerId = 1;

		public Party party { get; internal set; }
		public PartyMember leader => party.Leader;
		public TeamManager teams => party.Teams;

		public BooQuest() : base()
		{
			CancellationTokenSource = new CancellationTokenSource();
			//triggers = new ConcurrentBag<Trigger>();
			triggers = new ConcurrentDictionary<int, Trigger>();
		}

		//public BooQuest(QuestInfo definition) : base(definition)
		//{
		//	//Definition = definition;
		//	CancellationTokenSource = new CancellationTokenSource();
		//}

		internal void Run()
		{
			questTask = Task.Run(() => OnRun());
		}

		protected virtual void OnRun()
		{
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

		public override void Complete(bool isSuccess)
		{
			base.Complete(isSuccess);
			CancellationTokenSource.Cancel();
		}

		public override void Update()
		{
			if( IsEnded )
				return;

			var completedTriggers = new List<Trigger>();
			
			foreach(var trigger in triggers.Values)
			{
				trigger.Update();

				if(trigger.IsCompleted)
					completedTriggers.Add(trigger);
			}

			foreach(var ct in completedTriggers)
			{
				triggers.TryRemove(ct.Id, out var removedTrigger);
			}

			base.Update();
		}

		#region Task based DSL

		protected void Delay(int milliseconds)
		{
			Task.Delay(milliseconds,CancellationToken).Wait();
		}

		protected void Delay(TimeSpan timeSpan)
		{
			Task.Delay(timeSpan, CancellationToken).Wait();
		}

		protected Task CreateTask(Action action)
		{
			var t = new Task(action, CancellationToken);
			return t;
		}

		protected bool IsCancellationRequested()
		{
			return CancellationToken.IsCancellationRequested;
		}

		protected Task XTrigger(Trigger trigger)
		{
			return XTrigger(trigger, -1);
		}

		protected Task XTrigger(Trigger trigger, int timeout)
		{
			//var t = Task.Delay(-1);

			trigger.Id = nextTriggerId++;
			
			triggers.TryAdd(trigger.Id, trigger);
			trigger.Task = Task.Run( () =>
			{
				SpinWait.SpinUntil(() => trigger.UpdateImpl(), timeout);
			}, CancellationToken);

			return trigger.Task;
		}

		#endregion
	}
	
}
