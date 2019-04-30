using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using OTAPI.Tile;
using TShockAPI;
using TShockAPI.Hooks;
using System.Diagnostics;
using Corruption.PluginSupport;

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

		private static readonly string ConfigPath = Path.Combine("skills", "config.json");
		public static CustomSkillsPlugin Instance = null;

		/// <summary>
		///     Initializes a new instance of the <see cref="CustomSkillsPlugin" /> class using the specified Main instance.
		/// </summary>
		/// <param name="game">The Main instance.</param>
		public CustomSkillsPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		/// <summary>
		///     Initializes the plugin.
		/// </summary>
		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);

			//register commands here...
			Commands.ChatCommands.Add(new Command("customskills.skill", SkillCommand, "skill"));
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
			}

			base.Dispose(disposing);
		}

		private void OnGamePostInitialize(EventArgs args)
		{
			OnLoad();
		}

		private void OnLoad()
		{
			Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);
		}

		private void OnReload(ReloadEventArgs args)
		{
			OnLoad();
			args.Player.SendSuccessMessage("[CustomSkills] Reloaded config!");
		}
	}
}
