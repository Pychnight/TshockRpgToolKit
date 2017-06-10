using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CustomQuests.Sessions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLua.Exceptions;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace CustomQuests
{
    /// <summary>
    ///     Represents the custom quests plugin.
    /// </summary>
    [ApiVersion(2, 1)]
    [UsedImplicitly]
    public sealed class CustomQuestsPlugin : TerrariaPlugin
    {
        private const int QuestsPerPage = 5;

        private static readonly string ConfigPath = Path.Combine("quests", "config.json");
        private static readonly string QuestInfosPath = Path.Combine("quests", "quests.json");

        private Config _config = new Config();
        private List<QuestInfo> _questInfos = new List<QuestInfo>();
        private SessionManager _sessionManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomQuestsPlugin" /> class using the specified Main instance.
        /// </summary>
        /// <param name="game">The Main instance.</param>
        public CustomQuestsPlugin(Main game) : base(game)
        {
            Instance = this;
        }

        /// <summary>
        ///     Gets the custom quests plugin instance.
        /// </summary>
        public static CustomQuestsPlugin Instance { get; private set; }

        /// <summary>
        ///     Gets the author.
        /// </summary>
        public override string Author => "MarioE";

        /// <summary>
        ///     Gets the description.
        /// </summary>
        public override string Description => "Provides a custom quest system.";

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public override string Name => "CustomQuests";

        /// <summary>
        ///     Gets the version.
        /// </summary>
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        ///     Finds the quest information with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The quest information, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [CanBeNull]
        public QuestInfo FindQuestInfo([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _questInfos.Find(q => q.Name == name);
        }

        /// <summary>
        ///     Initializes the plugin.
        /// </summary>
        public override void Initialize()
        {
            Directory.CreateDirectory(Path.Combine("quests", "sessions"));
            if (File.Exists(ConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            _sessionManager = new SessionManager(this, _config);
            if (File.Exists(QuestInfosPath))
            {
                _questInfos = JsonConvert.DeserializeObject<List<QuestInfo>>(File.ReadAllText(QuestInfosPath));
            }

            _questInfos.Add(new QuestInfo {Description = "TestDesc", FriendlyName = "Friendly Name!", Name = "name"});

            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);

            Commands.ChatCommands.Add(new Command("customquests.quest", Quest, "quest"));
        }

        /// <summary>
        ///     Disposes the plugin.
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sessionManager.Dispose();

                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
                File.WriteAllText(QuestInfosPath, JsonConvert.SerializeObject(_questInfos, Formatting.Indented));

                GeneralHooks.ReloadEvent -= OnReload;
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
            }

            base.Dispose(disposing);
        }

        private Session GetSession(TSPlayer player) => _sessionManager.GetOrCreate(player);

        private void OnLeave(LeaveEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player != null)
            {
                _sessionManager.Remove(player);
            }
        }

        private void OnReload(ReloadEventArgs args)
        {
            if (File.Exists(ConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            if (File.Exists(QuestInfosPath))
            {
                _questInfos = JsonConvert.DeserializeObject<List<QuestInfo>>(File.ReadAllText(QuestInfosPath));
            }
        }

        private void OnUpdate(EventArgs args)
        {
            foreach (var player in TShock.Players.Where(p => p != null))
            {
                var session = GetSession(player);
                session.UpdateQuest();
            }
        }

        private void Quest(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            var subcommand = parameters.Count == 0 ? "" : parameters[0];

            if (subcommand.Equals("abort", StringComparison.OrdinalIgnoreCase))
            {
                QuestAbort(args);
            }
            else if (subcommand.Equals("accept", StringComparison.OrdinalIgnoreCase))
            {
                QuestAccept(args);
            }
            else if (subcommand.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                QuestList(args);
            }
            else
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest abort");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest accept <number>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest list [page]");
            }
        }

        private void QuestAbort(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest abort");
                return;
            }

            var session = GetSession(player);
            var quest = session.CurrentQuest;
            if (quest == null)
            {
                player.SendErrorMessage("You have not accepted a quest.");
                return;
            }

            quest.Dispose();
            session.CurrentQuest = null;
            session.SetQuestState(null);
            player.SendSuccessMessage("Aborted quest.");
        }

        private void QuestAccept(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest accept <number>");
                return;
            }

            var session = GetSession(player);
            var availableQuests = session.AvailableQuestNames.Select(s => _questInfos.First(q => q.Name == s)).ToList();
            var inputNumber = parameters[1];
            if (!int.TryParse(inputNumber, out var questNumber) || questNumber <= 0 ||
                questNumber > availableQuests.Count)
            {
                player.SendErrorMessage($"Invalid quest number '{inputNumber}'.");
                return;
            }

            if (session.CurrentQuest != null)
            {
                player.SendErrorMessage("You have already accepted a quest. Either abort it or complete it.");
                return;
            }

            var questInfo = availableQuests[questNumber - 1];
            var path = Path.Combine("quests", $"{questInfo.Name}.lua");
            if (!File.Exists(path))
            {
                player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
                return;
            }

            try
            {
                session.LoadQuest(questInfo);
                player.SendSuccessMessage($"Started quest '{questInfo.FriendlyName}'!");
            }
            catch (LuaException ex)
            {
                player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
                TShock.Log.ConsoleError(ex.ToString());
                TShock.Log.ConsoleError(ex.InnerException?.ToString());
            }
        }

        private void QuestList(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count > 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest list [page]");
                return;
            }

            var session = GetSession(player);
            var availableQuests = session.AvailableQuestNames.Select(s => _questInfos.First(q => q.Name == s)).ToList();
            var completedQuests = session.CompletedQuestNames.Select(s => _questInfos.First(q => q.Name == s)).ToList();
            var failedQuests = session.FailedQuestNames.Select(s => _questInfos.First(q => q.Name == s)).ToList();
            var totalQuestCount = availableQuests.Count + completedQuests.Count + failedQuests.Count;
            var maxPage = (totalQuestCount - 1) / QuestsPerPage + 1;
            var inputPageNumber = parameters.Count == 1 ? "1" : parameters[1];
            if (!int.TryParse(inputPageNumber, out var pageNumber) || pageNumber <= 0 || pageNumber > maxPage)
            {
                player.SendErrorMessage($"Invalid page number '{inputPageNumber}'");
                return;
            }

            if (totalQuestCount == 0)
            {
                player.SendErrorMessage("No quests found.");
                return;
            }

            player.SendSuccessMessage($"Quests (page {pageNumber} out of {maxPage})");
            var offset = (pageNumber - 1) * QuestsPerPage;
            for (var i = offset; i < offset + 5 && i < totalQuestCount; ++i)
            {
                if (i < availableQuests.Count)
                {
                    var questInfo = availableQuests[i];
                    var currentQuestInfo = session.CurrentQuest?.QuestInfo;
                    player.SendInfoMessage(questInfo == currentQuestInfo
                        // ReSharper disable once PossibleNullReferenceException
                        ? $"{questInfo.FriendlyName} (IN PROGRESS): {questInfo.Description}"
                        : $"{questInfo.FriendlyName} ({i + 1}): {questInfo.Description}");
                }
                else if (i < availableQuests.Count + completedQuests.Count)
                {
                    var questInfo = completedQuests[i - availableQuests.Count];
                    player.SendSuccessMessage($"{questInfo.FriendlyName} (COMPLETED): {questInfo.Description}");
                }
                else
                {
                    var questInfo = failedQuests[i - availableQuests.Count - completedQuests.Count];
                    player.SendErrorMessage($"{questInfo.FriendlyName} (FAILED): {questInfo.Description}");
                }
            }
            if (pageNumber != maxPage)
            {
                player.SendSuccessMessage($"Type {Commands.Specifier}quest list {pageNumber + 1} for more.");
            }
        }
    }
}
