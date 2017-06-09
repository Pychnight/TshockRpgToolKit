using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLua;

namespace CustomQuests.Sessions
{
    /// <summary>
    ///     Holds session information.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Session : IDisposable
    {
        [JsonProperty("AvailableQuests")]
        private readonly List<string> _availableQuestNames = new List<string>();

        [JsonProperty("CompletedQuests")]
        private readonly List<string> _completedQuestNames = new List<string>();

        [JsonProperty("FailedQuests")]
        private readonly List<string> _failedQuestNames = new List<string>();

        /// <summary>
        ///     Gets a read-only view of the available quest names.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public ReadOnlyCollection<string> AvailableQuestNames => _availableQuestNames.AsReadOnly();

        /// <summary>
        ///     Gets a read-only view of the completed quest names.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public ReadOnlyCollection<string> CompletedQuestNames => _completedQuestNames.AsReadOnly();

        /// <summary>
        ///     Gets or sets the current quest.
        /// </summary>
        [CanBeNull]
        public Quest CurrentQuest { get; set; }

        /// <summary>
        ///     Gets a read-only view of the failed quest names.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public ReadOnlyCollection<string> FailedQuestNames => _failedQuestNames.AsReadOnly();

        /// <summary>
        ///     Gets or sets the current Lua instance.
        /// </summary>
        [CanBeNull]
        public Lua Lua { get; set; }

        /// <summary>
        ///     Disposes the session.
        /// </summary>
        public void Dispose()
        {
            CurrentQuest?.Dispose();
            CurrentQuest = null;
            Lua?.Dispose();
            Lua = null;
        }

        /// <summary>
        ///     Unlocks the specified quest name.
        /// </summary>
        /// <param name="questName">The quest name, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="questName" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public void UnlockQuestName([NotNull] string questName)
        {
            if (questName == null)
            {
                throw new ArgumentNullException(nameof(questName));
            }

            _availableQuestNames.Add(questName);
        }

        /// <summary>
        ///     Updates the current quest.
        /// </summary>
        public void UpdateQuest()
        {
            if (CurrentQuest == null)
            {
                return;
            }

            CurrentQuest.Update();
            var questName = CurrentQuest.QuestInfo.Name;
            if (CurrentQuest.IsCompleted)
            {
                _availableQuestNames.Remove(questName);
                _completedQuestNames.Add(questName);
                CurrentQuest?.Dispose();
                CurrentQuest = null;
                Lua?.Dispose();
                Lua = null;
            }
            else if (CurrentQuest.IsFailed)
            {
                _availableQuestNames.Remove(questName);
                _failedQuestNames.Add(questName);
                CurrentQuest?.Dispose();
                CurrentQuest = null;
                Lua?.Dispose();
                Lua = null;
            }
        }
    }
}
