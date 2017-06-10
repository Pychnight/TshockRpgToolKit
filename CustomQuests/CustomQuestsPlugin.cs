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

        private readonly Dictionary<string, Party> _parties =
            new Dictionary<string, Party>(StringComparer.OrdinalIgnoreCase);

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
        ///     Initializes the plugin.
        /// </summary>
        public override void Initialize()
        {
            Directory.CreateDirectory(Path.Combine("quests", "sessions"));
            if (File.Exists(ConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            _sessionManager = new SessionManager(_config);
            if (File.Exists(QuestInfosPath))
            {
                _questInfos = JsonConvert.DeserializeObject<List<QuestInfo>>(File.ReadAllText(QuestInfosPath));
            }

            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);

            Commands.ChatCommands.Add(new Command("customquests.party", Party, "qparty"));
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

        private void LeaveParty(TSPlayer player)
        {
            var session = GetSession(player);
            var party = session.Party;
            if (party == null)
            {
                return;
            }

            if (player == party.Leader)
            {
                session.Party = null;
                party.SendInfoMessage($"{player.Name} disbanded the party.");
                foreach (var player2 in party)
                {
                    var session2 = GetSession(player2);
                    session2.Party = null;
                }
                _parties.Remove(party.Name);
            }
            else
            {
                session.Party = null;
                party.Remove(player);
                party.SendInfoMessage($"{player.Name} left the party.");
            }
        }

        private void OnLeave(LeaveEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player != null)
            {
                var session = GetSession(player);
                if (session.Party != null)
                {
                    session.Dispose();
                    session.SetQuestState(null);
                }

                LeaveParty(player);
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

        private void Party(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            var subcommand = parameters.Count == 0 ? "" : parameters[0];
            if (subcommand.Equals("form", StringComparison.OrdinalIgnoreCase))
            {
                PartyForm(args);
            }
            else if (subcommand.Equals("invite", StringComparison.OrdinalIgnoreCase))
            {
                PartyInvite(args);
            }
            else if (subcommand.Equals("kick", StringComparison.OrdinalIgnoreCase))
            {
                PartyKick(args);
            }
            else if (subcommand.Equals("leave", StringComparison.OrdinalIgnoreCase))
            {
                PartyLeave(args);
            }
            else
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}qparty form <name>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}qparty invite <player>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}qparty kick <player>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}qparty leave");
            }
        }

        private void PartyForm(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}qparty form <name>");
                return;
            }

            var session = GetSession(player);
            if (session.CurrentQuest != null)
            {
                player.SendErrorMessage("You cannot form a party while in a quest.");
                return;
            }
            if (session.Party != null)
            {
                player.SendErrorMessage("You are already in a party.");
                return;
            }

            var inputName = parameters[1];
            if (_parties.ContainsKey(inputName))
            {
                player.SendErrorMessage($"A party with the name '{inputName}' already exists.");
                return;
            }

            var party = new Party(inputName, player);
            _parties[inputName] = party;
            session.Party = party;
            player.SendSuccessMessage($"Formed the '{inputName}' party.");
        }

        private void PartyInvite(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}qparty invite <player>");
                return;
            }

            var session = GetSession(player);
            if (session.CurrentQuest != null)
            {
                player.SendErrorMessage("You cannot invite a player while in a quest.");
                return;
            }

            var party = session.Party;
            if (party == null)
            {
                player.SendErrorMessage("You are not in a party.");
                return;
            }

            var inputPlayer = parameters[1];
            var players = TShock.Utils.FindPlayer(inputPlayer);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayer}'.");
                return;
            }
            if (players.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var player2 = players[0];
            if (party.Contains(player2))
            {
                player.SendErrorMessage($"{player2.Name} is already in the party.");
                return;
            }

            player2.SendInfoMessage($"You have been invited to join the '{party.Name}' party.");
            player2.SendInfoMessage("Use /accept or /decline.");
            player2.AwaitingResponse.Add("accept", args2 =>
            {
                party.SendInfoMessage($"{player2.Name} has joined the party.");
                party.Add(player2);
                player2.SendSuccessMessage($"Joined the '{party.Name}' party.");
                player2.AwaitingResponse.Remove("decline");
            });
            player2.AwaitingResponse.Add("decline", args2 =>
            {
                player.SendInfoMessage($"{player2.Name} has declined your invitation.");
                player2.SendSuccessMessage("Declined the invitation.");
                player2.AwaitingResponse.Remove("accept");
            });
            player.SendSuccessMessage($"Sent invitation to {player2.Name}.");
        }

        private void PartyKick(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}qparty invite <player>");
                return;
            }

            var session = GetSession(player);
            var party = session.Party;
            if (party == null)
            {
                player.SendErrorMessage("You are not in a party.");
                return;
            }
            if (party.Leader != player)
            {
                player.SendErrorMessage("You are not the leader of your party.");
                return;
            }

            var inputPlayer = parameters[1];
            var players = TShock.Utils.FindPlayer(inputPlayer);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayer}'.");
                return;
            }
            if (players.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var player2 = players[0];
            if (!party.Contains(player2))
            {
                player.SendErrorMessage($"{player2.Name} is not in the party.");
                return;
            }
            if (player == player2)
            {
                player.SendErrorMessage("You cannot kick yourself from the party.");
                return;
            }

            party.Remove(player2);
            party.SendInfoMessage($"{player.Name} kicked {player2.Name} from the party.");
            player2.SendInfoMessage("You have been kicked from the party.");
        }

        private void PartyLeave(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}qparty leave");
                return;
            }

            var session = GetSession(player);
            var party = session.Party;
            if (party == null)
            {
                player.SendErrorMessage("You are not in a party.");
                return;
            }

            LeaveParty(player);
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
                player.SendErrorMessage("You are not in a quest.");
                return;
            }

            var party = session.Party;
            if (party != null)
            {
                if (party.Leader != player)
                {
                    player.SendErrorMessage("Only the party leader can abort the quest.");
                    return;
                }
                foreach (var player2 in party)
                {
                    var session2 = GetSession(player2);
                    session2.Dispose();
                    session2.SetQuestState(null);
                }
                party.SendSuccessMessage("Aborted quest.");
            }
            else
            {
                session.Dispose();
                session.SetQuestState(null);
                player.SendSuccessMessage("Aborted quest.");
            }
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
            if (session.CurrentQuest != null)
            {
                player.SendErrorMessage("You are already in a quest. Either abort it or complete it.");
                return;
            }

            var availableQuests = session.AvailableQuestNames.Select(s => _questInfos.First(q => q.Name == s)).ToList();
            var inputNumber = parameters[1];
            if (!int.TryParse(inputNumber, out var questNumber) || questNumber <= 0 ||
                questNumber > availableQuests.Count)
            {
                player.SendErrorMessage($"Invalid quest number '{inputNumber}'.");
                return;
            }

            var questInfo = availableQuests[questNumber - 1];
            var path = Path.Combine("quests", $"{questInfo.Name}.lua");
            if (!File.Exists(path))
            {
                player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
                return;
            }

            var party = session.Party;
            if (party != null)
            {
                if (party.Leader != player)
                {
                    player.SendErrorMessage("Only the party leader can accept a quest.");
                    return;
                }
                if (party.Select(GetSession)
                    .Any(session2 => !session2.AvailableQuestNames.Contains(questInfo.Name)))
                {
                    player.SendErrorMessage($"Not everyone can start the quest '{questInfo.FriendlyName}'.");
                    return;
                }
                if (party.Count < questInfo.MinPartySize)
                {
                    player.SendErrorMessage($"Quest requires a larger party size of {questInfo.MinPartySize}.");
                    return;
                }
                if (party.Count > questInfo.MaxPartySize)
                {
                    player.SendErrorMessage($"Quest requires a smaller party size of {questInfo.MaxPartySize}.");
                    return;
                }

                foreach (var player2 in party)
                {
                    var session2 = GetSession(player2);
                    try
                    {
                        session2.LoadQuest(questInfo.Name);
                        player2.SendSuccessMessage($"Started quest '{questInfo.FriendlyName}'!");
                    }
                    catch (LuaException ex)
                    {
                        player2.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
                        TShock.Log.ConsoleError(ex.ToString());
                        TShock.Log.ConsoleError(ex.InnerException?.ToString());
                    }
                }
            }
            else
            {
                if (questInfo.MinPartySize > 1)
                {
                    player.SendErrorMessage("Quest requires a party.");
                    return;
                }

                try
                {
                    session.LoadQuest(questInfo.Name);
                    player.SendSuccessMessage($"Started quest '{questInfo.FriendlyName}'!");
                }
                catch (LuaException ex)
                {
                    player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
                    TShock.Log.ConsoleError(ex.ToString());
                    TShock.Log.ConsoleError(ex.InnerException?.ToString());
                }
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
                    player.SendInfoMessage(questInfo.Name == session.CurrentQuestName
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
