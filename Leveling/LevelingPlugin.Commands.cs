using Banking;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Leveling
{
	public sealed partial class LevelingPlugin : TerrariaPlugin
	{
		private void AddHp(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count != 2)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}addhp <player-name> <hp-amount>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var inputHpAmount = parameters[1];
			if (!int.TryParse(inputHpAmount, out var hpAmount) || hpAmount == 0)
			{
				player.SendErrorMessage($"Invalid HP amount '{inputHpAmount}'.");
				return;
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);
			otherPlayer.TPlayer.statLifeMax = Math.Max(100, otherPlayer.TPlayer.statLifeMax + hpAmount);
			TSPlayer.All.SendData(PacketTypes.PlayerHp, "", otherPlayer.Index);

			player.SendSuccessMessage($"Gave {otherPlayer.Name} {hpAmount} HP.");
			if (hpAmount > 0)
			{
				otherPlayer.SendInfoMessage($"You gained [c/{CombatText.HealLife.Hex3()}:{hpAmount} HP].");
				session.AddCombatText($"+{hpAmount} HP", CombatText.HealLife);
			}
			else
			{
				otherPlayer.SendInfoMessage($"You lost [c/{CombatText.DamagedFriendly.Hex3()}:{-hpAmount} HP].");
				session.AddCombatText($"{hpAmount} HP", CombatText.DamagedFriendly);
			}
		}

		private void AddMp(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count != 2)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}addmp <player-name> <mp-amount>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var inputMpAmount = parameters[1];
			if (!int.TryParse(inputMpAmount, out var mpAmount) || mpAmount == 0)
			{
				player.SendErrorMessage($"Invalid MP amount '{inputMpAmount}'.");
				return;
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);
			otherPlayer.TPlayer.statManaMax = Math.Max(20, otherPlayer.TPlayer.statManaMax + mpAmount);
			TSPlayer.All.SendData(PacketTypes.PlayerMana, "", otherPlayer.Index);

			player.SendSuccessMessage($"Gave {otherPlayer.Name} {mpAmount} MP.");
			if (mpAmount > 0)
			{
				otherPlayer.SendInfoMessage($"You gained [c/{CombatText.HealMana.Hex3()}:{mpAmount} MP].");
				session.AddCombatText($"+{mpAmount} MP", CombatText.HealMana);
			}
			else
			{
				otherPlayer.SendInfoMessage($"You lost [c/{CombatText.DamagedFriendly.Hex3()}:{-mpAmount} MP].");
				session.AddCombatText($"{mpAmount} MP", CombatText.DamagedFriendly);
			}
		}

		private void ClassCmd(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count > 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}class [class-name]");
				return;
			}

			var session = GetOrCreateSession(player);
			if (parameters.Count == 0)
			{
				var classes = session.UnlockedClasses.Except(session.MasteredClasses).ToList();
				var newClasses = _classes.Except(session.UnlockedClasses).Where(session.HasPrerequisites).ToList();
				if (classes.Count > 0)
				{
					player.SendInfoMessage($"Classes: {string.Join(", ", classes)}");
				}
				if (session.MasteredClasses.Count > 0)
				{
					player.SendInfoMessage(
						$"Mastered classes: [c/{Color.LightSlateGray.Hex3()}:{string.Join(", ", session.MasteredClasses)}]");
				}
				if (newClasses.Count > 0)
				{
					player.SendInfoMessage(
						$"New classes: [c/{Color.LimeGreen.Hex3()}:{string.Join(", ", newClasses)}]");
				}
			}
			else
			{
				var inputClassName = parameters[0];
				var klass = _classes.FirstOrDefault(
					c => string.Equals(c.DisplayName, inputClassName, StringComparison.OrdinalIgnoreCase));
				if (klass == null)
				{
					player.SendErrorMessage($"Invalid class '{inputClassName}'.");
					return;
				}

				if (!session.Class.AllowSwitching)
				{
					player.SendErrorMessage("You can't switch classes.");
					return;
				}
				if (!session.Class.AllowSwitchingBeforeMastery && !session.MasteredClasses.Contains(session.Class))
				{
					player.SendErrorMessage("You can't switch classes until you've mastered your current one.");
					return;
				}

				if (session.UnlockedClasses.Contains(klass))
				{
					session.Class = klass;
					player.SendSuccessMessage($"Changed to the {klass} class.");
					return;
				}

				var missingLevels = klass.PrerequisiteLevels.Where(l => !session.HasLevel(l)).ToList();
				if (missingLevels.Count > 0)
				{
					player.SendErrorMessage(
						$"You can't unlock the {klass} class, as you haven't reached " +
						$"{string.Join(", ", missingLevels.Select(l => $"{l} {l.Class}"))}");
					return;
				}

				var missingPermissions = klass.PrerequisitePermissions.Where(p => !player.HasPermission(p)).ToList();
				if (missingPermissions.Count > 0)
				{
					player.SendErrorMessage(
						$"You can't unlock the {klass} class, as you don't have the " +
						$"{string.Join(", ", missingPermissions)} permission(s).");
					return;
				}

				if (klass.CostCurrency != null && klass.Cost > 0)
				{
					player.SendInfoMessage($"It costs [c/{Color.OrangeRed.Hex3()}:{klass.CostString}] to unlock the {klass} class.");
					player.SendInfoMessage("Do you wish to proceed? Type /yes or /no.");
					player.AddResponse("yes", args2 =>
					{
						player.AwaitingResponse.Remove("no");

						var bankAccount = BankingPlugin.Instance.GetBankAccount(player, klass.CostCurrency.InternalName);
						if (bankAccount == null || bankAccount.Balance < klass.Cost)
						{
							player.SendErrorMessage($"Insufficient funds to unlock the {klass} class.");
							return;
						}

						if (bankAccount.TryTransferTo(BankingPlugin.Instance.GetBankAccount("Server", klass.CostCurrency.InternalName), klass.Cost))
						{
							session.UnlockClass(klass);
							session.Class = klass;
							player.SendSuccessMessage($"Changed to the {klass} class.");
						}
						else
						{
							player.SendErrorMessage($"Currency transfer failed.");
						}
					});
					player.AddResponse("no", args2 =>
					{
						player.AwaitingResponse.Remove("yes");
						player.SendInfoMessage($"Canceled unlocking the {klass} class.");
					});
					return;
				}

				session.UnlockClass(klass);
				session.Class = klass;
				player.SendSuccessMessage($"Changed to the {klass} class.");
			}
		}

		private void Exp(CommandArgs args)
		{
			var player = args.Player;
			var session = GetOrCreateSession(player);
			var level = session.Level;
			player.SendInfoMessage($"You are currently a {level} {session.Class}.");
			if (level.ExpRequired > 0)
			{
				player.SendInfoMessage($"EXP: [c/{Color.LimeGreen.Hex3()}:{session.Exp}/{level.ExpRequired}]");
			}
		}

		private void GiveExp(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count != 2)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}giveexp <player-name> <exp-amount>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var inputExpAmount = parameters[1];
			if (!long.TryParse(inputExpAmount, out var expAmount) || expAmount == 0)
			{
				player.SendErrorMessage($"Invalid EXP amount '{inputExpAmount}'.");
				return;
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);

			player.SendSuccessMessage($"Gave {otherPlayer.Name} {expAmount} EXP.");
			otherPlayer.SendInfoMessage(expAmount > 0
											? $"You gained [c/{Color.LimeGreen.Hex3()}:{expAmount} EXP]."
											: $"You lost [c/{Color.OrangeRed.Hex3()}:{-expAmount} EXP].");
			session.AddExpToReport(expAmount);
			session.GiveExp(expAmount);
		}

		private void GiveOnce(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count < 2 || parameters.Count > 4)
			{
				player.SendErrorMessage(
					$"Syntax: {Commands.Specifier}giveonce <player-name> <item-name> [stack] [prefix]");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var inputItemName = parameters[1];
			var items = TShock.Utils.GetItemByIdOrName(inputItemName);
			if (items.Count == 0)
			{
				player.SendErrorMessage($"Invalid item '{inputItemName}'.");
				return;
			}
			if (items.Count > 1)
			{
				player.SendErrorMessage($"Multiple items matched '{inputItemName}':");
				//TShock.Utils.SendMultipleMatchError(player, items);
				return;
			}

			var item = items[0];
			var inputStack = parameters.Count > 2 ? parameters[2] : item.maxStack.ToString();
			if (!int.TryParse(inputStack, out var stack) || stack <= 0 || stack > item.maxStack)
			{
				player.SendErrorMessage($"Invalid stack '{inputStack}'.");
				return;
			}

			var prefix = 0;
			if (parameters.Count > 3)
			{
				var inputPrefix = parameters[3];
				var prefixes = TShock.Utils.GetPrefixByIdOrName(inputPrefix);
				if (prefixes.Count == 0)
				{
					player.SendErrorMessage($"Invalid prefix '{inputPrefix}'.");
					return;
				}
				if (prefixes.Count > 1)
				{
					player.SendErrorMessage($"Multiple prefixes matched '{inputItemName}':");
					//TShock.Utils.SendMultipleMatchError(player, prefixes.Cast<object>());
					return;
				}
				prefix = prefixes[0];
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);
			if (session.ItemIdsGiven.Contains(item.type))
			{
				player.SendErrorMessage(
					$"{otherPlayer.Name} was already given [i/s{stack},p{prefix}:{item.type}].");
				return;
			}

			session.AddItemId(item.type);
			player.SendSuccessMessage($"Gave [i/s{stack},p{prefix}:{item.type}] to {otherPlayer.Name}.");
			otherPlayer.GiveItem(item.type, stack, prefix);
			otherPlayer.SendInfoMessage($"Received [i/s{stack},p{prefix}:{item.type}].");
		}

		private void LevelDown(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count != 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}leveldown <player-name>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);
			if (session.LevelDown())
			{
				player.SendSuccessMessage($"Leveled down {otherPlayer.Name}.");
				otherPlayer.SendInfoMessage("You have been leveled down.");
			}
			else
			{
				player.SendErrorMessage($"{otherPlayer.Name} could not be leveled down.");
			}
		}

		private void LevelUp(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count != 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}levelup <player-name>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);
			if (session.LevelUp())
			{
				player.SendSuccessMessage($"Leveled up {otherPlayer.Name}.");
				otherPlayer.SendInfoMessage("You have been leveled up.");
			}
			else
			{
				player.SendErrorMessage($"{otherPlayer.Name} could not be leveled up.");
			}
		}

		private void LevelReset(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count != 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}levelreset <player-name>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);

			session.LevelReset();
			player.SendSuccessMessage($"Reset level for {otherPlayer.Name}.");
			otherPlayer.SendInfoMessage("Your level has been reset.");
		}

		private void SendTo(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count < 3)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}sendto <player-name> <rrr,ggg,bbb> <text>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var inputRgb = parameters[1];
			var inputRgbComponents = inputRgb.Split(',');
			if (inputRgbComponents.Length != 3 || !byte.TryParse(inputRgbComponents[0], out var r) ||
				!byte.TryParse(inputRgbComponents[1], out var g) || !byte.TryParse(inputRgbComponents[2], out var b))
			{
				player.SendErrorMessage($"Invalid RGB components '{inputRgb}'.");
				return;
			}

			var inputText = string.Join(" ", parameters.Skip(2));
			players[0].SendMessage(inputText, r, g, b);
		}

		private void SetClass(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if (parameters.Count != 2)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}setclass <player-name> <class-name>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TSPlayer.FindByNameOrID(inputPlayerName);
			if (players.Count == 0)
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if (players.Count > 1)
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				//TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var inputClassName = parameters[1];
			var @class = _classes.FirstOrDefault(
				c => c.DisplayName.Equals(inputClassName, StringComparison.OrdinalIgnoreCase));
			if (@class == null)
			{
				player.SendErrorMessage($"Invalid class '{inputClassName}'.");
				return;
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);
			if (!session.UnlockedClasses.Contains(@class))
			{
				session.UnlockClass(@class);
			}
			session.Class = @class;
			player.SendSuccessMessage($"Set {otherPlayer.Name}'s class to {@class}.");
			otherPlayer.SendInfoMessage($"You have been set to the {@class} class.");
		}

		private void LevelDump(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;

			var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
			var fileName = $"{player.Name}-{timeStamp}.json";

			fileName = Path.Combine("leveling", fileName);

			var session = GetOrCreateSession(player);
			var json = JsonConvert.SerializeObject(session._definition, Formatting.Indented);

			File.WriteAllText(fileName, json);

			player.SendErrorMessage($"Dumped leveling info to file '{fileName}'.");
		}
	}
}
