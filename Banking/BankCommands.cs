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
						viewBalance(player, parameters.Count == 2 ? parameters[1] : "");
						break;

					case "pay":
						if( parameters.Count == 4 )
						{
							var currency = parameters[1];
							var target = parameters[2];
							decimal.TryParse(parameters[3], out amount);

							payPlayer(player, currency, target, amount);
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
			var player = args.Player;
			var parameters = args.Parameters;

			if( parameters.Count > 0 )
			{
				var subcommand = parameters[0];
				var amount = 0m;

				switch(subcommand)
				{
					case "set":
						if(parameters.Count == 4)
						{
							var currency = parameters[1];
							var targetName = parameters[2];
							decimal.TryParse(parameters[3], out amount);

							setPlayerBalance(player, currency, targetName, amount);
							return;
						}
						break;
				}
			}
			
			viewBankAdminHelp(args.Player);
		}

		private static void viewBankAdminHelp(TSPlayer player)
		{
			player.SendErrorMessage($"Usage is: (Some commands are proposed only, not working)");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin create <player>");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin delete <player>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin give|take|set <currency> <player> <amount>");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin lock <player>");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin unlock <player>");
		}

		private static void viewBankHelp(TSPlayer player)
		{
			player.SendErrorMessage($"Usage is:");
			player.SendErrorMessage($"{Commands.Specifier}bank bal <currency>");
			player.SendErrorMessage($"{Commands.Specifier}bank pay <currency> <player> <amount>");
		}

		private static void viewBalance(TSPlayer client, string currency)
		{
			//if( string.IsNullOrWhiteSpace(currency) )
			//	currency = client.Name;

			var account = BankingPlugin.Instance.GetBankAccount(client.Name, currency);

			if( account == null )
			{
				client.SendErrorMessage($"Unable to find account for currency '{currency}'.");
				return;
			}

			client.SendInfoMessage($"Current Balance: {account.Balance}");
		}

		private static void payPlayer(TSPlayer client, string currency, string targetName, decimal amount)
		{
			var clientAccount = BankingPlugin.Instance.GetBankAccount(client.Name, currency);
			var targetAccount = BankingPlugin.Instance.GetBankAccount(targetName, currency);

			if( clientAccount == null )
			{
				client.SendErrorMessage($"Unable to find account for currency '{currency}'.");
				return;
			}

			if(targetAccount==null)
			{
				client.SendErrorMessage($"Unable to transfer funds, account for {targetName} could not be found.");
				return;
			}
						
			var result = clientAccount.TryTransferTo(targetAccount, amount);
			if(result)
			{
				client.SendInfoMessage($"Transferred {amount} to {targetName}'s '{currency}' account.");
			}
			else
			{
				client.SendErrorMessage("Unable to transfer, insufficient funds.");
			}
		}

		private static void setPlayerBalance(TSPlayer client, string currency, string targetName, decimal newBalance)
		{
			var targetAccount = BankingPlugin.Instance.GetBankAccount(targetName,currency);

			if( targetAccount == null )
			{
				client.SendErrorMessage($"Unable to find '{currency}' account for {targetName}.");
				return;
			}

			targetAccount.Set(newBalance);
			client.SendInfoMessage($"Set {targetAccount.OwnerName}'s account to {newBalance}.");
		}
	}
}
