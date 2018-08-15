using Banking.Configuration;
using Corruption.PluginSupport;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking
{
	public static class BankCommands
	{
		const string parseAmoungString = @"(\d+)([A-Za-z\d]+)";
		static Regex parseAmountRegex = new Regex(parseAmoungString, RegexOptions.Compiled);

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

						var pageNumber = 1;

						if(parameters.Count==2)
						{
							var param = parameters[1];

							if(int.TryParse(param, out pageNumber))
							{
								viewBalance(player, pageNumber);
							}
							else
							{
								viewBalance(player, param );
							}
						}
						else
						{
							viewBalance(player, pageNumber);
						}

						return;

						//viewBankHelp(player);
						break;

					case "pay":
						if( parameters.Count >= 3 )
						{
							var target = parameters[1];
							payPlayerSmart(player, target, parameters.GetRange(2, parameters.Count - 2));
							return;
						}

						viewBankHelp(player);
						return;

					case "list":
						listCurrency(player);
						return;

					default:
						player.SendErrorMessage($"Unknown subcommand '{subcommand}'.");
						viewBankHelp(player);
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

					case "reset":
						if( parameters.Count == 3 )
						{
							var currency = parameters[1];
							var targetName = parameters[2];
							
							resetPlayerBalance(player, currency, targetName);
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
			player.SendErrorMessage($"{Commands.Specifier}bankadmin bal <currency> <player>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin set <currency> <player> <amount>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin give <currency> <player> <amount>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin take <currency> <player> <amount>");
			player.SendErrorMessage($"{Commands.Specifier}bankadmin reset <currency> <player>");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin lock <player>");
			//player.SendErrorMessage($"{Commands.Specifier}bankadmin unlock <player>");
		}

		private static void viewBankHelp(TSPlayer player)
		{
			player.SendErrorMessage($"Usage is:");
			player.SendErrorMessage($"{Commands.Specifier}bank bal <currency> | <page>");
			//player.SendErrorMessage($"{Commands.Specifier}bank pay <currency> <player> <amount>");
			player.SendErrorMessage($"{Commands.Specifier}bank pay <player> <amount> (examples: 320Gold or 320g or 320g,50s,2Copper)");
			player.SendErrorMessage($"{Commands.Specifier}bank list");
		}

		private static void viewBalance(TSPlayer client, string currencyType, string target=null)
		{
			var currency = BankingPlugin.Instance.Bank.CurrencyManager[currencyType];
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

			var balance = currency.GetCurrencyConverter().ToString(account.Balance);//, true);
			client.SendInfoMessage($"Current Balance: {balance}");
		}

		private static void viewBalance(TSPlayer client, int pageNumber)
		{
			const int itemsPerPage = 4;
			var lines = new List<string>(BankingPlugin.Instance.Bank.CurrencyManager.Count);

			foreach(var currency in BankingPlugin.Instance.Bank.CurrencyManager)
			{
				var account = BankingPlugin.Instance.GetBankAccount(client.Name, currency.InternalName);
				
				if( account == null )
				{
					client.SendErrorMessage($"Unable to find account for currency '{currency.InternalName}'.");
					return;
				}

				var balance = currency.GetCurrencyConverter().ToString(account.Balance);//, true);
				
				lines.Add($"{currency.InternalName} - {balance}");
			}

			var pageCount = lines.PageCount(itemsPerPage);

			if( pageNumber < 1 || pageNumber > pageCount )
				pageNumber = 1;

			var page = lines.GetPage(pageNumber-1, itemsPerPage);//we display based off of 1
			
			client.SendMessage($"Page #{pageNumber} of {pageCount}.", Color.Green);

			foreach(var l in page)
			{
				client.SendInfoMessage(l);
			}

			client.SendMessage("Use /bank bal <page> or /bank bal <currency> to see more.", Color.Green);
		}
		
		private static void payPlayerSmart(TSPlayer client, string targetName, IEnumerable<string> amounts)
		{
			var combinedAmounts = new StringBuilder();
			amounts.ForEach(s => combinedAmounts.Append(s));

			//we should really think about changing the below code to using CurrencyManager.TryFindCurrencyFromString(), since its adapted from below.
			//but for better error reporting, we use this original version.
			var quadNames = CurrencyConverter.ParseQuadrantNames(combinedAmounts.ToString());

			if(quadNames.Count<1)
			{
				client.SendErrorMessage("Invalid input. Please check your formatting, and try again.");
				return;
			}
						
			var mgr = BankingPlugin.Instance.Bank.CurrencyManager;
			var firstQuad = quadNames.First();
			var currency = mgr.GetCurrencyByQuadName(firstQuad);
			if( currency == null )
			{
				client.SendErrorMessage($"'{firstQuad}' does not belong to a known Currency.");
				return;
			}
			
			//ensure all quads lead to the same currency
			foreach(var quadName in quadNames)
			{
				var cur = mgr.GetCurrencyByQuadName(quadName);

				if(cur!=currency)
				{
					client.SendErrorMessage($"'{quadName}' is not valid for currency '{currency.InternalName}'.");
					return;
				}
			}

			payPlayer(client, currency.InternalName, targetName, combinedAmounts.ToString());
		}

		private static void payPlayer(TSPlayer client, string currencyType, string targetName, string money)
		{
			var currency = BankingPlugin.Instance.Bank.CurrencyManager[currencyType];
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

		private static void listCurrency(TSPlayer client)
		{
			client.SendInfoMessage($"Recognized Currencies:");

			foreach(var cur in BankingPlugin.Instance.Bank.CurrencyManager)
			{
				client.SendInfoMessage($"{cur.DisplayString}");
			}
		}

		private static void setPlayerBalance(TSPlayer client, string currencyType, string targetName, string money, SetBalanceMode mode)
		{
			var currency = BankingPlugin.Instance.Bank.CurrencyManager[currencyType];
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
						if(targetAccount.TryWithdraw(amount,WithdrawalMode.AllowOverdraw))
						{
							client.SendInfoMessage($"Took {money} from {targetAccount.OwnerName}'s account.");
							return;
						}

						client.SendInfoMessage($"Unknown error while trying to take {money} from {targetAccount.OwnerName}'s account.");
						client.SendInfoMessage($"Account not modified.");
						break;

					case SetBalanceMode.Set:
						targetAccount.SetBalance(amount);
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

		private static void resetPlayerBalance(TSPlayer client, string currencyType, string targetName)
		{
			var currency = BankingPlugin.Instance.Bank.CurrencyManager[currencyType];
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
				client.SendErrorMessage($"Unable to reset balance, account for {targetName} could not be found.");
				return;
			}

			targetAccount.SetBalance(0);
			client.SendInfoMessage($"Reset {targetAccount.OwnerName}'s '{currencyType}' account.");
		}

		private enum SetBalanceMode
		{
			Take = -1,
			Set = 0,
			Give = 1
		}

		//based off leveling plugin's original Multiplier command.
		public static void Multiplier(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 3 )
			{
				//player.SendErrorMessage($"Syntax: {Commands.Specifier}multiplier <death|deathpvp|exp> <value>");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}multiplier <currency> <gain|death|deathpvp> <value>");
				return;
			}

			var currencyValue = parameters[0];
			var currency = BankingPlugin.Instance.Bank.CurrencyManager[currencyValue];
			if(currency==null)
			{
				player.SendErrorMessage($"Invalid currency '{currencyValue}'.");
				return;
			}
			
			var inputValue = parameters[2];
			if( !float.TryParse(inputValue, out var value) || value < 0.0 )
			{
				player.SendErrorMessage($"Invalid value '{inputValue}'.");
				return;
			}

			var multiplier = parameters[1];
			if( multiplier.Equals("death", StringComparison.OrdinalIgnoreCase) )
			{
				//Config.Instance.DeathPenaltyMultiplier = value;
				currency.DeathPenaltyMultiplier = value;
				player.SendSuccessMessage($"Set death penalty multiplier for {currency}.");
			}
			else if( multiplier.Equals("deathpvp", StringComparison.OrdinalIgnoreCase) )
			{
				//Config.Instance.DeathPenaltyPvPMultiplier = value;
				currency.DeathPenaltyPvPMultiplier = value;
				player.SendSuccessMessage($"Set death PVP penalty multiplier for {currency}.");
			}
			else if( multiplier.Equals("gain", StringComparison.OrdinalIgnoreCase) )
			{
				//Config.Instance.ExpMultiplier = value;
				currency.Multiplier = value;
				player.SendSuccessMessage($"Set gain multiplier for {currency}.");
			}
			else
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}multiplier <currency> <gain|death|deathpvp> <value>");
			}
		}

		public static void Reward(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var config = Config.Instance.Voting;
			var voteChecker = BankingPlugin.Instance.VoteChecker;

			if( !config.Enabled )
			{
				player.SendErrorMessage("Sorry, voting rewards are currently disabled.");
				return;
			}
				
			player.SendInfoMessage("Checking external server for your vote... this may take a few moments.");

			var checkTask = voteChecker.HasPlayerVotedAsync(player.Name)
										.ContinueWith( async t =>
										{
											if( t.Result == VoteStatus.Unclaimed )
											{
												player.SendInfoMessage(config.RewardMessage);
												var voteReward = new VoteReward(player.Name);
												BankingPlugin.Instance.RewardDistributor.EnqueueReward(voteReward);
												
												await voteChecker.ClaimPlayerVoteAsync(player.Name);
											}
											else
												player.SendErrorMessage(config.NoRewardMessage);
										});

			//BankingPlugin.Instance.RewardDistributor.TryAddVoteReward(player.Name);
		}
	}
}
