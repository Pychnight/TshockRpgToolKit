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
using TShockAPI;
using TShockAPI.Hooks;
using CustomNpcs.Invasions;
using CustomNpcs.Npcs;
using CustomNpcs.Projectiles;
using JetBrains.Annotations;
using System.Diagnostics;

namespace CustomNpcs
{
	/// <summary>
	///     Represents the custom NPCs plugin.
	/// </summary>
	[ApiVersion(2, 1)]
	[UsedImplicitly]
	public sealed class CustomNpcsPlugin : TerrariaPlugin
	{
		private static readonly string ConfigPath = Path.Combine("npcs", "config.json");

		internal static CustomNpcsPlugin Instance = null;

		/// <summary>
		///     Initializes a new instance of the <see cref="CustomNpcsPlugin" /> class using the specified Main instance.
		/// </summary>
		/// <param name="game">The Main instance.</param>
		public CustomNpcsPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		/// <summary>
		///     Gets the author.
		/// </summary>
		public override string Author => "MarioE";

		/// <summary>
		///     Gets the description.
		/// </summary>
		public override string Description => "Provides a custom NPC system.";

		/// <summary>
		///     Gets the name.
		/// </summary>
		public override string Name => "CustomNpcs";

		/// <summary>
		///     Gets the version.
		/// </summary>
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		/// <summary>
		///     Initializes the plugin.
		/// </summary>
		public override void Initialize()
		{
			Directory.CreateDirectory("npcs");
			if( File.Exists(ConfigPath) )
			{
				Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
			}
			InvasionManager.Instance = new InvasionManager(this);
			NpcManager.Instance = new NpcManager(this);
			ProjectileManager.Instance = new ProjectileManager(this);

			GeneralHooks.ReloadEvent += OnReload;

			Commands.ChatCommands.Add(new Command("customnpcs.cinvade", CustomInvade, "cinvade"));
			Commands.ChatCommands.Add(new Command("customnpcs.cmaxspawns", CustomMaxSpawns, "cmaxspawns"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnmob", CustomSpawnMob, "cspawnmob", "csm"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnprojectile", CustomSpawnProjectile, "cspawnprojectile", "csp"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnrate", CustomSpawnRate, "cspawnrate"));

			Commands.ChatCommands.Add(new Command("customnpcs.cspawnmob", CustomMobControl, "cmob"));
			Commands.ChatCommands.Add(new Command("customnpcs.cspawnprojectile", CustomProjectileControl, "cprojectile"));
			//Commands.ChatCommands.Add(new Command("customnpcs.cspawnrate", CustomSpawnRate, "cspawnrate"));
		}

		/// <summary>
		///     Disposes the plugin.
		/// </summary>
		/// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));

				InvasionManager.Instance?.Dispose();
				InvasionManager.Instance = null;
				NpcManager.Instance?.Dispose();
				NpcManager.Instance = null;
				ProjectileManager.Instance?.Dispose();
				ProjectileManager.Instance = null;

				GeneralHooks.ReloadEvent -= OnReload;
			}

			base.Dispose(disposing);
		}

		public void LogPrint(string message, TraceLevel level )
		{
			ServerApi.LogWriter.PluginWriteLine(this, message, level);
		}

		private void CustomInvade(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 1 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cinvade <name|stop>");
				return;
			}

			var currentInvasion = InvasionManager.Instance?.CurrentInvasion;
			var inputName = parameters[0];
			if( inputName.Equals("stop", StringComparison.OrdinalIgnoreCase) )
			{
				if( currentInvasion == null )
				{
					player.SendErrorMessage("There is currently no custom invasion.");
					return;
				}

				//InvasionManager.Instance.StartInvasion(null);
				InvasionManager.Instance.EndInvasion();
				TSPlayer.All.SendInfoMessage($"{player.Name} stopped the current custom invasion.");
				return;
			}

			if( currentInvasion != null )
			{
				player.SendErrorMessage("There is currently already a custom invasion.");
				return;
			}

			var definition = InvasionManager.Instance?.FindDefinition(inputName);
			if( definition == null )
			{
				player.SendErrorMessage($"Invalid invasion '{inputName}'.");
				return;
			}

			InvasionManager.Instance.StartInvasion(definition);
		}

		private void CustomMaxSpawns(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 1 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cmaxspawns <max-spawns>");
				return;
			}

			var inputMaxSpawns = parameters[0];
			if( !int.TryParse(inputMaxSpawns, out var maxSpawns) || maxSpawns < 0 || maxSpawns > 200 )
			{
				player.SendErrorMessage($"Invalid maximum spawns '{inputMaxSpawns}'.");
				return;
			}

