using System;
using System.Collections.ObjectModel;
using System.IO;
using JetBrains.Annotations;
using NLua;
using TShockAPI;

namespace CustomQuests.Sessions
{
    /// <summary>
    ///     Holds session information.
    /// </summary>
    public sealed class Session : IDisposable
    {
        private readonly TSPlayer _player;

        private Lua _currentLua;
        private Quest _currentQuest;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Session" /> class with the specified player and session information.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <param name="sessionInfo">The session information, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="player" /> or <paramref name="sessionInfo" /> is <c>null</c>.
        /// </exception>
        public Session([NotNull] TSPlayer player, SessionInfo sessionInfo)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            SessionInfo = sessionInfo ?? throw new ArgumentNullException(nameof(sessionInfo));
        }

        /// <summary>
        ///     Gets a read-only view of the available quest names.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public ReadOnlyCollection<string> AvailableQuestNames => SessionInfo.AvailableQuestNames.AsReadOnly();

        /// <summary>
        ///     Gets a read-only view of the completed quest names.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public ReadOnlyCollection<string> CompletedQuestNames => SessionInfo.CompletedQuestNames.AsReadOnly();

        /// <summary>
        ///     Gets or sets the current quest.
        /// </summary>
        [CanBeNull]
        public Quest CurrentQuest
        {
            get => _currentQuest;
            set
            {
                _currentQuest = value;
                SessionInfo.CurrentQuestName = _currentQuest?.QuestInfo.Name;
            }
        }

        /// <summary>
        ///     Gets the current quest name.
        /// </summary>
        [CanBeNull]
        public string CurrentQuestName => SessionInfo.CurrentQuestName;

        /// <summary>
        ///     Gets a read-only view of the failed quest names.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public ReadOnlyCollection<string> FailedQuestNames => SessionInfo.FailedQuestNames.AsReadOnly();

        /// <summary>
        ///     Gets the session information.
        /// </summary>
        [NotNull]
        public SessionInfo SessionInfo { get; }

        /// <summary>
        ///     Disposes the session.
        /// </summary>
        public void Dispose()
        {
            CurrentQuest?.Dispose();
            CurrentQuest = null;
            _currentLua?.Dispose();
            _currentLua = null;
        }

        /// <summary>
        ///     Gets the quest state.
        /// </summary>
        /// <returns>The quest state.</returns>
        [LuaGlobal]
        [UsedImplicitly]
        public string GetQuestState() => SessionInfo.CurrentQuestState;

        /// <summary>
        ///     Loads the specified quest information as the current quest.
        /// </summary>
        /// <param name="questInfo">The quest information, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="questInfo" /> is <c>null</c>.</exception>
        public void LoadQuest([NotNull] QuestInfo questInfo)
        {
            if (questInfo == null)
            {
                throw new ArgumentNullException(nameof(questInfo));
            }

            var quest = new Quest(questInfo);
            var lua = new Lua
            {
                ["player"] = _player
            };
            lua.LoadCLRPackage();
            lua.DoString("import('System')");
            lua.DoString("import('CustomQuests', 'CustomQuests.Triggers')");
            LuaRegistrationHelper.TaggedInstanceMethods(lua, quest);
            LuaRegistrationHelper.TaggedInstanceMethods(lua, this);
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(QuestFunctions));

            var path = Path.Combine("quests", $"{questInfo.Name}.lua");
            lua.DoFile(path);
            CurrentQuest = quest;
            _currentLua = lua;
        }

        /// <summary>
        ///     Sets the quest state.
        /// </summary>
        /// <param name="questState">The quest state.</param>
        [LuaGlobal]
        [UsedImplicitly]
        public void SetQuestState([CanBeNull] string questState)
        {
            SessionInfo.CurrentQuestState = questState;
        }

        /// <summary>
        ///     Unlocks the specified quest.
        /// </summary>
        /// <param name="questName">The quest name, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="questName" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public void UnlockQuest([NotNull] string questName)
        {
            if (questName == null)
            {
                throw new ArgumentNullException(nameof(questName));
            }

            SessionInfo.AvailableQuestNames.Add(questName);
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
                SessionInfo.AvailableQuestNames.Remove(questName);
                SessionInfo.CompletedQuestNames.Add(questName);
                CurrentQuest?.Dispose();
                CurrentQuest = null;
                _currentLua?.Dispose();
                _currentLua = null;
            }
            else if (CurrentQuest.IsFailed)
            {
                SessionInfo.AvailableQuestNames.Remove(questName);
                SessionInfo.FailedQuestNames.Add(questName);
                CurrentQuest?.Dispose();
                CurrentQuest = null;
                _currentLua?.Dispose();
                _currentLua = null;
            }
        }
    }
}
