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
using CustomNpcs.Invasions;
using CustomNpcs.Npcs;
using CustomNpcs.Projectiles;
using System.Diagnostics;
using Corruption.PluginSupport;

namespace CustomNpcs
{
	/// <summary>
	///     Represents the custom NPCs plugin.
	/// </summary>
	[ApiVersion(2, 1)]
	public sealed partial class CustomNpcsPlugin : TerrariaPlugin
	{
		/// <summary>
		///     Gets the author.
		/// </summary>
		public override string Author => "MarioE, Timothy Barela";

		/// <summary>
		///     Gets the description.
		/// </summary>
		public override string Description => "System for Customized NPC's, Projectiles, and Invasions.";

		/// <summary>
		///     Gets the name.
		/// </summary>
		public override string Name => "CustomNpcs";

		/// <summary>
		///     Gets the version.
		/// </summary>
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		private static readonly string ConfigPath = Path.Combine("npcs", "config.json");
		public static CustomNpcsPlugin Instance = null;
		
		/// <summary>
		///     Initializes a new instance of the <see cref="CustomNpcsPlugin" /> class using the specified Main instance.
		/// </summary>
		/// <param name="game">The Main instance.</param>
		public CustomNpcsPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		/// <summary>
		///     Initializes the plugin.
		/// </summary>
		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			ServerApi.Hooks.GamePostInitialize.Register(this,OnGamePostInitialize);

			Commands.ChatCommands.Add(new Command("customnpcs.cinvade", CustomInvade, "cinvade"));
			Commands.ChatCommands.Add(new Command("customnpcs.cmaxspawns", CustomMaxSpawns, "cmaxspawns"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnmob", CustomSpawnMob, "cspawnmob", "csm"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnprojectile", CustomSpawnProjectile, "cspawnprojectile", "csp"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnrate", CustomSpawnRate, "cspawnrate"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnmob", CustomMobControl, "cmob"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnprojectile", CustomProjectileControl, "cprojectile"));
		}
		
		/// <summary>
		///     Disposes the plugin.
		/// </summary>
		/// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				//File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));

				InvasionManager.Instance?.Dispose();
				InvasionManager.Instance = null;
				NpcManager.Instance?.Dispose();
				NpcManager.Instance = null;
				ProjectileManager.Instance?.Dispose();
				ProjectileManager.Instance = null;

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

			InvasionManager.Instance = InvasionManager.Instance ?? new InvasionManager(this);
			NpcManager.Instance = NpcManager.Instance ?? new NpcManager(this);
			ProjectileManager.Instance = ProjectileManager.Instance ?? new ProjectileManager(this);
		}
		
		private void sendGroupedInfoMessage(TSPlayer player, IEnumerable<string> items, int itemsPerLine, string separator = ", ")
		{
			if( player == null || items == null || itemsPerLine < 1 )
				return;

			//var testItems = new List<string>(48);			
			//for(int i=0;i<testItems.Capacity;i++)
			//{
			//	var t = i;
			//	var n = $"test{i}";
			//	var test = $"{n} - {t}";
			//	testItems.Add(test);
			//}
			//items = items.Concat(testItems);

			var sb = new StringBuilder(256);
			var lineItems = 0;

			foreach( var i in items )
			{
				if( lineItems > 0 )
					sb.Append(separator);

				sb.Append(i);
				lineItems++;

				if( lineItems == itemsPerLine )
				{
					player.SendInfoMessage(sb.ToString());
					sb.Clear();
					lineItems = 0;
				}
			}

			//send any remaining.
			if( lineItems > 0 )
				player.SendInfoMessage(sb.ToString());
		}

		private void sendPagedInfoMessage(TSPlayer player, IList<string> items, int page, int itemsPerPage)
		{
			if( player == null || items == null || itemsPerPage < 1 || page < 1 )
				return;

			var pageCount = ( items.Count / itemsPerPage ) +
							( items.Count % itemsPerPage > 0 ? 1 : 0 );

			if( page > pageCount )
				page = pageCount;

			var startIndex = ( page - 1 ) * itemsPerPage;
			
			for(var i = 0; i<itemsPerPage;i++)
			{
				var lineNumber = startIndex + i;

				//reached end
				if( lineNumber >= items.Count )
					break;

				player.SendInfoMessage($"{lineNumber+1}. {items[lineNumber]}");
			}

			//page number out of pages
			player.SendInfoMessage($"Page# {page} / {pageCount}");
		}

		private void OnReload(ReloadEventArgs args)
        {
			OnLoad();
            args.Player.SendSuccessMessage("[CustomNpcs] Reloaded config!");
        }
    }
}
