using Corruption;
using Corruption.PluginSupport;
using CustomSkills.Database;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace CustomSkills
{
	/// <summary>
	/// CustomSkillsPlugin.  
	/// </summary>
	[ApiVersion(2, 1)]
	public sealed partial class CustomSkillsPlugin : TerrariaPlugin
	{
		/// <summary>
		///     Gets the author.
		/// </summary>
		public override string Author => "Timothy Barela";

		/// <summary>
		///     Gets the description.
		/// </summary>
		public override string Description => "System for Customized Skills and Spells.";

		/// <summary>
		///     Gets the name.
		/// </summary>
		public override string Name => "CustomSkills";

		/// <summary>
		///     Gets the version.
		/// </summary>
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		public static readonly string DataDirectory = "skills";
		public static readonly string ConfigPath = Path.Combine(DataDirectory, "config.json");
		public const string SkillPermission = "customskills.skill";

		public static CustomSkillsPlugin Instance = null;
		internal CustomSkillDefinitionLoader CustomSkillDefinitionLoader { get; private set; }
		internal CustomSkillRunner CustomSkillRunner { get; private set; }
		internal ISessionDatabase SessionRepository { get; set; }

		/// <summary>
		///     Initializes a new instance of the <see cref="CustomSkillsPlugin" /> class using the specified Main instance.
		/// </summary>
		/// <param name="game">The Main instance.</param>
		public CustomSkillsPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		//This type of functionality should be made global, and should have been in the plugins from the beginning... 
		/// <summary>
		/// Transforms a relative path string to be relative to the plugins DataDirectory.
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Transformed string if path is not null, and not an absolute path.</returns>
		public string PluginRelativePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return path;

			if (Path.IsPathRooted(path))
				return path;

			return Path.Combine(DataDirectory, path);
		}

		/// <summary>
		///     Initializes the plugin.
		/// </summary>
		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
			ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
			PlayerHooks.PlayerChat += OnPlayerChat;

			//register commands here...
			Commands.ChatCommands.Add(new Command(SkillPermission, SkillCommand, "skill"));
		}

		/// <summary>
		///     Disposes the plugin.
		/// </summary>
		/// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				GeneralHooks.ReloadEvent -= OnReload;
				ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
				ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
				ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
				PlayerHooks.PlayerChat -= OnPlayerChat;
			}

			base.Dispose(disposing);
		}

		private void OnGamePostInitialize(EventArgs args) => OnLoad();

		private void OnLoad()
		{
			var cfg = Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);
			var dbConfig = cfg.DatabaseConfig;

			SessionRepository = SessionDatabaseFactory.LoadOrCreateDatabase(dbConfig.DatabaseType, dbConfig.ConnectionString);

			CustomSkillDefinitionLoader = CustomSkillDefinitionLoader.Load(Path.Combine(DataDirectory, cfg.DefinitionFilepath), cfg.AutoCreateDefinitionFile);
			CustomSkillRunner = new CustomSkillRunner();

			//connected players need sessions regenerated, in case this was a reload
			foreach (var player in TShock.Players)
			{
				if (player?.Active == true)
				{
					var session = Session.GetOrCreateSession(player);
					//... may not need to actually do anything here
				}
			}
		}

		private void OnReload(ReloadEventArgs args)
		{
			Session.SaveAll();
			Session.ActiveSessions.Clear();

			OnLoad();
			args.Player.SendSuccessMessage("[CustomSkills] Reloaded config!");
		}

		private void OnGameUpdate(EventArgs args) => CustomSkillRunner.Update();

		//we handle the join event so that we can ensure were creating sessions at this point, and not during runtime.
		private void OnServerJoin(JoinEventArgs args)
		{
			if (args.Who < 0 || args.Who >= Main.maxPlayers)
				return;

			var player = TShock.Players[args.Who];
			if (player != null)
			{
				var session = Session.GetOrCreateSession(player);
				//session.Save();
			}
		}

		private void OnServerLeave(LeaveEventArgs args)
		{
			if (args.Who < 0 || args.Who >= Main.maxPlayers)
				return;

			var player = TShock.Players[args.Who];
			if (player != null)
			{
				Session.ActiveSessions.TryRemove(player.Name, out var session);
				session?.Save();
			}
		}

		private void OnPlayerChat(PlayerChatEventArgs e)
		{
			//don't check commands
			if (e.RawText.StartsWith(Commands.Specifier))
				return;

			if (!e.Player.HasPermission(SkillPermission))
				return;

			var session = Session.GetOrCreateSession(e.Player);

			if (session.TriggerWordsToSkillDefinitions.Count > 0)
			{
				foreach (var kvp in session.TriggerWordsToSkillDefinitions)
				{
					var triggerWord = kvp.Key;
					var definition = kvp.Value;

					if (e.RawText.Contains(triggerWord))
					{
						//can we use this skill?
						if (definition.PermissionsToUse != null && !PlayerFunctions.PlayerHasPermission(e.Player, definition.PermissionsToUse))
							continue;

						if (!session.IsSkillReady(definition.Name))
							continue;

						if (session.PlayerSkillInfos.TryGetValue(definition.Name, out var playerSkillInfo))
						{
							CustomSkillRunner.AddActiveSkill(e.Player, definition, playerSkillInfo.CurrentLevel);
							return;
						}
					}
				}
			}

			//Debug.Print($"Chat: raw: {e.RawText}");
			//Debug.Print($"Chat: formatted: {e.TShockFormattedText}");
		}
	}
}
