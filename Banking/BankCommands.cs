﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

			if( parameters.Count == 0 )
			{
				viewBankHelp(args.Player);
				return;
			}

			if( parameters.Count > 0 )
			{
				var subcommand = parameters[0];

				switch( subcommand )
				{
					case "bal":
						if( parameters.Count == 2 )
						{
							viewBalance(player, parameters[1]);
							return;
						}

						viewBankHelp(player);
						break;

					case "pay":
						if( parameters.Count == 4 )
						{
							var currency = parameters[1];
							var target = parameters[2];
							//decimal.TryParse(parameters[3], out amount);
							var money = parameters[3];

							payPlayer(player, currency, target, money);
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

				switch( subcommand )
				{
					case "set":
						if( parameters.Count == 4 )
						{
							var currency = parameters[1];
							var targetName = parameters[2];
							var money = parameters[3];

							setPlayerBalance(player, currency, targetName, money, SetBalanceMode.Set);
							return;
						}
						break;

					case "give":
						if( parameters.Count == 4 )
						{
							var currency = parameters[1];
							var targetName = parameters[2];
							var money = parameters[3];

							setPlayerBalance(player, currency, targetName, money, SetBalanceMode.Give);
							return;
						}
						break;

					case "take":
						if( parameters.Count == 4 )
						{
							var currency = parameters[1];
							var targetName = parameters[2];
							var money = parameters[3];

							setPlayerBalance(player, currency, targetName, money, SetBalanceMode.Take);
							return;
						}
						break;

					case "bal":
						if( parameters.Count == 3 )
						{
							var currency = parameters[1];
							var targetName = parameters[2];

							viewBalance(player, currency, targetName);
							return;
						}
						break;
				}
			}

			viewBankAdminHelp(args.Player);
		}

		private static void viewBankAdminHelp(TSPlayer player)
		{
			player.SendErrorMessage($"Usage is:");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin create <player>");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin delete <player>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin set <currency> <player> <amount>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin give <currency> <player> <amount>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin take <currency> <player> <amount>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin bal <currency> <player>");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin lock <player>");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin unlock <player>");
		}

		private static void viewBankHelp(TSPlayer player)
		{
			player.SendErrorMessage($"Usage is:");
			player.SendErrorMessage($"{Commands.Specifier}bank bal <currency>");
			player.SendErrorMessage($"{Commands.Specifier}bank pay <currency> <player> <amount>");
		}

		private static void viewBalance(TSPlayer client, string currencyType, string target=null)
		{
			var currency = BankingPlugin.Instance.BankAccountManager.CurrencyManager[currencyType];
			var account = BankingPlugin.Instance.GetBankAccount(!string.IsNullOrWhiteSpace(target) ? target : client.Name, currencyType);

			if( currency == null )
			{
				client.SendErrorMessage($"Unable to find currency type '{currencyType}'.");
				return;
			}

			if( account == null )
			{
				client.SendErrorMessage($"Unable to find account for currency '{currencyType}'.");
				return;
			}

			var balance = currency.GetCurrencyConverter().ToString(account.Balance);
			client.SendInfoMessage($"Current Balance: {balance}");
		}

		private static void payPlayer(TSPlayer client, string currencyType, string targetName, string money)
		{
			var currency = BankingPlugin.Instance.BankAccountManager.CurrencyManager[currencyType];
			var clientAccount = BankingPlugin.Instance.GetBankAccount(client.Name, currencyType);
			var targetAccount = BankingPlugin.Instance.GetBankAccount(targetName, currencyType);

			if( currency == null )
			{
				client.SendErrorMessage($"Unable to find currency type '{currencyType}'.");
				return;
			}

			if( clientAccount == null )
			{
				client.SendErrorMessage($"Unable to find account for currency '{currencyType}'.");
				return;
			}

			if( targetAccount == null )
			{
				client.SendErrorMessage($"Unable to transfer funds, account for {targetName} could not be found.");
				return;
			}

			if( currency.GetCurrencyConverter().TryParse(money, out var amount) )
			{
				Debug.Print($"Translated currency to standard units: {amount}.");
				//client.SendInfoMessage($"Translated currency to standard units: {amount}.");

				var result = clientAccount.TryTransferTo(targetAccount, amount);
				if( result )
				{
					client.SendInfoMessage($"Transferred {amount} to {targetName}'s '{currencyType}' account.");
				}
				else
				{
					client.SendErrorMessage("Unable to transfer, insufficient funds.");
				}
			}
			else
			{
				client.SendErrorMessage($"Currency format for '{currencyType}' is invalid.");
				return;
			}
		}

		private static void setPlayerBalance(TSPlayer client, string currencyType, string targetName, string money, SetBalanceMode mode)
		{
			var currency = BankingPlugin.Instance.BankAccountManager.CurrencyManager[currencyType];
			var clientAccount = BankingPlugin.Instance.GetBankAccount(client.Name, currencyType);
			var targetAccount = BankingPlugin.Instance.GetBankAccount(targetName, currencyType);

			if( currency == null )
			{
				client.SendErrorMessage($"Unable to find currency type '{currencyType}'.");
				return;
			}

			if( clientAccount == null )
			{
				client.SendErrorMessage($"Unable to find account for currency '{currencyType}'.");
				return;
			}

			if( targetAccount == null )
			{
				client.SendErrorMessage($"Unable to set balance, account for {targetName} could not be found.");
				return;
			}

			if( currency.GetCurrencyConverter().TryParse(money, out var amount) )
			{
				Debug.Print($"Translated currency to standard units: {amount}.");
				//client.SendInfoMessage($"Translated currency to standard units: {amount}.");

				switch( mode )
				{
					case SetBalanceMode.Take:
						if(targetAccount.TryWithdraw(amount,true))//allow account to be overdrawn
						{
							client.SendInfoMessage($"Took {money} from {targetAccount.OwnerName}'s account.");
							return;
						}

						client.SendInfoMessage($"Unknown error while trying to take {money} from {targetAccount.OwnerName}'s account.");
						client.SendInfoMessage($"Account not modified.");
						break;

					case SetBalanceMode.Set:
						targetAccount.Set(amount);
						client.SendInfoMessage($"Set {targetAccount.OwnerName}'s account to {money}.");
						break;

					case SetBalanceMode.Give:
						targetAccount.Deposit(amount);
						client.SendInfoMessage($"Gave {money} to {targetAccount.OwnerName}'s account.");
						break;
				}
			}
			else
			{
				client.SendErrorMessage($"Currency format for '{currencyType}' is invalid.");
				return;
			}
		}

		private enum SetBalanceMode
		{
			Take = -1,
			Set = 0,
			Give = 1
		}
	}
}
