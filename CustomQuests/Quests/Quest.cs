using System;
using System.Collections.Generic;
using System.Linq;
using CustomQuests.Triggers;
using JetBrains.Annotations;
using NLua;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CustomQuests.Quests
{
    /// <summary>
    ///     Represents a quest instance.
    /// </summary>
    public class Quest : IDisposable
    {
		protected readonly Dictionary<string, QuestThread> _threads = new Dictionary<string, QuestThread>
        {
            ["main"] = new QuestThread()
        };

		public Quest()
		{
		}

        /// <summary>
        ///     Initializes a new instance of the <see cref="Quest" /> class with the specified quest info.
        /// </summary>
        /// <param name="questInfo">The quest info, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="questInfo" /> is <c>null</c>.</exception>
        public Quest([NotNull] QuestInfo questInfo)
        {
            QuestInfo = questInfo ?? throw new ArgumentNullException(nameof(questInfo));

			QuestStatusColor = Color.White;
        }

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
        [NotNull]
        public QuestInfo QuestInfo { get; internal set; }

		/// <summary>
		///  Gets or sets a friendly string informing players of their progress within a quest.
		/// </summary>
		public string QuestStatus { get; set; }

		public Color QuestStatusColor { get; set; } // = Color.White;

        /// <summary>
        ///     Disposes the quest.
        /// </summary>
        public void Dispose()
        {
            foreach (var thread in _threads.Values)
            {
                thread.Dispose();
            }
            _threads.Clear();
        }

        /// <summary>
        ///     Adds the specified trigger.
        /// </summary>
        /// <param name="trigger">The trigger to add, which must not be <c>null</c>.</param>
        /// <param name="prioritized"><c>true</c> to prioritize the trigger; otherwise, <c>false</c>.</param>
        /// <param name="threadName">The thread name, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="trigger" /> or <paramref name="threadName" />is <c>null</c>.
        /// </exception>
        [LuaGlobal]
        [UsedImplicitly]
        public void AddTrigger([NotNull] Trigger trigger, bool prioritized = false, string threadName = "main")
        {
            if (trigger == null)
            {
                throw new ArgumentNullException(nameof(trigger));
            }

            if (!_threads.TryGetValue(threadName, out var thread))
            {
                thread = new QuestThread();
                _threads[threadName] = thread;
            }

			Debug.Print($"Trigger added to thread '{threadName}'. [{trigger.GetType()}] ");

			thread.AddTrigger(trigger, prioritized);
			thread.Name = threadName;
        }

		//[LuaGlobal]
		//public void AddTriggerX(string threadName, Trigger trigger, LuaFunction callback)
		//{
		//	trigger.Callback = callback;

		//	AddTrigger(trigger, false, threadName);
		//}

        /// <summary>
        ///     Completes the quest.
        /// </summary>
        /// <param name="isSuccess"><c>true</c> to complete successfully; otherwise, <c>false</c>.</param>
        [LuaGlobal]
        [UsedImplicitly]
        public void Complete(bool isSuccess)
        {
            IsEnded = true;
            IsSuccessful = isSuccess;
        }

        /// <summary>
        ///     Pops the top trigger.
        /// </summary>
        /// <param name="threadName">The thread name, which must be valid and not <c>null</c>.</param>
        /// <returns>The top trigger, or <c>null</c> if there is none.</returns>
        /// <exception cref="ArgumentException"><paramref name="threadName" /> is invalid.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="threadName" /> is <c>null</c>.</exception>
        [CanBeNull]
        [LuaGlobal]
        [UsedImplicitly]
        public Trigger PopTrigger([NotNull] string threadName = "main")
        {
            if (threadName == null)
            {
                throw new ArgumentNullException(nameof(threadName));
            }
            if (!_threads.TryGetValue(threadName, out var thread))
            {
                throw new ArgumentException("Thread name is invalid.", nameof(threadName));
            }

            return thread.PopTrigger();
        }

        /// <summary>
        ///     Updates the quest.
        /// </summary>
        public void Update()
        {
            if (IsEnded)
            {
                return;
            }

            var threads = _threads.Values.ToList();
            foreach (var thread in threads)
            {
				//Debug.Print($"Updating thread {thread.Name}.");
				thread.Update();
            }
        }
    }
}
