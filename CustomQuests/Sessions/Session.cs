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
                SessionInfo.CurrentQuestName = _currentQuest?.Name;
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
        ///     Gets or sets the party.
        /// </summary>
        [CanBeNull]
        public Party Party { get; set; }

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
        /// <returns>The state.</returns>
        [LuaGlobal]
        [UsedImplicitly]
        public string GetQuestState() => SessionInfo.CurrentQuestState;

        /// <summary>
        ///     Loads the quest with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        public void LoadQuest([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var quest = new Quest(name);
            var lua = new Lua {["party"] = Party ?? new Party(_player.Name, _player)};
            lua.LoadCLRPackage();
            lua.DoString("import('System')");
            lua.DoString("import('CustomQuests', 'CustomQuests.Triggers')");
            LuaRegistrationHelper.TaggedInstanceMethods(lua, quest);
            LuaRegistrationHelper.TaggedInstanceMethods(lua, this);
            LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(QuestFunctions));

            var path = Path.Combine("quests", $"{name}.lua");
            lua.DoFile(path);
            CurrentQuest = quest;
            _currentLua = lua;
        }

        /// <summary>
        ///     Sets the quest state.
        /// </summary>
        /// <param name="state">The state.</param>
        [LuaGlobal]
        [UsedImplicitly]
        public void SetQuestState([CanBeNull] string state)
        {
            SessionInfo.CurrentQuestState = state;
        }

        /// <summary>
        ///     Unlocks the quest with the specified name.
        /// </summary>
        /// <param name="name">The quest name, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public void UnlockQuest([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            SessionInfo.AvailableQuestNames.Add(name);
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
            if (CurrentQuest.IsEnded)
            {
                SessionInfo.AvailableQuestNames.Remove(CurrentQuestName);
                if (CurrentQuest.IsSuccessful)
                {
                    SessionInfo.CompletedQuestNames.Add(CurrentQuestName);
                }
                else
                {
                    SessionInfo.FailedQuestNames.Add(CurrentQuestName);
                }

                CurrentQuest?.Dispose();
                CurrentQuest = null;
                _currentLua?.Dispose();
                _currentLua = null;
            }
        }
    }
}
