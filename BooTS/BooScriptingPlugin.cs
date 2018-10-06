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

		//Convention based scripts
		internal Script ScriptServerStart { get; set; }
		internal Script ScriptServerJoin { get; set; }
		internal Script ScriptServerLeave { get; set; }

		internal ConcurrentDictionary<string,Script> ScheduledScripts { get; set; }
				
		public BooScriptingPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			
			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
			ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
			
			Commands.ChatCommands.Add(new Command("boots.control", CommandLoad, "boo")
			{
				HelpText = $"Syntax: {Commands.Specifier}boo run <script>"
			});
		}
		
		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				//JsonConfig.Save(this, Config.Instance, ConfigPath);
				
				GeneralHooks.ReloadEvent -= OnReload;
				//	PlayerHooks.PlayerChat -= OnPlayerChat;
				ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
				ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
			}

			base.Dispose(disposing);
		}

		private void onLoad()
		{
			//Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);

			try
			{
				Directory.CreateDirectory(DataDirectory);
								
				LoadScriptsByConvention();
				RunScript(ScriptServerStart);

				ScheduledScripts = new ConcurrentDictionary<string, Script>();
				LoadScheduledScripts();			   
			}
			catch( Exception ex )
			{
				this.LogPrint(ex.ToString(), TraceLevel.Error);
			}
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
			Debug.Print("OnServerJoin");

			var player = TShock.Players[args.Who];
			RunScript(ScriptServerJoin, player);
		}

		private void OnServerLeave(LeaveEventArgs args)
		{
			Debug.Print("OnServerLeave");

			var player = new TSPlayer(args.Who);
			RunScript(ScriptServerLeave, player);
		}

		private void OnGameUpdate(EventArgs args)
		{
			if (ScheduledScripts != null)
			{
				var currentTime = TimeFunctions.GetTimeOfDay();

				foreach (var script in ScheduledScripts.Values)
				{
					var scheduler = script.GetSchedulerObject();
					var shouldRun = scheduler?.OnUpdate(currentTime);

					if(shouldRun==true)
						RunScript(script);
				}
			}
		}

		/// <summary>
		/// Scans the scripts directory for convention based filenames, and attempts to compile and cache them.
		/// </summary>
		internal void LoadScriptsByConvention()
		{
			ScriptServerStart	= TryReloadScript("ServerStart.boo", ScriptServerStart);
			ScriptServerJoin	= TryReloadScript("ServerJoin.boo", ScriptServerJoin);
			ScriptServerLeave	= TryReloadScript("ServerLeave.boo", ScriptServerLeave);
		}

		/// <summary>
		/// Scans the scheduled scripts directory for scripts, which will be run based off of their configured Scheduler.
		/// </summary>
		internal void LoadScheduledScripts()
		{
			var baseDirectory = Path.Combine(DataDirectory, "scheduled");

			Directory.CreateDirectory(baseDirectory);

			var booFiles = Directory.EnumerateFiles(baseDirectory,"*.boo");

			foreach(var file in booFiles)
			{
				ScheduledScripts.TryGetValue(file, out var script);
				script = TryReloadScript(file, script, isScheduled: true);
				ScheduledScripts[file] = script;
			}
		}
	
		/// <summary>
		/// Attempts to compile a Script, if it does not exist or is not up to date.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		internal Script TryReloadScript(string fileName, Script script, bool isScheduled = false)
		{
			string path;

			//HACK quick fix since scheduled scripts are coming in with the data directory already tacked on...
			if (fileName.StartsWith(DataDirectory))
				path = fileName;
			else
				path = Path.Combine(DataDirectory, fileName);

			script = script ?? new Script(path);

			if( script.TryRebuild(Script.Compile, out var context))
			{
				if (context.Errors.Count != 0)// && context.GeneratedAssembly != null)
				{
					BooScriptingPlugin.Instance.LogPrint($"Boo script '{path}' compile failed with error(s).", TraceLevel.Error);
				}

				if(isScheduled && script.GetSchedule!=null)
				{
					try
					{
						var scheduler = script.GetSchedule();

						if(scheduler!=null)
						{
							script.SetSchedulerObject(scheduler);	
						}
					}
					catch(Exception ex)
					{
						//BooScriptingPlugin.Instance.LogPrint($"Boo script '{path}' GetSchedule() failed with error(s).", TraceLevel.Error);
						BooScriptingPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
					}
				}
			}
			
			return script;
		}

		/// <summary>
		/// Runs a precompiled script, passing in the optional string arguments.
		/// </summary>
		/// <param name="script"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		internal bool RunScript(Script script, params object[] args)
		{
			if (script == null)
				throw new ArgumentNullException("script");	

			if(script.IsBuilt)
				return script.Run(args);
			
			return false;
		}

		/// <summary>
		/// Compiles and runs a Script, but does not cache the script assembly.
		/// </summary>
		/// <param name="fileName">Filepath to script.</param>
		/// <param name="args">String args.</param>
		/// <returns>True if the Script ran successfully, false if not.</returns>
		internal bool RunScriptOnDemand(string fileName, params object[] args)
		{
			var path = Path.Combine(DataDirectory, fileName);

			if( File.Exists(path) )
			{
				var script = new Script(path);
				var context = script.Compile();

				if (context!=null && context.Errors.Count == 0 && context.GeneratedAssembly != null)
					return script.Run(args);
				else
				{
					BooScriptingPlugin.Instance.LogPrint($"Boo script '{path}' compile failed with error(s).", TraceLevel.Error);
					return false;
				}
			}
			else
			{
				BooScriptingPlugin.Instance.LogPrint($"Unable to run boo script '{path}', file not found.", TraceLevel.Error);
				return false;
			}
		}

		private void CommandLoad(CommandArgs args)
		{
			var player = args.Player;

			if (args.Parameters.Count < 2)
			{
				player.SendErrorMessage($"Not enough parameters.");
				player.SendErrorMessage($"Format is: {Commands.Specifier}boo run <script>");
			}

			var sub = args.Parameters[0];
			var filePath = args.Parameters[1];

			if (sub != "run")
			{
				player.SendErrorMessage($"Unknown sub command '{sub}'.");
				return;
			}

			var fullFilePath = Path.Combine(DataDirectory, filePath);

			if (!File.Exists(fullFilePath))
			{
				player.SendErrorMessage($"Unknown to find script '{filePath}'.");
				return;
			}

			string[] runArgs = null;

			if (args.Parameters.Count > 2)
				runArgs = args.Parameters.GetRange(2, args.Parameters.Count - 2).ToArray();
			else
				runArgs = new string[0];

			Task.Run(() =>
			{
				var result = RunScriptOnDemand(filePath, runArgs);//load script will automatically load from DataFolder.

				if (!result)
					player?.SendErrorMessage("Script failed. Check logs for error information.");
			});

			//player.SendErrorMessage($"{Commands.Specifier}boo run <script>");
		}
	}
}
