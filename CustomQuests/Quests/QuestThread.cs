using System;
using System.Collections.Generic;
using CustomQuests.Triggers;
using JetBrains.Annotations;

namespace CustomQuests.Quests
{
    /// <summary>
    ///     Represents a quest thread of triggers.
    /// </summary>
    public sealed class QuestThread : IDisposable
    {
        private readonly List<Trigger> _triggers = new List<Trigger>();

		/// <summary>
		/// Used for debugging. 
		/// </summary>
		public string Name { get; set; }

        /// <summary>
        ///     Disposes the quest thread.
        /// </summary>
        public void Dispose()
        {
            foreach (var trigger in _triggers)
            {
                trigger.Dispose();
            }
            _triggers.Clear();
        }

        /// <summary>
        ///     Adds the specified trigger.
        /// </summary>
        /// <param name="trigger">The trigger to add, which must not be <c>null</c>.</param>
        /// <param name="prioritized"><c>true</c> to prioritize the trigger; otherwise, <c>false</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trigger" /> is <c>null</c>.</exception>
        public void AddTrigger([NotNull] Trigger trigger, bool prioritized = false)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException(nameof(trigger));
            }

            if (prioritized)
            {
                _triggers.Insert(0, trigger);
            }
            else
            {
                _triggers.Add(trigger);
            }
        }

        /// <summary>
        ///     Pops the top trigger.
        /// </summary>
        /// <returns>The top trigger, or <c>null</c> if there is none.</returns>
        [CanBeNull]
        [UsedImplicitly]
        public Trigger PopTrigger()
        {
            if (_triggers.Count == 0)
            {
                return null;
            }

            var result = _triggers[0];
            _triggers.RemoveAt(0);
            return result;
        }

        /// <summary>
        ///     Updates the quest thread.
        /// </summary>
        public void Update()
        {
            if (_triggers.Count == 0)
            {
                return;
            }

            var currentTrigger = _triggers[0];
            currentTrigger.Update();
            if (currentTrigger.IsCompleted)
            {
                currentTrigger.Dispose();
                _triggers.Remove(currentTrigger);
            }
        }
    }
}
