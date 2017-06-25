using System;
using System.Collections.Generic;
using System.Linq;
using CustomQuests.Triggers;
using JetBrains.Annotations;
using NLua;

namespace CustomQuests
{
    /// <summary>
    ///     Represents a quest instance.
    /// </summary>
    public class Quest : IDisposable
    {
        private readonly List<Trigger> _completedTriggers = new List<Trigger>();
        private readonly List<Trigger> _triggers = new List<Trigger>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Quest" /> class with the specified quest info.
        /// </summary>
        /// <param name="questInfo">The quest info, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="questInfo" /> is <c>null</c>.</exception>
        public Quest([NotNull] QuestInfo questInfo)
        {
            QuestInfo = questInfo ?? throw new ArgumentNullException(nameof(questInfo));
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
        public QuestInfo QuestInfo { get; }

        /// <summary>
        ///     Disposes the quest.
        /// </summary>
        public void Dispose()
        {
            foreach (var trigger in _completedTriggers.Concat(_triggers))
            {
                trigger.Dispose();
            }
        }

        /// <summary>
        ///     Adds the specified trigger.
        /// </summary>
        /// <param name="trigger">The trigger to add, which must not be <c>null</c>.</param>
        /// <param name="prioritize"><c>true</c> to prioritize the trigger; otherwise, <c>false</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trigger" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public void AddTrigger([NotNull] Trigger trigger, bool prioritize = false)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException(nameof(trigger));
            }

            if (prioritize)
            {
                _triggers.Insert(0, trigger);
            }
            else
            {
                _triggers.Add(trigger);
            }
        }

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
        /// <returns>The top trigger, or <c>null</c> if there is none.</returns>
        [CanBeNull]
        [LuaGlobal]
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
        ///     Updates the quest.
        /// </summary>
        public void Update()
        {
            if (IsEnded || _triggers.Count == 0)
            {
                return;
            }

            var currentTrigger = _triggers[0];
            currentTrigger.Update();
            if (currentTrigger.IsCompleted)
            {
                _completedTriggers.Add(currentTrigger);
                _triggers.Remove(currentTrigger);
            }
        }
    }
}