			Config.Instance.MaxSpawns = maxSpawns;
			if( args.Silent )
			{
				player.SendSuccessMessage($"Set custom maximum spawns to {maxSpawns}.");
			}
			else
			{
				TSPlayer.All.SendInfoMessage($"{player.Name} set the custom maximum spawns to {maxSpawns}.");
			}
		}

		private void CustomSpawnMob(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count == 0 || parameters.Count > 4 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cspawnmob <name> [amount] [x] [y]");
				return;
			}

			var inputName = parameters[0];
			var definition = NpcManager.Instance?.FindDefinition(inputName);
			if( definition == null )
			{
				player.SendErrorMessage($"Invalid custom NPC name '{inputName}'.");
				return;
			}

			var inputAmount = parameters.Count >= 2 ? parameters[1] : "1";
			if( !int.TryParse(inputAmount, out var amount) || amount <= 0 || amount > 200 )
			{
				player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
				return;
			}

			var inputX = parameters.Count >= 3 ? parameters[2] : player.TileX.ToString();
			if( !int.TryParse(inputX, out var x) || x < 0 || x > Main.maxTilesX )
			{
				player.SendErrorMessage($"Invalid X position '{inputX}'.");
				return;
			}

			var inputY = parameters.Count == 4 ? parameters[3] : player.TileY.ToString();
			if( !int.TryParse(inputY, out var y) || y < 0 || y > Main.maxTilesY )
			{
				player.SendErrorMessage($"Invalid Y position '{inputY}'.");
				return;
			}

			for( var i = 0; i < amount; ++i )
			{
				TShock.Utils.GetRandomClearTileWithInRange(x, y, 50, 50, out var spawnX, out var spawnY);
				NpcManager.Instance.SpawnCustomNpc(definition, 16 * spawnX, 16 * spawnY);
			}
			player.SendSuccessMessage($"Spawned {amount} {definition.Name}(s).");
		}

		private void CustomSpawnProjectile(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count == 0 || parameters.Count > 4 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cspawnprojectile <name> [(x,y)]");
				return;
			}

			var inputName = parameters[0];
			var definition = ProjectileManager.Instance?.FindDefinition(inputName);
			if( definition == null )
			{
				player.SendErrorMessage($"Invalid custom projectile name '{inputName}'.");
				return;
			}

			var speed = 5;
			var facing = args.Player.TPlayer.direction;
			var playerX = args.Player.TileX + ( facing * 3 );//fire from 2 tiles in front/back of player
			var playerY = args.Player.TileY;
			var targetX = (int)( playerX + ( facing * 22 ) );//2+20 tiles
			var targetY = (int)playerY;

			var targetString = parameters.Count >= 2 ? parameters[1] : $"({targetX},{targetY})";
			if( !targetString.StartsWith("(") || !targetString.EndsWith(")") )
			{
				player.SendErrorMessage($"Expected parenthesis. Target must be in the form (x,y).");
				return;
			}

			targetString = targetString.Substring(1, targetString.Length - 2);

			var components = targetString.Split(',');
			if( components.Length != 2 )
			{
				player.SendErrorMessage($"Expected 2 numeric values. Target must be in the form (x,y).");
				return;
			}

			if( !int.TryParse(components[0], out targetX) || targetX < 0 || targetX > Main.maxTilesX )
			{
				player.SendErrorMessage($"Invalid X position '{components[0]}'.");
				return;
			}

			if( !int.TryParse(components[1], out targetY) || targetY < 0 || targetY > Main.maxTilesY )
			{
				player.SendErrorMessage($"Invalid Y position '{components[1]}'.");
				return;
			}

			//if( !int.TryParse(inputSpeed, out speed) || speed <= 0 || speed > 50 )
			//{
			//	player.SendErrorMessage($"Invalid speed '{inputSpeed}'.");
			//	return;
			//}

			//var inputSpeed = parameters.Count >= 2 ? parameters[1] : speed.ToString();
			//if( !int.TryParse(inputSpeed, out speed) || speed <= 0 || speed > 50 )
			//{
			//	player.SendErrorMessage($"Invalid speed '{inputSpeed}'.");
			//	return;
			//}

			var delta = new Vector2(targetX - playerX, targetY - playerY);
			delta.Normalize();
			delta *= speed;

			ProjectileManager.Instance.SpawnCustomProjectile(definition, playerX * 16, playerY * 16, delta.X, delta.Y, player.Index);

			player.SendSuccessMessage($"Spawned {definition.Name}.");
		}

		private void CustomSpawnRate(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 1 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cspawnrate <spawn-rate>");
				return;
			}

			var inputSpawnRate = parameters[0];
			if( !int.TryParse(inputSpawnRate, out var spawnRate) || spawnRate < 1 )
			{
				player.SendErrorMessage($"Invalid spawn rate '{inputSpawnRate}'.");
				return;
			}

			Config.Instance.SpawnRate = spawnRate;
			if( args.Silent )
			{
				player.SendSuccessMessage($"Set custom spawn rate to {spawnRate}.");
			}
			else
			{
				TSPlayer.All.SendInfoMessage($"{player.Name} set the custom spawn rate to {spawnRate}.");
			}
		}

		private void CustomMobControl(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var subCommand = "list";
			if( parameters.Count > 1 )
			{
				subCommand = parameters[0];
			}

			if( subCommand == "list" )
			{
				var definitions = NpcManager.Instance?._definitions;
				if( definitions != null )
				{
					player.SendInfoMessage($"Listing CustomNpcs:");

					var defs = from def in definitions
							   orderby def.Name
							   select $"{def.Name} - {def.BaseType}";
					
					sendGroupedInfoMessage(player, defs, 5, ", ");
					return;
				}
			}
			else
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cmob <subcommand>");
				return;
			}
		}

		private void CustomProjectileControl(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var subCommand = "list";
			if( parameters.Count > 1 )
			{
				subCommand = parameters[0];
			}

			if( subCommand == "list" )
			{
				var definitions = ProjectileManager.Instance?.Definitions;
				if( definitions != null )
				{
					player.SendInfoMessage($"Listing CustomProjectiles:");

					var defs = from def in definitions
							   orderby def.Name
							   select $"{def.Name} - {def.BaseType}";

					sendGroupedInfoMessage(player, defs, 5, ", ");
					return;
				}
			}
			else
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cprojectile <subcommand>");
				return;
			}
		}

		private void sendGroupedInfoMessage(TSPlayer player, IEnumerable<string> items, int itemsPerLine, string separator = ", ")
		{
			if( player == null || items==null || itemsPerLine < 1 )
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
				
				if(lineItems==itemsPerLine)
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

		private void OnReload(ReloadEventArgs args)
        {
            if (File.Exists(ConfigPath))
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            args.Player.SendSuccessMessage("[CustomNpcs] Reloaded config!");
        }
    }
}
