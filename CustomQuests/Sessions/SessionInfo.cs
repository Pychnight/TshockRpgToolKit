using System.Collections.Generic;
using CustomQuests.Quests;
using JetBrains.Annotations;

namespace CustomQuests.Sessions
{
    /// <summary>
    ///     Represents information about a quest.
    /// </summary>
    public sealed class SessionInfo
    {
        /// <summary>
        ///     Gets the available quest names.
        /// </summary>
        [NotNull]
        public HashSet<string> AvailableQuestNames { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets the completed quest names.
        /// </summary>
        [NotNull]
        public HashSet<string> CompletedQuestNames { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets or sets the current quest info.
        /// </summary>
        [CanBeNull]
        public QuestInfo CurrentQuestInfo { get; set; }

        /// <summary>
        ///     Gets or sets the current quest state.
        /// </summary>
        [CanBeNull]
        public string CurrentQuestState { get; set; }

        /// <summary>
        ///     Gets the repeated quest names.
        /// </summary>
        [NotNull]
        public Dictionary<string, int> RepeatedQuestNames { get; } = new Dictionary<string, int>();
    }
}
