using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Banking
{
	[ApiVersion(2, 1)]
	public sealed class BankingPlugin : TerrariaPlugin
	{
		public static BankingPlugin Instance { get; private set; }
		//private static readonly string ConfigPath = Path.Combine("leveling", "config.json");
		public BankingPlugin(Main game) : base(game)
		{
#if DEBUG
			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif
			Instance = this;
		}

		public override string Author => "Timothy A. Barela";
		public override string Description => "A simple, banking and currency system for TShock.";
		public override string Name => "Banking";
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		internal NpcStrikeTracker NpcStrikeTracker;

		public override void Initialize()
		{
			//try
			//{
			//	Directory.CreateDirectory("leveling");
			//	if( File.Exists(ConfigPath) )
			//	{
			//		Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
			//	}

			//	//SessionRepository = new SqliteSessionRepository(Path.Combine("leveling", "sessions.db"));

			//}
			//catch( Exception ex )
			//{
			//	ServerApi.LogWriter.PluginWriteLine(BankingPlugin.Instance, $"Error: {ex.Message}", TraceLevel.Error);
			//	ServerApi.LogWriter.PluginWriteLine(BankingPlugin.Instance, ex.StackTrace, TraceLevel.Error);
			//	ServerApi.LogWriter.PluginWriteLine(BankingPlugin.Instance, $"Plugin is disabled. Please correct errors and restart server.", TraceLevel.Error);
			//	this.Enabled = false;
			//	return;
			//}

			NpcStrikeTracker = new NpcStrikeTracker();
			
			GeneralHooks.ReloadEvent += OnReload;
			//PlayerHooks.PlayerChat += OnPlayerChat;
			//PlayerHooks.PlayerPermission += OnPlayerPermission;
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			//ServerApi.Hooks.NetGetData.Register(this, OnNetGetData, int.MinValue);
			ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);
			ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
			//ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);

			//Commands.ChatCommands.Add(new Command("leveling.addhp", AddHp, "addhp")
			//{
			//	HelpText = $"Syntax: {Commands.Specifier}addhp <player-name> <hp-amount>\n" +
			//			   "Adds an amount of max HP to the specified player."
			//});
		}

		private void OnGameUpdate(EventArgs args)
		{
			NpcStrikeTracker.OnGameUpdate();
		}

		private void OnNpcStrike(NpcStrikeEventArgs args)
		{
			Debug.Print("OnNpcStrike!");
			
			NpcStrikeTracker.OnNpcStrike(args.Player, args.Npc);
		}

		private void OnNpcKilled(NpcKilledEventArgs args)
		{
			Debug.Print("NpcKilled!");
			Debug.Print($"Value: {args.npc.value}");

			NpcStrikeTracker.OnNpcKilled(args.npc);
		}

		private void OnReload(ReloadEventArgs e)
		{
			Debug.Print("OnReload...we should do something here!");
		}

		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				GeneralHooks.ReloadEvent -= OnReload;
			//	PlayerHooks.PlayerChat -= OnPlayerChat;
			//	PlayerHooks.PlayerPermission -= OnPlayerPermission;
				ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
				//	ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
				ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
				ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
				//	ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
			}

			base.Dispose(disposing);
		}
	}
}
