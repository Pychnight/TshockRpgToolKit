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
			Instance = this;
		}

		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			
			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
			//ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
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
				var result = RunScriptOnDemand(filePath,runArgs);//load script will automatically load from DataFolder.

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
				
				GeneralHooks.ReloadEvent -= OnReload;
				//	PlayerHooks.PlayerChat -= OnPlayerChat;
				//	PlayerHooks.PlayerPermission -= OnPlayerPermission;
				//ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
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
				RunScriptOnDemand("ServerLoad.boo");
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

		//private void OnGameUpdate(EventArgs args)
		//{
		//}

		internal void LoadScriptsByConvention()
		{
			
		}

		internal bool TryLoadScript(string fileName)
		{
			throw new NotImplementedException();
		}

		internal bool RunScriptOnDemand(string fileName, params string[] args)
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
