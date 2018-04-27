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
		protected CancellationToken QuestCancellationToken => CancellationTokenSource.Token;
		//private ConcurrentBag<Trigger> triggers;
		private ConcurrentDictionary<int, Trigger> triggers;
		int nextTriggerId = 1;

		public Party party { get; internal set; }
		public PartyMember leader => party.Leader;
		public TeamManager teams => party.Teams;

		public BooQuest() : base()
		{
			CancellationTokenSource = new CancellationTokenSource();
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
			Debug.Print("Cancelling...");
			CancellationTokenSource.Cancel();
		}
		
		public override void Complete(bool isSuccess)
		{
			base.Complete(isSuccess);
			CancellationTokenSource.Cancel();
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach( var ct in triggers.Values )
			{
				ct.Dispose();
			}
		}

		internal override void Update()
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
				ct.Dispose();
			}

			base.Update();
		}

		#region Task based DSL

		protected void Delay(int milliseconds)
		{
			Task.Delay(milliseconds,QuestCancellationToken).Wait();
		}

		protected void Delay(TimeSpan timeSpan)
		{
			Task.Delay(timeSpan, QuestCancellationToken).Wait();
		}

		protected Task CreateTask(Action action)
		{
			return CreateTask(QuestCancellationToken, action);
		}

		protected Task CreateTask(CancellationToken cancellationToken, Action action)
		{
			var t = new Task(action, cancellationToken);
			return t;
		}

		protected bool IsCancellationRequested()
		{
			return QuestCancellationToken.IsCancellationRequested;
		}
		
		//protected Task XTrigger(Trigger trigger, int timeout)
		//{
		//	//var t = Task.Delay(-1);

		//	trigger.Id = nextTriggerId++;
			
		//	triggers.TryAdd(trigger.Id, trigger);
		//	trigger.Task = Task.Run( () =>
		//	{
		//		SpinWait.SpinUntil(() => trigger.UpdateImpl(), timeout);
		//	}, QuestCancellationToken);

		//	return trigger.Task;
		//}

		private void AddTrigger(Trigger trigger, CancellationToken cancellationToken )
		{
			if( trigger.Id != 0 )
				throw new InvalidOperationException("Trigger has already been used.");

			trigger.Id = nextTriggerId++;
			trigger.Task = Task.Run(() =>
			{
				trigger.Signal.Wait(cancellationToken);
			}, cancellationToken);
			
			triggers.TryAdd(trigger.Id, trigger);
		}

		private Task[] AddTrigger(Trigger[] triggers, CancellationToken cancellationToken)
		{
			var tasks = new Task[triggers.Length];
			var i = 0;

			foreach( var trigger in triggers )
			{
				AddTrigger(trigger, cancellationToken);
				tasks[i++] = trigger.Task;
			}

			return tasks;
		}

		protected bool TriggerWaitAll(int timeoutMilliseconds, CancellationToken cancellationToken, params Trigger[] triggers)
		{
			var tasks = AddTrigger(triggers, cancellationToken);
			var result = Task.WaitAll(tasks, timeoutMilliseconds, cancellationToken);
			return result;
		}

		protected bool TriggerWaitAll(int timeoutMilliseconds, params Trigger[] triggers)
		{
			return TriggerWaitAll(timeoutMilliseconds, QuestCancellationToken, triggers);
		}

		protected bool TriggerWaitAll(params Trigger[] triggers)
		{
			return TriggerWaitAll(-1, triggers);
		}
		
		protected int TriggerWaitAny(int timeoutMilliseconds, CancellationToken cancellationToken, params Trigger[] triggers)
		{
			var tasks = AddTrigger(triggers, cancellationToken);
			var result = Task.WaitAny(tasks, timeoutMilliseconds, cancellationToken);
			return result;
		}

		protected int TriggerWaitAny(int timeoutMilliseconds, params Trigger[] triggers)
		{
			return TriggerWaitAny(timeoutMilliseconds, QuestCancellationToken, triggers);
		}

		protected int TriggerWaitAny(params Trigger[] triggers)
		{
			return TriggerWaitAny(-1, triggers);
		}

		#endregion
	}
	
}
