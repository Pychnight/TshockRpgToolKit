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
		public override string Author => "Timothy A. Barela";
		public override string Description => "A simple, banking and currency system for TShock.";
		public override string Name => "Banking";
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		public static BankingPlugin Instance { get; private set; }
		private static readonly string ConfigPath = Path.Combine("banking", "config.json");

		internal BankAccountManager BankAccountManager;
		internal NpcStrikeTracker NpcStrikeTracker;
		
		public BankAccount WorldAccount { get { return BankAccountManager.WorldAccount; } }
		
		public BankingPlugin(Main game) : base(game)
		{
#if DEBUG
			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif
			Instance = this;
		}

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

			//Debug.Print("INITIALIZE!!");

			BankAccountManager = new BankAccountManager();
			NpcStrikeTracker = new NpcStrikeTracker();
			NpcStrikeTracker.StruckNpcKilled += OnStruckNpcKilled;

			GeneralHooks.ReloadEvent += OnReload;
			//PlayerHooks.PlayerChat += OnPlayerChat;
			//PlayerHooks.PlayerPermission += OnPlayerPermission;

			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			//ServerApi.Hooks.NetGetData.Register(this, OnNetGetData, int.MinValue);
			ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);
			ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
			ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
			//ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);

			//bank bal - View your balance
			//bank bal <player> View other peoples balance
			//bank pay <player> <amount>
			//spawn/delete money with /bank give|take <player> <amount>
						
			Commands.ChatCommands.Add(new Command("banking.bank", BankCommands.Bank, "xbank")
			{
				HelpText = $"Syntax: {Commands.Specifier}bank bal <player-name>\n" +
									$"{Commands.Specifier}bank pay <player-name> <amount>\n"
			});
			Commands.ChatCommands.Add(new Command("banking.admin", BankCommands.BankAdmin, "bankadmin")
			{
				HelpText = $"Syntax: {Commands.Specifier}bank bal <player-name>\n" +
									$"{Commands.Specifier}bank pay <player-name> <amount>\n"
			});
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
				ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
				//ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
			}

			base.Dispose(disposing);
		}

		private void OnPostInitialize(EventArgs args)
		{
			onLoad();
		}

		private void OnReload(ReloadEventArgs e)
		{
			onLoad();
		}

		private void OnServerJoin(JoinEventArgs args)
		{
			var player = new TSPlayer(args.Who);
			BankAccountManager.GetOrCreateBankAccount(player.Name);
		}

		//private void OnServerLeave(LeaveEventArgs args)
		//{
		//	var player = new TSPlayer(args.Who);
		//	Debug.Print($"Player {player.Name} has left the game.");
		//}

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
		
		private void OnStruckNpcKilled(object sender, StruckNpcKilledEventArgs args)
		{
			Debug.Print("OnStruckNpcKilled!");
		}
		
		private void onLoad()
		{
			NpcStrikeTracker.Clear();
			//load bank accounts here.
		}

		public BankAccount GetBankAccount(TSPlayer player)
		{
			return BankAccountManager.GetBankAccount(player);
		}

		public BankAccount GetBankAccount(string name)
		{
			return BankAccountManager.GetBankAccount(name);
		}
	}
}
