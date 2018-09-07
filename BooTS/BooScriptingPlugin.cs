using Corruption;
using Corruption.PluginSupport;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BooTS
{
	/// <summary>
	/// Temporary type/name for standalone scripts that are only used for their side effects.
	/// </summary>
	public class XScript
	{
		public const string Prefix = "boots_";

		Assembly ass;

		public XScript(string filePath)
		{
			ass = null;

			if( !File.Exists(filePath) )
			{
				return;
			}

			var assName = Prefix+Path.GetFileNameWithoutExtension(filePath)+".exe";
			var cc = new BooScriptCompiler();

			//var refs = BooHelpers.GetSystemAssemblies();
			//refs.AddRange(BooHelpers.GetBooLangAssemblies());

			//var imports = new List<string>()
			//{

			//};

			var refs = ScriptHelpers.GetReferences();
			var imports = ScriptHelpers.GetDefaultImports();

			cc.Configure(refs, imports);
			cc.InternalCompiler.Parameters.OutputType = Boo.Lang.Compiler.CompilerOutputType.ConsoleApplication;
			var context = cc.Compile(assName,new List<string>() { filePath });

			if(context.Errors.Count>0)
			{
				BooScriptingPlugin.Instance.LogPrintBooErrors(context);
				return;
			}

			if( context.Warnings.Count > 0 )
			{
				BooScriptingPlugin.Instance.LogPrintBooWarnings(context);
			}

			ass = context.GeneratedAssembly;
		}

		public bool Run(params string[] args)
		{
			try
			{
				//ass?.EntryPoint?.Invoke(null,new object[1] { new string[0] } );
				ass?.EntryPoint?.Invoke(null, new object[1] { args });
				return true;
			}
			catch(Exception ex)
			{
				BooScriptingPlugin.Instance.LogPrint(ex.ToString(),TraceLevel.Error);
				return false;
			}
		}
	}

	[ApiVersion(2, 1)]
	public sealed class BooScriptingPlugin : TerrariaPlugin
	{
		public override string Author => "Timothy Barela";
		public override string Description => "Boo scripting for TShock.";
		public override string Name => "BooTS";
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		internal static string DataDirectory { get; set; } = "scripting";
		internal static string ConfigPath => Path.Combine(DataDirectory, "config.json");

		public static BooScriptingPlugin Instance { get; private set; }

		//public XScript ScriptStartup { get; set; }

		public XScript ScriptServerJoin { get; set; }
		public XScript ScriptServerLeave { get; set; }
				
		public BooScriptingPlugin(Main game) : base(game)
		{
#if DEBUG
			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif
			Instance = this;
		}

		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			
			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
			//ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			//ServerApi.Hooks.NetGetData.Register(this, OnNetGetData);
			//ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
			//ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);
			//ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
			ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
			ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
			//ServerApi.Hooks.WorldSave.Register(this, OnWorldSave);

			Commands.ChatCommands.Add(new Command("boots.control", CommandLoad, "boo")
			{
				HelpText = $"Syntax: {Commands.Specifier}boo run <script>"
			});
		}

		private void CommandLoad(CommandArgs args)
		{
			var player = args.Player;
			
			if(args.Parameters.Count<2)
			{
				player.SendErrorMessage($"Not enough parameters.");
				player.SendErrorMessage($"Format is: {Commands.Specifier}boo run <script>");
			}

			var sub = args.Parameters[0];
			var filePath = args.Parameters[1];
			
			if(sub!="run")
			{
				player.SendErrorMessage($"Unknown sub command '{sub}'.");
				return;
			}

			var fullFilePath = Path.Combine(DataDirectory, filePath);

			if( !File.Exists(fullFilePath) )
			{
				player.SendErrorMessage($"Unknown to find script '{filePath}'.");
				return;
			}

			string[] runArgs = null;

			if( args.Parameters.Count > 2 )
				runArgs = args.Parameters.GetRange(2, args.Parameters.Count - 2).ToArray();
			else
				runArgs = new string[0];

			Task.Run(() =>
			{
				var result = LoadScriptOnDemand(filePath,runArgs);//load script will automatically load from DataFolder.

				if(!result)
					player?.SendErrorMessage("Script failed. Check logs for error information.");
			});
						
			//player.SendErrorMessage($"{Commands.Specifier}boo run <script>");
		}

		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				//JsonConfig.Save(this, Config.Instance, ConfigPath);
				//Bank.Save();

				GeneralHooks.ReloadEvent -= OnReload;
				//	PlayerHooks.PlayerChat -= OnPlayerChat;
				//	PlayerHooks.PlayerPermission -= OnPlayerPermission;
				//ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
				//ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
				//ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
				//ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
				//ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
				ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
				//ServerApi.Hooks.WorldSave.Deregister(this, OnWorldSave);
			}

			base.Dispose(disposing);
		}

		private void onLoad()
		{
			//Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);

			try
			{
				Directory.CreateDirectory(DataDirectory);

				LoadScriptOnDemand("ServerLoad.boo");

				//Debug.Print($"Loading Bank.");

				//if( Bank == null )
				//{
				//	PlayerRewardNotificationDistributor = new PlayerRewardNotificationDistributor();
				//	Bank = new Bank();
				//	NpcSpawnHP = new Dictionary<int, int>();
				//	NpcStrikeTracker = new NpcStrikeTracker();
				//	NpcStrikeTracker.StruckNpcKilled += OnStruckNpcKilled;
				//	PlayerFishingTracker = new PlayerFishingTracker();
				//	PlayerTileTracker = new PlayerTileTracker(DataDirectory);
				//	PlayerSessionTracker = new PlayingRewardTracker();
				//	RewardDistributor = new RewardDistributor();
				//	VoteChecker = new VoteChecker();
				//}

				//NpcStrikeTracker.Clear();
				//Bank.Load();
			}
			catch( Exception ex )
			{
				this.LogPrint(ex.ToString(), TraceLevel.Error);
			}
		}

		private void OnPostInitialize(EventArgs args)
		{
			//should we run a one time start up script here?
			onLoad();
		}

		private void OnReload(ReloadEventArgs e)
		{
			onLoad();
		}

		private void OnWorldSave(WorldSaveEventArgs args)
		{
		}

		private void OnServerJoin(JoinEventArgs args)
		{
			var player = TShock.Players[args.Who];
		}

		private void OnServerLeave(LeaveEventArgs args)
		{
			var player = new TSPlayer(args.Who);
		}

		private void OnNetGetData(GetDataEventArgs args)
		{
			if( args.Handled )
				return;

			switch( args.MsgID )
			{
				//	case PacketTypes.Tile:
				//		using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
				//		{
				//			var action = reader.ReadByte();

				//			if( action >= 0 && action <= 3 )//0 kill, 1 place tile, 2 kill, 3 place wall
				//			{
				//				var tileX = reader.ReadInt16();
				//				var tileY = reader.ReadInt16();
				//				var var1 = reader.ReadInt16();//kill tile status
				//				var var2 = reader.ReadInt16();//place tile

				//				//Debug.Print($"action: {action}");
				//				//Debug.Print($"tileX: {tileX}");
				//				//Debug.Print($"tileY: {tileY}");
				//				//Debug.Print($"var1: {var1}");
				//				//Debug.Print($"var2: {var2}");

				//				var player = TShock.Players[args.Msg.whoAmI];
				//				TileSubTarget tileSubTarget;

				//				if( action < 2 )
				//					tileSubTarget = TileSubTarget.Tile;
				//				else
				//					tileSubTarget = TileSubTarget.Wall;

				//				if( ( action == 0 || action == 2 ) && var1 == 0 )
				//				{
				//					var tile = Main.tile[tileX, tileY];

				//					//kill tile
				//					if( action == 2 || tile.collisionType > 0 ) //ignore grass
				//					{
				//						OnTileKilled(new TileChangedEventArgs(player, tileX, tileY, tile.type, tile.wall, tileSubTarget));
				//						return;
				//					}
				//				}
				//				else if( action == 1 || action == 3 )// && var1 > 0)
				//				{
				//					//var1 should hold the type of the tile or wall we placed.

				//					//place tile
				//					OnTilePlaced(new TileChangedEventArgs(player, tileX, tileY, (ushort)var1, (byte)var1, tileSubTarget));
				//					return;
				//				}
				//			}
				//		}

				//		break;

				//	case PacketTypes.PlayerDeathV2:
				//		//based off of MarioE's original code from the Leveling plugin...
				//		using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
				//		{
				//			var player = TShock.Players[args.Msg.whoAmI];

				//			reader.ReadByte();
				//			var deathReason = PlayerDeathReason.FromReader(reader);
				//			reader.ReadInt16();
				//			reader.ReadByte();
				//			var wasPvP = ( (BitsByte)reader.ReadByte() )[0];
				//			if( wasPvP )
				//			{
				//				var otherPlayer = deathReason.SourcePlayerIndex >= 0
				//					? TShock.Players[deathReason.SourcePlayerIndex]
				//					: null;
				//				if( otherPlayer == player )
				//				{
				//					return;
				//				}

				//				if( otherPlayer != null )
				//				{
				//					var reward = new DeathReward(player.Name, RewardReason.DeathPvP, otherPlayer.Name);
				//					RewardDistributor.EnqueueReward(reward);

				//				}
				//				else
				//				{
				//					var reward = new DeathReward(player.Name, RewardReason.DeathPvP, "");
				//					RewardDistributor.EnqueueReward(reward);
				//				}
				//			}
				//			else
				//			{
				//				var reward = new DeathReward(player.Name, RewardReason.Death, "");
				//				RewardDistributor.EnqueueReward(reward);

				//			}
				//		}

				//		break;

				//	case PacketTypes.ProjectileNew:
				//		//Debug.Print("ProjectileNew!");

				//		using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
				//		{
				//			var projectileId = reader.ReadInt16();
				//			reader.ReadSingle();
				//			reader.ReadSingle();
				//			reader.ReadSingle();
				//			reader.ReadSingle();
				//			reader.ReadSingle();
				//			reader.ReadInt16();
				//			var playerId = reader.ReadByte();
				//			var type = reader.ReadInt16();
				//			//var aiFlags = reader.ReadByte();
				//			//var ai0 = reader.ReadSingle();
				//			//var ai1 = reader.ReadSingle();
				//			//var projUUID = reader.ReadSingle();

				//			PlayerFishingTracker.TryBeginFishing(playerId, projectileId, type);
				//		}

				//		break;

				//	case PacketTypes.ProjectileDestroy:
				//		//Debug.Print("ProjectileDestroy!");
				//		using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
				//		{
				//			var projectileId = reader.ReadInt16();
				//			var ownerId = reader.ReadByte();

				//			PlayerFishingTracker.TryEndFishing(ownerId, projectileId);
				//		}

				//		break;

				//	case PacketTypes.PlayerSlot:
				//		//Debug.Print("PlayerSlot!");
				//		using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
				//		{
				//			var playerId = reader.ReadByte();
				//			var slotId = reader.ReadByte();
				//			var stack = reader.ReadInt16();
				//			var prefix = reader.ReadByte();
				//			var itemId = reader.ReadInt16();

				//			if( PlayerFishingTracker.IsItemFromFishing(playerId) )//, stack, prefix, itemId))
				//			{
				//				var player = TShock.Players[args.Msg.whoAmI];
				//				var reward = new FishingReward(player.Name, stack, prefix, itemId);
				//				RewardDistributor.EnqueueReward(reward);
				//			}
				//		}

				//		break;

				default:
					break;
			}
		}

		private void OnGameUpdate(EventArgs args)
		{
			
		}

		private void OnNpcSpawn(NpcSpawnEventArgs args)
		{
			var npc = Main.npc[args.NpcId];
			//NpcSpawnHP[args.NpcId] = npc.life;
		}

		private void OnNpcStrike(NpcStrikeEventArgs args)
		{
			//Debug.Print($"Banking - OnNpcStrike! Damage: {args.Damage}, Critical: {args.Critical}");

			var item = args.Player.HeldItem;
			//Debug.Print($"Strike NPC with {item.Name}!");
		}

		private void OnNpcKilled(NpcKilledEventArgs args)
		{
			//Debug.Print($"NpcKilled! #{args.npc.whoAmI} - {args.npc.GivenOrTypeName}");
			//Debug.Print($"Value: {args.npc.value}");

			//if( !NpcSpawnHP.TryGetValue(args.npc.whoAmI, out var spawnHp) )
			//{
			//	throw new Exception("Unable to retrieve NpcSpawnHP!");
			//}

			//Debug.Print($"NpcHP: {spawnHp}");
			////NpcStrikeTracker.OnNpcKilled(args.npc);

			//Task.Run(() =>
			//{
			//	//Debug.Print("Task.Run() => OnNpcKilled!");
			//	NpcStrikeTracker.OnNpcKilled(args.npc, spawnHp);
			//});
		}

		internal void LoadScriptsByConvention()
		{
			throw new NotImplementedException();
		}

		internal bool LoadScriptOnDemand(string fileName, params string[] args)
		{
			var path = Path.Combine(DataDirectory, fileName);

			if( File.Exists(path) )
			{
				BooScriptingPlugin.Instance.LogPrint($"Running {path}...", TraceLevel.Info);
				var startup = new XScript(path);
				return startup.Run(args);
			}

			return false;
		}
	}
}
