using CustomNpcs.Invasions;
using CustomNpcs.Npcs;
using CustomNpcs.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace CustomNpcs
{
	public sealed partial class CustomNpcsPlugin : TerrariaPlugin
	{
		private void CustomInvade(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count != 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cinvade <name|stop>");
				return;
			}

			var currentInvasion = InvasionManager.Instance?.CurrentInvasion;
			var inputName = parameters[0];
			if (inputName.Equals("stop", StringComparison.OrdinalIgnoreCase))
			{
				if (currentInvasion == null)
				{
					player.SendErrorMessage("There is currently no custom invasion.");
					return;
				}

				//InvasionManager.Instance.StartInvasion(null);
				InvasionManager.Instance.EndInvasion();
				TSPlayer.All.SendInfoMessage($"{player.Name} stopped the current custom invasion.");
				return;
			}

			if (currentInvasion != null)
			{
				player.SendErrorMessage("There is currently already a custom invasion.");
				return;
			}

			var definition = InvasionManager.Instance?.FindDefinition(inputName);
			if (definition == null)
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
			if (parameters.Count != 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cmaxspawns <max-spawns>");
				return;
			}

			var inputMaxSpawns = parameters[0];
			if (!int.TryParse(inputMaxSpawns, out var maxSpawns) || maxSpawns < 0 || maxSpawns > 200)
			{
				player.SendErrorMessage($"Invalid maximum spawns '{inputMaxSpawns}'.");
				return;
			}

			Config.Instance.MaxSpawns = maxSpawns;
			if (args.Silent)
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
			if (parameters.Count == 0 || parameters.Count > 4)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cspawnmob <name> [amount] [x] [y]");
				return;
			}

			var inputName = parameters[0];
			var definition = NpcManager.Instance?.FindDefinition(inputName);
			if (definition == null)
			{
				player.SendErrorMessage($"Invalid custom NPC name '{inputName}'.");
				return;
			}

			var inputAmount = parameters.Count >= 2 ? parameters[1] : "1";
			if (!int.TryParse(inputAmount, out var amount) || amount <= 0 || amount > 200)
			{
				player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
				return;
			}

			var inputX = parameters.Count >= 3 ? parameters[2] : player.TileX.ToString();
			if (!int.TryParse(inputX, out var x) || x < 0 || x > Main.maxTilesX)
			{
				player.SendErrorMessage($"Invalid X position '{inputX}'.");
				return;
			}

			var inputY = parameters.Count == 4 ? parameters[3] : player.TileY.ToString();
			if (!int.TryParse(inputY, out var y) || y < 0 || y > Main.maxTilesY)
			{
				player.SendErrorMessage($"Invalid Y position '{inputY}'.");
				return;
			}

			for (var i = 0; i < amount; ++i)
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
			if (parameters.Count == 0 || parameters.Count > 4)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cspawnprojectile <name> [(x,y)]");
				return;
			}

			var inputName = parameters[0];
			var definition = ProjectileManager.Instance?.FindDefinition(inputName);
			if (definition == null)
			{
				player.SendErrorMessage($"Invalid custom projectile name '{inputName}'.");
				return;
			}

			var speed = 5;
			var facing = args.Player.TPlayer.direction;
			var playerX = args.Player.TileX + (facing * 3);//fire from 2 tiles in front/back of player
			var playerY = args.Player.TileY;
			var targetX = playerX + (facing * 22);//2+20 tiles
			var targetY = playerY;

			var targetString = parameters.Count >= 2 ? parameters[1] : $"({targetX},{targetY})";
			if (!targetString.StartsWith("(") || !targetString.EndsWith(")"))
			{
				player.SendErrorMessage($"Expected parenthesis. Target must be in the form (x,y).");
				return;
			}

			targetString = targetString.Substring(1, targetString.Length - 2);

			var components = targetString.Split(',');
			if (components.Length != 2)
			{
				player.SendErrorMessage($"Expected 2 numeric values. Target must be in the form (x,y).");
				return;
			}

			if (!int.TryParse(components[0], out targetX) || targetX < 0 || targetX > Main.maxTilesX)
			{
				player.SendErrorMessage($"Invalid X position '{components[0]}'.");
				return;
			}

			if (!int.TryParse(components[1], out targetY) || targetY < 0 || targetY > Main.maxTilesY)
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
			if (parameters.Count != 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cspawnrate <spawn-rate>");
				return;
			}

			var inputSpawnRate = parameters[0];
			if (!int.TryParse(inputSpawnRate, out var spawnRate) || spawnRate < 1)
			{
				player.SendErrorMessage($"Invalid spawn rate '{inputSpawnRate}'.");
				return;
			}

			Config.Instance.SpawnRate = spawnRate;
			if (args.Silent)
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
			var page = 1;

			if (parameters.Count == 1)
			{
				if (int.TryParse(parameters[0], out var parsedPage))
					page = parsedPage;
				else
					subCommand = null;//force it to show syntax... yes this is hacky.
			}

			if (subCommand == "list")
			{
				var definitions = NpcManager.Instance?.Definitions;
				if (definitions != null)
				{
					var defs = from def in definitions
							   select $"{def.Name}: \"{def._baseOverride.Name}\", {def.BaseType}";

					sendPagedInfoMessage(player, defs.ToList(), page, 5);
					return;
				}
			}
			else
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cmob <page>");
				return;
			}
		}

		private void CustomProjectileControl(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var subCommand = "list";
			var page = 1;

			if (parameters.Count == 1)
			{
				if (int.TryParse(parameters[0], out var parsedPage))
					page = parsedPage;
				else
					subCommand = null;//force it to show syntax... yes this is hacky.
			}

			if (subCommand == "list")
			{
				var definitions = ProjectileManager.Instance?.Definitions;
				if (definitions != null)
				{
					var defs = from def in definitions
							   select $"{def.Name}: \"{Lang.GetProjectileName(def.BaseType).Value}\", {def.BaseType}";

					sendPagedInfoMessage(player, defs.ToList(), page, 5);
					return;
				}
			}
			else
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}cprojectile <page>");
				return;
			}
		}
	}
}
