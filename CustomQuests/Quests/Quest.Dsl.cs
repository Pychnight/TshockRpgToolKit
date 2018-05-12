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
	public partial class Quest : IDisposable
	{
		//private Task questTask { get; set; }
		//private CancellationTokenSource CancellationTokenSource;
		//protected CancellationToken QuestCancellationToken => CancellationTokenSource.Token;
		//private ConcurrentDictionary<int, Trigger> triggers;
		//int nextTriggerId = 1;

		public Party party { get; internal set; }
		public PartyMember leader => party.Leader;
		public TeamManager teams => party.Teams;
		
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
			var t = new Task(action, cancellationToken, TaskCreationOptions.AttachedToParent);
			return t;
		}

		protected bool IsCancellationRequested()
		{
			return QuestCancellationToken.IsCancellationRequested;
		}
		
		private void AddTrigger(Trigger trigger, CancellationToken cancellationToken )
		{
			if( trigger.Id != 0 )
				throw new InvalidOperationException("Trigger has already been used.");

			trigger.Id = nextTriggerId++;
			//trigger.Task = Task.Run(() =>
			//{
			//	trigger.Signal.Wait(cancellationToken);
			//}, cancellationToken);
			
			trigger.Task = Task.Factory.StartNew(() =>
			{
				trigger.Signal.Wait(cancellationToken);
			}, cancellationToken, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

			
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

		protected bool TriggerWaitAll(TimeSpan timeout, CancellationToken cancellationToken, params Trigger[] triggers)
		{
			return TriggerWaitAll((int)timeout.TotalMilliseconds, cancellationToken, triggers);
		}

		protected bool TriggerWaitAll(int timeoutMilliseconds, CancellationToken cancellationToken, params Trigger[] triggers)
		{
			var tasks = AddTrigger(triggers, cancellationToken);
			var result = Task.WaitAll(tasks, timeoutMilliseconds, cancellationToken);
			return result;
		}

		protected bool TriggerWaitAll(TimeSpan timeout, params Trigger[] triggers)
		{
			return TriggerWaitAll((int)timeout.TotalMilliseconds, triggers);
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

		protected int TriggerWaitAny(TimeSpan timeout, CancellationToken cancellationToken, params Trigger[] triggers)
		{
			return TriggerWaitAny((int)timeout.TotalMilliseconds, cancellationToken, triggers);
		}

		protected int TriggerWaitAny(int timeoutMilliseconds, params Trigger[] triggers)
		{
			return TriggerWaitAny(timeoutMilliseconds, QuestCancellationToken, triggers);
		}

		protected int TriggerWaitAny(TimeSpan timeout, params Trigger[] triggers)
		{
			return TriggerWaitAny((int)timeout.TotalMilliseconds, triggers);
		}

		protected int TriggerWaitAny(params Trigger[] triggers)
		{
			return TriggerWaitAny(-1, triggers);
		}
	}
}
