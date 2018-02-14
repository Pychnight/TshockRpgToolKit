using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking
{
	public static class BankCommands
	{
		public static void Bank(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;

			if(parameters.Count == 0)
			{
				viewBankHelp(args.Player);
				return;
			}

			if(parameters.Count > 0)
			{
				var subcommand = parameters[0];
				var amount = 0m;

				switch(subcommand)
				{
					case "bal":
						viewBalance(player, parameters.Count == 2 ? parameters[1] : null);
						break;

					case "pay":
						if( parameters.Count == 3 )
						{
							if( !decimal.TryParse(parameters[2], out amount) )
							{
								player.SendErrorMessage($"Invalid amount, '{parameters[2]}'");
								return;
							}

							payPlayer(player, parameters[1], amount);
							return;
						}
						
						viewBankHelp(player);
						return;
							
					default:
						player.SendErrorMessage($"Unknown subcommand '{subcommand}'.");
						break;
				}
			}
		}

		public static void BankAdmin(CommandArgs args)
		{
			viewBankAdminHelp(args.Player);
		}

		private static void viewBankAdminHelp(TSPlayer player)
		{
			player.SendErrorMessage($"Usage is: (These commands are proposed only, not working)");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin create <player>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin delete <player>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin give|take|set <player> <amount>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin lock <player>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin unlock <player>");
		}

		private static void viewBankHelp(TSPlayer player)
		{
			player.SendErrorMessage($"Usage is:");
			player.SendErrorMessage($"{Commands.Specifier}bank bal <player>");
			player.SendErrorMessage($"{Commands.Specifier}bank pay <player> <amount>");
		}

		private static void viewBalance(TSPlayer client, string targetName)
		{
			if( string.IsNullOrWhiteSpace(targetName) )
				targetName = client.Name;

			var account = BankingPlugin.Instance.GetBankAccount(targetName);

			if( account == null )
			{
				client.SendErrorMessage($"Unable to find account for {targetName}");
				return;
			}

			if( client.Name == targetName )
				client.SendInfoMessage($"Current Balance: {account.Balance}");
			else
				client.SendInfoMessage($"{targetName}'s Current Balance: {account.Balance}");
		}

		private static void payPlayer(TSPlayer client, string targetName, decimal amount)
		{
			var clientAccount = BankingPlugin.Instance.GetBankAccount(client);
			var targetAccount = BankingPlugin.Instance.GetBankAccount(targetName);

			if(targetAccount==null)
			{
				client.SendErrorMessage($"Unable to transfer funds, account for {targetName} could not be found.");
				return;
			}
						
			var result = clientAccount.TryTransferTo(targetAccount, amount);
			if(result)
			{
				client.SendInfoMessage($"Transferred {amount} to {targetAccount.OwnerName}'s account.");
			}
			else
			{
				client.SendErrorMessage("Unable to transfer, insufficient funds.");
			}
		}
	}
}
