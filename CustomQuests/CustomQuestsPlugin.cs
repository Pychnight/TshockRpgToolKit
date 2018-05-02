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
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

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

		private readonly Dictionary<string, Party> _parties = new Dictionary<string, Party>(StringComparer.OrdinalIgnoreCase);

		private DateTime _lastSave;
		private QuestManager questManager = new QuestManager();
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
		///		Event fired when a chest is unlocked.
		/// </summary>
		public event EventHandler<ChestUnlockedEventArgs> ChestUnlocked;

		/// <summary>
		///     Gets the corresponding session for the specified player.
		/// </summary>
		/// <param name="player">The player, which must not be <c>null</c>.</param>
		/// <returns>The session.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
		public Session GetSession(TSPlayer player)
		{
			if( player == null )
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
			questManager = new QuestManager();

			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
			ServerApi.Hooks.NetSendData.Register(this, OnSendData);
			ServerApi.Hooks.NetGetData.Register(this, onGetData);
			
			GeneralHooks.ReloadEvent += OnReload;
			GetDataHandlers.PlayerTeam += OnPlayerTeam;
			//GetDataHandlers.PlayerSpawn += OnPlayerSpawn;
			GetDataHandlers.TileEdit += OnTileEdit;

			//GetDataHandlers.

			Commands.ChatCommands.RemoveAll(c => c.Names.Contains("p"));
			Commands.ChatCommands.RemoveAll(c => c.Names.Contains("party"));
			Commands.ChatCommands.Add(new Command("customquests.party", P, "p"));
			Commands.ChatCommands.Add(new Command("customquests.party", Party, "party"));
			Commands.ChatCommands.Add(new Command("customquests.quest", Quest, "quest"));
			Commands.ChatCommands.Add(new Command("customquests.tileinfo", TileInfo, "tileinfo"));
		}

		private void OnPostInitialize(EventArgs args)
		{
			load();
		}

		private void load()
		{
			Directory.CreateDirectory("quests");
			if( File.Exists(ConfigPath) )
			{
				_config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
			}
			_sessionManager = new SessionManager(_config, this);

			questManager.LoadQuestInfos(QuestInfosPath);
		}

		private void OnReload(ReloadEventArgs args)
		{
			_sessionManager.OnReload();//abort in play quests
			load();
		}

		//private void OnPlayerSpawn(object sender, GetDataHandlers.SpawnEventArgs args)
		//{
		//	Debug.Print("OnPlayerSpawn");
		//	Debug.Print($"SpawnX: {args.SpawnX}");
		//	Debug.Print($"SpawnY: {args.SpawnY}");

		//	//args.SpawnX += 20;
		//	//args.SpawnY += 20;

		//	var playerIndex = args.Player;

		//	TShock.Players[playerIndex].Spawn(args.SpawnX, args.SpawnY);

		//	args.Handled = true;
		//}

		//private void dummy(GetDataEventArgs args)
		//{
		//	if( args.MsgID == PacketTypes.PlayerTeam)
		//	{
		//		Debug.Print("Viola");
		//	}
		//}

		private void onGetData(GetDataEventArgs args)
		{
			if( args.Handled )
				return;

			switch(args.MsgID)
			{
				case PacketTypes.ChestUnlock:
					using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
					{
						var type = reader.ReadByte();
						var x = reader.ReadInt16();
						var y = reader.ReadInt16();

						if(type==1)
						{
							OnChestUnlock(x, y);
						}
						//type==2 = door unlock
					}
					break;
			}
		}

		void OnChestUnlock(int x, int y)
		{
			Debug.Print($"OnChestUnlock! {x}, {y}");
			ChestUnlocked?.Invoke(this, new ChestUnlockedEventArgs(x, y));
		}

		//private void onGetData(GetDataEventArgs args)
		//{
		//	if( args.Handled )
		//		return;

		//	if( args.MsgID == PacketTypes.PlayerDeathV2 )
		//	{
		//		//based off of MarioE's original code from the Leveling plugin...
		//		//using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
		//		{
		//			var player = TShock.Players[args.Msg.whoAmI];

		//			//player.Spawn(Main.spawnTileX + 20, Main.spawnTileY);

		//			//if( args.MsgID == PacketTypes.PlayerSpawnSelf )
		//			//{
		//			//	Debug.Print("SpawnSelf");
		//			//	return;
		//			//}
		//		}

		//		Debug.Print("PlayerDeathV2");
		//	}

		//	if( args.MsgID == PacketTypes.PlayerSpawn )
		//	{
		//		Debug.Print("Player Spawn!");

		//		//if(disableSpawn)
		//		//{
		//		//	Debug.Print("Disabled spawn.");
		//		//	args.Handled = true;
		//		//	return;
		//		//}

		//		var playerIndex = (int)args.Msg.readBuffer[0];

		//		using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
		//		{
		//			var player = reader.ReadByte();
		//			var spawnX = reader.ReadInt16();
		//			var spawnY = reader.ReadInt16();

		//			//reader.Seek(1, SeekOrigin.Begin);
		//			//reader.Write((short)2409);
		//			//reader.Write((short)249 - 10);

		//			Debug.Print($"Spawn#: {player}");
		//			Debug.Print($"SpawnX: {spawnX}");
		//			Debug.Print($"SpawnY: {spawnY}");

		//			//disableSpawn = true;

		//			//var tp = new TSPlayer(player);

		//			var tp = TShock.Players[player];

		//			Debug.Print($"Name: {tp.Name}");



		//			args.Handled = true;

		//			Task.Run(() =>
		//			{
		//				//tp.TPlayer.SpawnX = spawnX + 100;
		//				//tp.TPlayer.SpawnY = spawnY - 10;

		//				tp.TPlayer.SpawnX = 2409;
		//				tp.TPlayer.SpawnY = 249;

		//				var pos = new Vector2(tp.TPlayer.SpawnX, tp.TPlayer.SpawnY);

		//				Debug.Print("Deferred spawn!");
		//				Task.Delay(2500).Wait();
		//				tp.TPlayer.Teleport(pos);
		//			});
		//		}
		//	}
		//}

		/// <summary>
		///     Disposes the plugin.
		/// </summary>
		/// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sessionManager.Dispose();
				
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
                    //session.SetQuestState(null);
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
		
		private void OnSendData(SendDataEventArgs args)
        {
			//if(args.MsgId == PacketTypes.PlayerSpawnSelf)
			//{
			//	Debug.Print("SpawnSelf!");
			//}

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
			else if( subcommand.Equals("leader", StringComparison.OrdinalIgnoreCase) )
			{
				PartyLeader(args);
			}
			else
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party form <name>.");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party invite <player>.");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party kick <player>.");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}party leave.");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party leader <player>.");
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
            //if (session.CurrentQuest != null)
            //{
            //    player.SendErrorMessage("You cannot kick a player while in a quest.");
            //    return;
            //}

            var inputPlayer = parameters[1];
			//var players = TShock.Utils.FindPlayer(inputPlayer);
			var targetIndex = party.IndexOf(inputPlayer);

            if (targetIndex < 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayer}'.");
                return;
            }
			if( targetIndex == 0 )
			{
				player.SendErrorMessage("You cannot kick yourself from the party.");
				return;
			}

			//if (players.Count > 1)
			//{
			//    TShock.Utils.SendMultipleMatchError(player, players);
			//    return;
			//}

			//var player2 = players[0];
			//if (!party.Contains(player2))
			//{
			//    player.SendErrorMessage($"{player2.Name} is not in the party.");
			//    return;
			//}

			var targetMember = party[targetIndex];
			var session2 = GetSession(targetMember);
            session2.Party = null;
            party.SendData(PacketTypes.PlayerTeam, "", targetMember.Index);
            party.Remove(targetMember.Player);
            party.SendInfoMessage($"{player.Name} kicked {targetMember.Name} from the party.");
            targetMember.SendInfoMessage("You have been kicked from the party.");
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

		private void PartyLeader(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
						
			if( parameters.Count != 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party leader <player>.");
				return;
			}

			var session = GetSession(player);
			var party = session.Party;
			var newLeader = parameters[1];

			if( party == null )
			{
				player.SendErrorMessage("You are not in a party.");
				return;
			}
			if( player != party.Leader.Player)
			{
				player.SendErrorMessage("You are not the party leader.");
				return;
			}

			var newLeaderIndex = party.IndexOf(newLeader);
			if( newLeaderIndex == -1 )
			{
				player.SendErrorMessage($"{newLeader} is not a member of your party.");
				return;
			}

			var result = party.SetLeader(newLeaderIndex);
			if(result)
			{
				player.SendInfoMessage("You are no longer party leader.");
				party.SendInfoMessage($"{newLeader} is now the party leader.");
			}
			else
			{
				player.SendErrorMessage($"Failed to set party leader.");
				return;
			}
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
					session2.QuestStatusManager.Clear();
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

            var availableQuests = session.AvailableQuestNames.Select(s => questManager.First(q => q.Name == s))
                .Where(session.CanSeeQuest).ToList();
            var inputName = string.Join(" ", parameters.Skip(1));
            var questInfo = availableQuests.FirstOrDefault(
                q => q.FriendlyName.Equals(inputName, StringComparison.OrdinalIgnoreCase) || q.Name == inputName);
            if (questInfo == null)
            {
                player.SendErrorMessage($"Invalid quest name '{inputName}'.");
                return;
            }
						
			var path = Path.Combine("quests", questInfo.ScriptPath ?? $"{questInfo.Name}.boo");
			if( !File.Exists(path) )
			{
				player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
				CustomQuestsPlugin.Instance.LogPrint($"Failed to start quest '{questInfo.Name}'. Script file not found at '{path}'.",TraceLevel.Error);
				return;
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
						session2.QuestStatusManager.Clear();
                        session2.CurrentQuest = session.CurrentQuest;
                    }
                }
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
			
			var quests = session.AvailableQuestNames.Where(s => questManager[s] != null).Select(s => questManager[s]);
			var availableQuests = quests.Where(q => session.CanSeeQuest(q) || session.CurrentQuestName == q.Name).ToList();
																		
			//...because sometimes quests are renamed, or removed...
			var validCompletedQuests = session.CompletedQuestNames.Where(q => questManager[q] != null);
			var completedQuests = validCompletedQuests.Select( s => questManager[s] ).ToList();
			
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
            if (questManager.All(q => q.Name != inputName))
            {
                player.SendErrorMessage($"Invalid quest name '{inputName}'.");
                return;
            }

            session2.RevokeQuest(inputName);
            var questInfo = questManager.First(q => q.Name == inputName);
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
            if (questManager.All(q => q.Name != inputName))
            {
                player.SendErrorMessage($"Invalid quest name '{inputName}'.");
                return;
            }

            session2.UnlockQuest(inputName);
            var questInfo = questManager.First(q => q.Name == inputName);
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
				
				foreach( var qs in session.QuestStatusManager )
					player.SendMessage(qs.Text, qs.Color);
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
