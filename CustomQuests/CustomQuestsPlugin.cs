using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CustomQuests.Quests;
using CustomQuests.Sessions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System.Diagnostics;
using Corruption.PluginSupport;

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

		//private readonly Dictionary<string, OldParty> _parties =
		//    new Dictionary<string, OldParty>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, Party> _parties = new Dictionary<string, Party>(StringComparer.OrdinalIgnoreCase);

		private Config _config = new Config();

        private DateTime _lastSave;
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
        public override string Author => "MarioE, Timothy Barela";

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
        ///     Gets the corresponding session for the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <returns>The session.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
        public Session GetSession(TSPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            return _sessionManager.GetOrCreate(player);
        }

		//compatibility shim.
		internal Session GetSession(PartyMember member)
		{
			return GetSession(member.Player);
		}


        /// <summary>
        ///     Initializes the plugin.
        /// </summary>
        public override void Initialize()
        {
            Directory.CreateDirectory("quests");
			if (File.Exists(ConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            _sessionManager = new SessionManager(_config, this);
            if (File.Exists(QuestInfosPath))
            {
                _questInfos = JsonConvert.DeserializeObject<List<QuestInfo>>(File.ReadAllText(QuestInfosPath));

				Debug.Print("Found the following quest infos:");
				foreach(var qi in _questInfos)
					Debug.Print($"Quest: {qi.FriendlyName}");
            }

            GeneralHooks.ReloadEvent += OnReload;
            GetDataHandlers.PlayerTeam += OnPlayerTeam;
            GetDataHandlers.TileEdit += OnTileEdit;
            ServerApi.Hooks.NetSendData.Register(this, OnSendData);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);

            Commands.ChatCommands.RemoveAll(c => c.Names.Contains("p"));
            Commands.ChatCommands.RemoveAll(c => c.Names.Contains("party"));
            Commands.ChatCommands.Add(new Command("customquests.party", P, "p"));
            Commands.ChatCommands.Add(new Command("customquests.party", Party, "party"));
            Commands.ChatCommands.Add(new Command("customquests.quest", Quest, "quest"));
			Commands.ChatCommands.Add(new Command("customquests.tileinfo", TileInfo, "tileinfo"));
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
                GetDataHandlers.PlayerTeam -= OnPlayerTeam;
                GetDataHandlers.TileEdit -= OnTileEdit;
                ServerApi.Hooks.NetSendData.Deregister(this, OnSendData);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
            }

            base.Dispose(disposing);
        }

        private void LeaveParty(TSPlayer player)
        {
            var session = GetSession(player);
            var party = session.Party;
            if (party == null)
            {
                return;
            }

            var questSession = player == party.Leader.Player ? session : GetSession(party.Leader);
            if (questSession.CurrentQuest != null)
            {
				//var onAbortFunction = session.CurrentLua?["OnAbort"] as LuaFunction;
				//onAbortFunction?.Call();
				//foreach (var player2 in party)
				//{
				//    var session2 = GetSession(player2);
				//    session2.IsAborting = true;
				//}

				try
				{
					var bquest = (BooQuest)session.CurrentQuest;
					bquest.Abort();
				}
				catch( Exception ex )
				{
					TShock.Log.ConsoleInfo("An exception occurred in OnAbort()!");
					TShock.Log.ConsoleInfo(ex.ToString());
				}


				//throw new NotImplementedException("Aborting not implemented yet.");

                party.SendInfoMessage("Aborted quest.");
            }

            if (player == party.Leader.Player)
            {
                foreach (var player2 in party)
                {
                    var session2 = GetSession(player2);
                    session2.Party = null;
                    party.SendData(PacketTypes.PlayerTeam, "", player2.Index);
                }
                _parties.Remove(party.Name);
                party.SendInfoMessage($"{player.Name} disbanded the party.");
            }
            else
            {
                session.Party = null;
                party.SendData(PacketTypes.PlayerTeam, "", player.Index);
                party.Remove(player);
                party.SendInfoMessage($"{player.Name} left the party.");
            }
        }

        private void OnLeave(LeaveEventArgs args)
        {
            if (args.Who < 0 || args.Who >= Main.maxPlayers)
            {
                return;
            }

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

        private void OnPlayerTeam(object sender, GetDataHandlers.PlayerTeamEventArgs args)
        {
            var player = TShock.Players[args.PlayerId];
            if (player != null)
            {
                args.Handled = true;
                var session = GetSession(player);
                player.TPlayer.team = session.Party != null ? 1 : 0;
                player.SendData(PacketTypes.PlayerTeam, "", player.Index);
                player.TPlayer.team = 0;
            }
        }

        private void OnReload(ReloadEventArgs args)
        {
			_sessionManager.OnReload();//abort in play quests
			
            if (File.Exists(ConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            if (File.Exists(QuestInfosPath))
            {
                _questInfos = JsonConvert.DeserializeObject<List<QuestInfo>>(File.ReadAllText(QuestInfosPath));
            }
        }

        private void OnSendData(SendDataEventArgs args)
        {
            if (args.Handled || args.MsgId != PacketTypes.Status)
            {
                return;
            }

            // Just don't send this
            if (args.text.ToString() == "Receiving tile data")
            {
                args.Handled = true;
            }
        }

        private void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs args)
        {
            var player = args.Player;
            if (args.Handled || player.AwaitingTempPoint != -1)
            {
                return;
            }

            var x = args.X;
            var y = args.Y;
            var tile = Main.tile[x, y];
            player.SendInfoMessage($"X: {x}, Y: {y}");
            player.SendInfoMessage($"Type: {tile.type}, FrameX: {tile.frameX}, FrameY: {tile.frameY}");
            player.AwaitingTempPoint = 0;
            args.Handled = true;
            player.SendTileSquare(x, y, 5);
        }

        private void OnGameUpdate(EventArgs args)
        {
            foreach (var player in TShock.Players.Where(p => p?.User != null))
            {
                var session = GetSession(player);
                session.UpdateQuest();
            }

            if (DateTime.UtcNow > _lastSave + _config.SavePeriod)
            {
                _lastSave = DateTime.UtcNow;
                foreach (var player in TShock.Players.Where(p => p?.User != null))
                {
                    var username = player.User?.Name ?? player.Name;
                    var session = GetSession(player);
					//var path = Path.Combine("quests", "sessions", $"{username}.json");
					//File.WriteAllText(path, JsonConvert.SerializeObject(session.SessionInfo, Formatting.Indented));
					_sessionManager.sessionRepository.Save(session.SessionInfo, username);
                }
            }
        }

        private void P(CommandArgs args)
        {
            var player = args.Player;
            if (args.Message.Length < 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}p <message>");
                return;
            }

            if (player.mute)
            {
                player.SendErrorMessage("You are muted.");
                return;
            }

            var session = GetSession(player);
            var party = session.Party;
            if (party == null)
            {
                player.SendErrorMessage("You are not in a party.");
                return;
            }

            var message = args.Message.Substring(2);
            party.SendMessage($"<{player.Name}> {message}", Main.teamColor[1]);
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
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party form <name>.");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party invite <player>.");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party kick <player>.");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party leave.");
            }
        }

        private void PartyForm(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party form <name>.");
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

			//var party = new OldParty(inputName, player);
			var party = new Party(inputName, player);
            _parties[inputName] = party;
            session.Party = party;
            player.TPlayer.team = 1;
            player.SendData(PacketTypes.PlayerTeam, "", player.Index);
            player.TPlayer.team = 0;
            player.SendSuccessMessage($"Formed the '{inputName}' party.");
        }

        private void PartyInvite(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party invite <player>.");
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
            var session2 = GetSession(player2);
            if (session2.Party != null)
            {
                player.SendErrorMessage($"{player2.Name} is already in a party.");
                return;
            }

            player2.SendInfoMessage($"You have been invited to join the '{party.Name}' party.");
            player2.SendInfoMessage("Use /accept or /decline.");
            player2.AwaitingResponse.Add("accept", args2 =>
            {
                if (session.CurrentQuest != null)
                {
                    player.SendErrorMessage("You cannot accept the invite while the party is on a quest.");
                    return;
                }

                session2.Party = party;
                party.SendInfoMessage($"{player2.Name} has joined the party.");
                party.Add(player2);
                foreach (var player3 in party)
                {
                    player3.Player.TPlayer.team = 1;
                    player2.SendData(PacketTypes.PlayerTeam, "", player3.Index);
                    player3.Player.TPlayer.team = 0;
                }
                player2.TPlayer.team = 1;
                party.SendData(PacketTypes.PlayerTeam, "", player2.Index);
                player2.TPlayer.team = 0;
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
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party kick <player>.");
                return;
            }

            var session = GetSession(player);
            var party = session.Party;
            if (party == null)
            {
                player.SendErrorMessage("You are not in a party.");
                return;
            }
            if (party.Leader.Player != player)
            {
                player.SendErrorMessage("You are not the leader of your party.");
                return;
            }
            if (session.CurrentQuest != null)
            {
                player.SendErrorMessage("You cannot kick a player while in a quest.");
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

            var session2 = GetSession(player2);
            session2.Party = null;
            party.SendData(PacketTypes.PlayerTeam, "", player2.Index);
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
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party leave.");
                return;
            }

            var session = GetSession(player);
            var party = session.Party;
            if (party == null)
            {
                player.SendErrorMessage("You are not in a party.");
                return;
            }
            if (session.CurrentQuest != null)
            {
                player.SendErrorMessage("You cannot leave the party while in a quest.");
                return;
            }

            LeaveParty(player);
        }

        private void Quest(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            var isAdmin = player.HasPermission("customquests.quest.admin");
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
            else if (subcommand.Equals("revoke", StringComparison.OrdinalIgnoreCase))
            {
                if (!isAdmin)
                {
                    player.SendErrorMessage("You do not have access to this command.");
                    return;
                }

                QuestRevoke(args);
            }
            else if (subcommand.Equals("unlock", StringComparison.OrdinalIgnoreCase))
            {
                if (!isAdmin)
                {
                    player.SendErrorMessage("You do not have access to this command.");
                    return;
                }

                QuestUnlock(args);
            }
			else if(subcommand.Equals("status",StringComparison.OrdinalIgnoreCase))
			{
				QuestStatus(args);
			}
            else
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest abort.");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest accept <name>.");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest list [page].");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest status.");
				if (isAdmin)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}quest revoke [player] <name>.");
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}quest unlock [player] <name>.");
                }
            }
        }

        private void QuestAbort(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest abort.");
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
                if (party.Leader.Player != player)
                {
                    player.SendErrorMessage("Only the party leader can abort the quest.");
                    return;
                }

                foreach (var player2 in party)
                {
                    var session2 = GetSession(player2);
                    session2.IsAborting = true;
                }

				//throw new NotImplementedException("Aborting not supported yet.");

				//var onAbortFunction = session.CurrentLua?["OnAbort"] as LuaFunction;
				//try
				//{
				//    onAbortFunction?.Call();
				//}
				//catch (Exception ex)
				//{
				//    TShock.Log.ConsoleInfo("An exception occurred in OnAbort: ");
				//    TShock.Log.ConsoleInfo(ex.ToString());
				//}

				//foreach (var player2 in party)
				//{
				//    var session2 = GetSession(player2);
				//    session2.HasAborted = true;
				//}

				//var onAbortFunction = session.CurrentLua?["OnAbort"] as LuaFunction;
				try
				{
					var bquest = (BooQuest)session.CurrentQuest;
					bquest.Abort();
				}
				catch( Exception ex )
				{
					TShock.Log.ConsoleInfo("An exception occurred in OnAbort()!");
					TShock.Log.ConsoleInfo(ex.ToString());
				}

				foreach( var player2 in party )
				{
					var session2 = GetSession(player2);
					session2.HasAborted = true;
				}
				
				party.SendSuccessMessage("Aborted quest.");
            }
            else
            {
                session.IsAborting = true;

				//throw new NotImplementedException("Aborting not supported yet.");
				//var onAbortFunction = session.CurrentLua?["OnAbort"] as LuaFunction;
				//try
				//{
				//    onAbortFunction?.Call();
				//}
				//catch (Exception ex)
				//{
				//    TShock.Log.ConsoleInfo("An exception occurred in OnAbort: ");
				//    TShock.Log.ConsoleInfo(ex.ToString());
				//}

				try
				{
					var bquest = (BooQuest)session.CurrentQuest;
					bquest.Abort();
				}
				catch( Exception ex )
				{
					TShock.Log.ConsoleInfo("An exception occurred in OnAbort()!");
					TShock.Log.ConsoleInfo(ex.ToString());
				}

				session.HasAborted = true;

                player.SendSuccessMessage("Aborted quest.");
            }
        }

        private void QuestAccept(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count < 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest accept <name>.");
                return;
            }

            var session = GetSession(player);
            if (session.CurrentQuest != null)
            {
                player.SendErrorMessage("You are already in a quest. Either abort it or complete it.");
                return;
            }

            var availableQuests = session.AvailableQuestNames.Select(s => _questInfos.First(q => q.Name == s))
                .Where(session.CanSeeQuest).ToList();
            var inputName = string.Join(" ", parameters.Skip(1));
            var questInfo = availableQuests.FirstOrDefault(
                q => q.FriendlyName.Equals(inputName, StringComparison.OrdinalIgnoreCase) || q.Name == inputName);
            if (questInfo == null)
            {
                player.SendErrorMessage($"Invalid quest name '{inputName}'.");
                return;
            }

			var path = "";
			
			if(!string.IsNullOrEmpty(questInfo.ScriptPath) && questInfo.ScriptPath.EndsWith(".boo"))
			{
				Debug.Print("Accepting a boo quest.");
			}
			else
			{
				path = Path.Combine("quests", questInfo.ScriptPath ?? $"{questInfo.Name}.lua");
				if( !File.Exists(path) )
				{
					player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
					return;
				}
			}
			
            var concurrentParties = _parties.Values.Select(p => GetSession(p.Leader))
                .Count(s => s.CurrentQuestName == inputName);
            if (concurrentParties >= questInfo.MaxConcurrentParties)
            {
                player.SendErrorMessage(
                    $"There are too many parties currently performing the quest '{questInfo.FriendlyName}'.");
                return;
            }

            var party = session.Party;
            if (party != null)
            {
                if (party.Leader.Player != player)
                {
                    player.SendErrorMessage("Only the party leader can accept a quest.");
                    return;
                }
                if (party.Select(GetSession).Any(s => !s.AvailableQuestNames.Contains(questInfo.Name) &&
                                                      !s.CompletedQuestNames.Contains(questInfo.Name)) ||
                    party.Select(GetSession).Any(s => !s.CanSeeQuest(questInfo)))
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

                try
                {
                    player.SendSuccessMessage($"Starting quest '{questInfo.FriendlyName}'!");
                    session.LoadQuest(questInfo);

                    foreach (var player2 in party.Where(p => p.Player != player))
                    {
                        player2.SendSuccessMessage($"Starting quest '{questInfo.FriendlyName}'!");
                        var session2 = GetSession(player2);
                        session2.CurrentQuest = session.CurrentQuest;
                    }
                }
                //catch (LuaException ex)
                //{
                //    player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
                //    TShock.Log.ConsoleError(ex.ToString());
                //    TShock.Log.ConsoleError(ex.InnerException?.ToString());
                //}
				catch(Exception ex)
				{
					player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
					TShock.Log.ConsoleError(ex.ToString());
					TShock.Log.ConsoleError(ex.InnerException?.ToString());
				}
            }
            else
            {
                if (questInfo.MinPartySize > 1)
                {
                    player.SendErrorMessage($"Quest requires a party size of {questInfo.MinPartySize}.");
                    return;
                }

                try
                {
                    player.SendSuccessMessage($"Starting quest '{questInfo.FriendlyName}'!");
                    session.LoadQuest(questInfo);
                }
                //catch (LuaException ex)
                //{
                //    player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
                //    TShock.Log.ConsoleError(ex.ToString());
                //    TShock.Log.ConsoleError(ex.InnerException?.ToString());
                //}
				catch( Exception ex )
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
			if( parameters.Count > 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest list [page].");
				return;
			}

			var session = GetSession(player);
			var availableQuests = session.AvailableQuestNames.Select(s => _questInfos.First(q => q.Name == s))
															.Where(q => session.CanSeeQuest(q) || session.CurrentQuestName == q.Name)
															.ToList();
			
			var completedQuests = session.CompletedQuestNames.Select(s => _questInfos.First(q => q.Name == s)).ToList();
            var totalQuestCount = availableQuests.Count + completedQuests.Count;
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
                        ? $"{questInfo.FriendlyName} (IN PROGRESS): {questInfo.Description}"
                        : $"{questInfo.FriendlyName} ({questInfo.Name}): {questInfo.Description}");
                }
                else
                {
                    var questInfo = completedQuests[i - availableQuests.Count];
                    player.SendSuccessMessage($"{questInfo.FriendlyName} (COMPLETED): {questInfo.Description}");
                }
            }
            if (pageNumber != maxPage)
            {
                player.SendSuccessMessage($"Type {Commands.Specifier}quest list {pageNumber + 1} for more.");
            }
        }

        private void QuestRevoke(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2 && parameters.Count != 3)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest revoke [player] <name>.");
                return;
            }

            var player2 = args.Player;
            if (parameters.Count == 3)
            {
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

                player2 = players[0];
            }

            var session2 = GetSession(player2);
            var inputName = parameters.Last();
            if (!session2.AvailableQuestNames.Contains(inputName) && !session2.CompletedQuestNames.Contains(inputName))
            {
                player.SendErrorMessage(
                    $"{(player2 == player ? "You have" : $"{player2.Name} has")} not unlocked quest '{inputName}'.");
                return;
            }
            if (_questInfos.All(q => q.Name != inputName))
            {
                player.SendErrorMessage($"Invalid quest name '{inputName}'.");
                return;
            }

            session2.RevokeQuest(inputName);
            var questInfo = _questInfos.First(q => q.Name == inputName);
            player2.SendSuccessMessage($"Revoked quest '{questInfo.Name}'.");
            if (player2 != player)
            {
                player.SendSuccessMessage($"Revoked quest '{questInfo.Name}' for {player2.Name}.");
            }
        }

        private void QuestUnlock(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2 && parameters.Count != 3)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}quest unlock [player] <name>.");
                return;
            }

            var player2 = args.Player;
            if (parameters.Count == 3)
            {
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

                player2 = players[0];
            }

            var session2 = GetSession(player2);
            var inputName = parameters.Last();
            if (session2.AvailableQuestNames.Contains(inputName))
            {
                player.SendErrorMessage(
                    $"{(player2 == player ? "You have" : $"{player2.Name} has")} already unlocked quest '{inputName}'.");
                return;
            }
            if (session2.CompletedQuestNames.Contains(inputName))
            {
                player.SendErrorMessage(
                    $"{(player2 == player ? "You have" : $"{player2.Name} has")} already completed quest '{inputName}'.");
                return;
            }
            if (_questInfos.All(q => q.Name != inputName))
            {
                player.SendErrorMessage($"Invalid quest name '{inputName}'.");
                return;
            }

            session2.UnlockQuest(inputName);
            var questInfo = _questInfos.First(q => q.Name == inputName);
            player2.SendSuccessMessage($"Unlocked quest '{questInfo.Name}'.");
            if (player2 != player)
            {
                player.SendSuccessMessage($"Unlocked quest '{questInfo.Name}' for {player2.Name}.");
            }
        }

		private void QuestStatus(CommandArgs args)
		{
			var player = args.Player;
			var session = GetSession(player);

			if(session.CurrentQuest==null )
			{
				player.SendErrorMessage("You are not currently on a quest!");
			}
			else
			{
				var isPartyLeader = player == session.Party.Leader.Player;
				var questName	= session.CurrentQuest.QuestInfo.FriendlyName;
				//var questStatus = session.CurrentQuest.QuestStatus ?? "";
				//var color		= session.CurrentQuest.QuestStatusColor;
				var savePoint = session.SessionInfo.GetOrCreateSavePoint(session.SessionInfo.CurrentQuestInfo.Name, isPartyLeader);
				var questStatus = savePoint.QuestStatus;
				var color = savePoint.QuestStatusColor;

				player.SendMessage($"[Quest {questName}] {questStatus}", color);
			}
		}

        private void TileInfo(CommandArgs args)
        {
            var player = args.Player;
            player.AwaitingTempPoint = -1;
            player.SendInfoMessage("Hit a block to get the tile info.");
        }
    }
}
