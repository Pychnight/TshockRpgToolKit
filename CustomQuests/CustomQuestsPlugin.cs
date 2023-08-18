﻿using Corruption.PluginSupport;
using CustomQuests.Configuration;
using CustomQuests.Quests;
using CustomQuests.Sessions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
	public sealed partial class CustomQuestsPlugin : TerrariaPlugin
	{
		private const int QuestsPerPage = 5;

		private static readonly string ConfigPath = Path.Combine("quests", "config.json");
		private static readonly string QuestInfosPath = Path.Combine("quests", "quests.json");

		internal Config _config = new Config();
		private readonly Dictionary<string, Party> _parties = new Dictionary<string, Party>(StringComparer.OrdinalIgnoreCase);
		private DateTime _lastSave;
		internal QuestLoader QuestLoader;
		internal QuestRunner QuestRunner;
		internal SessionManager _sessionManager;

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
		///		Event fired when an NPC is caught.
		/// </summary>
		public event EventHandler<CatchNpcEventArgs> CatchNpc;

		/// <summary>
		///		Event fired when an NPC is released.
		/// </summary>
		public event EventHandler<ReleaseNpcEventArgs> ReleaseNpc;

		/// <summary>
		///		Event fired when a chest is unlocked.
		/// </summary>
		public event EventHandler<ChestUnlockedEventArgs> ChestUnlocked;

		/// <summary>
		///		Event fired when an item stack in a chest is changed.
		/// </summary>
		public event EventHandler<ChestItemChangedEventArgs> ChestItemChanged;

		/// <summary>
		///		Event fired when a sign is read.
		/// </summary>
		public event EventHandler<SignReadEventArgs> SignRead;

		/// <summary>
		///		Event fired when a sign is changed.
		/// </summary>
		public event EventHandler<SignChangedEventArgs> SignChanged;

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
		internal Session GetSession(PartyMember member) => GetSession(member.Player);

		/// <summary>
		///     Initializes the plugin.
		/// </summary>
		public override void Initialize()
		{
			QuestLoader = new QuestLoader();
			QuestRunner = new QuestRunner(QuestLoader);

			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
			ServerApi.Hooks.NetSendData.Register(this, OnSendData);
			ServerApi.Hooks.NetGetData.Register(this, onGetData);

			GeneralHooks.ReloadEvent += OnReload;
			GetDataHandlers.PlayerTeam += OnPlayerTeam;
			//GetDataHandlers.PlayerSpawn += OnPlayerSpawn;
			GetDataHandlers.TileEdit += OnTileEdit;

			Commands.ChatCommands.RemoveAll(c => c.Names.Contains("p"));
			Commands.ChatCommands.RemoveAll(c => c.Names.Contains("party"));
			Commands.ChatCommands.Add(new Command("customquests.party", P, "p"));
			Commands.ChatCommands.Add(new Command("customquests.party", Party, "party"));
			Commands.ChatCommands.Add(new Command("customquests.quest", Quest, "quest"));
			Commands.ChatCommands.Add(new Command("customquests.tileinfo", TileInfo, "tileinfo"));
		}

		private void OnPostInitialize(EventArgs args) => load();

		private void load()
		{
			_config = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);

			//_sessionManager = new SessionManager(_config);

			QuestLoader.LoadQuests(QuestInfosPath);

			//new session manager must be initialized after quest loader loads the quest infos, since it requires them as well.

			//workaround, to preserve parties but still react to changed db config.
			if (_sessionManager == null)
			{
				_sessionManager = new SessionManager(_config);
			}
			else
			{
				_sessionManager.UseDatabase(_config);//only refresh db, but try to preserve sessions & parties...
			}
		}

		private void OnReload(ReloadEventArgs args)
		{
			_sessionManager.OnReload();//abort in play quests
									   //_parties.Clear();

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

		private void onGetData(GetDataEventArgs args)
		{
			if (args.Handled)
				return;

			switch (args.MsgID)
			{
				case PacketTypes.CatchNPC:
					using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
					{
						var npcId = reader.ReadInt16();
						//var who = reader.ReadByte();

						onCatchNpc(args.Msg.whoAmI, npcId);
					}
					break;

				case PacketTypes.ReleaseNPC:
					using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
					{
						var x = reader.ReadInt32();
						var y = reader.ReadInt32();
						var npcType = reader.ReadInt16();
						var style = reader.ReadByte();

						onReleaseNpc(args.Msg.whoAmI, x, y, npcType, style);
					}
					break;

				case PacketTypes.ChestUnlock:
					using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
					{
						var type = reader.ReadByte();
						var x = reader.ReadInt16();
						var y = reader.ReadInt16();

						if (type == 1)
						{
							onChestUnlock(args.Msg.whoAmI, x, y);
						}
						//type==2 = door unlock
					}
					break;

				case PacketTypes.ChestItem:
					using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
					{
						var chestId = reader.ReadInt16();
						var itemSlot = reader.ReadByte();
						var stack = reader.ReadInt16();
						var prefix = reader.ReadInt16();
						var netId = reader.ReadInt16();

						onChestItem(args.Msg.whoAmI, chestId, itemSlot, stack, prefix, netId);
					}
					break;

				case PacketTypes.SignRead:
					using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
					{
						var x = reader.ReadInt16();
						var y = reader.ReadInt16();

						onSignRead(args.Msg.whoAmI, x, y);
					}
					break;

				case PacketTypes.SignNew:
					using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
					{
						var signId = reader.ReadInt16();
						var x = reader.ReadInt16();
						var y = reader.ReadInt16();
						var txt = reader.ReadString();
						//var playerId = reader.ReadByte();

						onSignChanged(args.Msg.whoAmI, signId, x, y, txt);
					}
					break;

					//case PacketTypes.PlayerSpawn:
					//	var id = args.Msg.whoAmI;
					//	var player = TShock.Players[id];

					//	if(!player.InitSpawn)
					//	{
					//		player.Spawn(2137,242);
					//	}

					//	break;

					//case PacketTypes.PlayerSpawnSelf:
					//	break;
			}
		}

		void onCatchNpc(int playerIndex, int npcId)
		{
			Debug.Print("CatchNPC!");
			Debug.Print($"PlayerIndex: {playerIndex}");
			Debug.Print($"npcId: {npcId}");
			//Debug.Print($"who: {who}");

			CatchNpc?.Invoke(this, new CatchNpcEventArgs(playerIndex, npcId));
		}

		void onReleaseNpc(int playerIndex, int x, int y, short npcType, byte style)
		{
			Debug.Print("ReleaseNPC!");
			Debug.Print($"PlayerIndex: {playerIndex}");
			Debug.Print($"x: {x}");
			Debug.Print($"y: {y}");
			Debug.Print($"npcType: {npcType}");
			Debug.Print($"style: {style}");

			ReleaseNpc?.Invoke(this, new ReleaseNpcEventArgs(playerIndex, x, y, npcType, style));
		}

		void onChestUnlock(int playerIndex, int x, int y)
		{
			Debug.Print($"OnChestUnlock! {x}, {y}");
			Debug.Print($"whoAmI: {playerIndex}");
			Debug.Print($"x: {x}");
			Debug.Print($"y: {y}");

			ChestUnlocked?.Invoke(this, new ChestUnlockedEventArgs(playerIndex, x, y));
		}

		void onChestItem(int playerIndex, int chestId, int itemSlot, int stack, int prefix, int netId)
		{
			Debug.Print($"OnChestItem!");
			Debug.Print($"playerIndex: {playerIndex}");
			Debug.Print($"chestId: {chestId}");
			Debug.Print($"itemSlot: {itemSlot}");
			Debug.Print($"stack: {stack}");
			Debug.Print($"prefix: {prefix}");
			Debug.Print($"netId: {netId}");

			ChestItemChanged?.Invoke(this, new ChestItemChangedEventArgs(playerIndex, chestId, itemSlot, stack, prefix, netId));
		}

		void onSignRead(int playerIndex, int x, int y)
		{
			Debug.Print("Sign read!");
			Debug.Print($"playerIndex: {playerIndex}");
			Debug.Print($"x: {x}");
			Debug.Print($"y: {y}");

			SignRead?.Invoke(this, new SignReadEventArgs(playerIndex, x, y));
		}

		void onSignChanged(int playerIndex, int signId, int x, int y, string txt)
		{
			Debug.Print("Sign changed!");
			Debug.Print($"playerIndex: {playerIndex}");
			Debug.Print($"signId: {signId}");
			Debug.Print($"x: {x}");
			Debug.Print($"y: {y}");
			Debug.Print($"txt: {txt}");

			SignChanged?.Invoke(this, new SignChangedEventArgs(playerIndex, signId, x, y, txt));
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
				ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
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

			var isLeader = player == party.Leader.Player;

			//        var questSession = player == party.Leader.Player ? session : GetSession(party.Leader);
			//        if (questSession.CurrentQuest != null)
			//        {
			//try
			//{
			//	var bquest = session.CurrentQuest;
			//	bquest.Abort();
			//}
			//catch( Exception ex )
			//{
			//	CustomQuestsPlugin.Instance.LogPrint(ex.ToString());
			//}

			//            party.SendInfoMessage("Aborted quest.");
			//        }

			//if (player == party.Leader.Player)
			//{
			//    foreach (var player2 in party)
			//    {
			//        var session2 = GetSession(player2);
			//        session2.Party = null;
			//        party.SendData(PacketTypes.PlayerTeam, "", player2.Index);
			//    }
			//    _parties.Remove(party.Name);
			//    party.SendInfoMessage($"{player.Name} disbanded the party.");
			//}
			//else
			//{

			session.Party = null;
			party.SendData(PacketTypes.PlayerTeam, "", player.Index);
			party.Remove(player);//,true);
			party.SendInfoMessage($"{player.Name} left the party.");
			player.SendInfoMessage("You have left the party.");

			if (isLeader && party.Count > 0)
				party.SendInfoMessage($"{party.Leader.Name} is the new party leader.");

			if (party.Count < 1)
			{
				_parties.Remove(party.Name);
			}
		}

		private void OnJoin(JoinEventArgs args)
		{
			if (args.Who < 0 || args.Who >= Main.maxPlayers)
			{
				return;
			}

			var player = TShock.Players[args.Who];

			if (player != null)
			{
				Task.Delay(1750).ContinueWith(t =>
				{
					var session = GetSession(player);
					session.RejoinQuest();
				});
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
					if (session.CurrentQuest != null && session.CurrentQuest.MainQuestTask.Status == TaskStatus.Running)
					{
						session.CurrentQuest.TryAddRejoinablePlayer(player);
					}

					session.Dispose();
					Debug.Print("Disabled SetQuestState() in OnLeave().");
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
			QuestRunner.Update();

			foreach (var player in TShock.Players.Where(p => p?.User != null))
			{
				var session = GetSession(player);

				session.CheckQuestCompleted();
				session.CheckRepeatInterval();
			}

			if (DateTime.UtcNow > _lastSave + _config.SavePeriod)
			{
				_lastSave = DateTime.UtcNow;
				_sessionManager.SaveAll();
			}
		}
	}
}
