using System;
using System.Collections.Generic;
using System.Linq;
using CustomQuests.Triggers;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace CustomQuests.Quests
{
    /// <summary>
    ///     Represents a quest instance.
    /// </summary>
    public partial class Quest : IDisposable
    {
		private Task questTask { get; set; }
		private CancellationTokenSource CancellationTokenSource;
		protected CancellationToken QuestCancellationToken => CancellationTokenSource.Token;
		private ConcurrentDictionary<int, Trigger> triggers;
		int nextTriggerId = 1;

		/// <summary>
		///     Gets a value indicating whether the quest is ended.
		/// </summary>
		public bool IsEnded { get; private set; }

		/// <summary>
		///     Gets a value indicating whether the quest is successful.
		/// </summary>
		public bool IsSuccessful { get; private set; }

		/// <summary>
		///     Gets the quest info.
		/// </summary>
		public QuestInfo QuestInfo { get; internal set; }

		/// <summary>
		///  Gets or sets a friendly string informing players of their progress within a quest.
		/// </summary>
		public string QuestStatus { get; set; }
		public Color QuestStatusColor { get; set; } // = Color.White;

		public Quest()
		{
			CancellationTokenSource = new CancellationTokenSource();
			triggers = new ConcurrentDictionary<int, Trigger>();

			QuestStatusColor = Color.White;
		}

        /// <summary>
        ///     Initializes a new instance of the <see cref="Quest" /> class with the specified quest info.
        /// </summary>
        /// <param name="questInfo">The quest info, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="questInfo" /> is <c>null</c>.</exception>
        public Quest(QuestInfo questInfo) : this()
        {
            QuestInfo = questInfo ?? throw new ArgumentNullException(nameof(questInfo));
        }
		
		/// <summary>
		///     Disposes the quest.
		/// </summary>
		public void Dispose()
		{
			foreach( var ct in triggers.Values )
			{
				ct.Dispose();
			}
		}

		internal void Run()
		{
			questTask = Task.Run(() => OnRun());
		}

		//this method gets overridden in boo, by transplanting the modules main method into it.
		protected virtual void OnRun()
		{
		}

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
		
		/// <summary>
		///     Completes the quest.
		/// </summary>
		/// <param name="isSuccess"><c>true</c> to complete successfully; otherwise, <c>false</c>.</param>
		public virtual void Complete(bool isSuccess)
        {
			if( IsEnded )
				return;

            IsEnded = true;
            IsSuccessful = isSuccess;

			CancellationTokenSource.Cancel();
		}
		
        /// <summary>
        ///     Updates the quest.
        /// </summary>
        internal virtual void Update()
        {
			if( IsEnded )
				return;

			updateTriggers();
		}

		private void updateTriggers()
		{
			var completedTriggers = new List<Trigger>();

			foreach( var trigger in triggers.Values )
			{
				trigger.Update();

				if( trigger.IsCompleted )
					completedTriggers.Add(trigger);
			}

			foreach( var ct in completedTriggers )
			{
				triggers.TryRemove(ct.Id, out var removedTrigger);
				ct.Dispose();
			}
		}
	}
}
