using Banking;
//using Wolfje.Plugins.SEconomy;
//using Wolfje.Plugins.SEconomy.Journal;
using Housing.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TShockAPI;

namespace Housing
{
	public class TaxService
	{
		HousingPlugin plugin;

		/// <summary>
		/// Gets or sets whether tax collectors will recieve tax payments.
		/// </summary>
		public bool IsEnabled { get; set; }

		public TaxService(HousingPlugin plugin)
		{
			this.plugin = plugin;
			//Debug.Print("Created TaxService.");
		}

		/// <summary>
		/// Iterates over the players accounts, pulling funds from each account until the payment is satisfied.
		/// It works in order of largest account to smallest.
		/// </summary>
		/// <param name="playerName">Player name.</param>
		/// <param name="payment">Total payment size, in generic units.</param>
		/// <returns>IEnumerable<Tuple<decimal,BankAccount>> containing the pay and account per iteration.</returns>
		public IEnumerable<Tuple<decimal, BankAccount>> PayTaxIterator(string playerName, decimal payment)
		{
			//no account has been specified for this player. So...
			//get all accounts for this player, sorted by largest balance
			var playerAccounts = BankingPlugin.Instance.GetAllBankAccountsForPlayer(playerName)
									.Where(ac => ac.Balance > 0m)
									.OrderByDescending(ac => ac.Balance);

			//deduct tax from account
			foreach (var acct in playerAccounts)
			{
				if (payment <= 0m)
					break;

				if (payment <= acct.Balance)
				{
					PayTax(acct, payment);
					yield return new Tuple<decimal, BankAccount>(payment, acct);

					payment = 0m;
				}
				else
				{
					var subPayment = acct.Balance;
					PayTax(acct, subPayment);
					payment -= subPayment;

					yield return new Tuple<decimal, BankAccount>(subPayment, acct);
				}
			}

			//at this point, anything not covered is debt.
		}

		/// <summary>
		/// Makes a TaxPayment, pulling funds from each of the players accounts until the payment is satisfied.
		/// It works in order of largest account to smallest.
		/// </summary>
		/// <param name="playerName">Player name.</param>
		/// <param name="payment">Total payment size, in generic units.</param>
		/// <returns>The leftover balance that could not be paid.</returns>
		public decimal PayTax(string playerName, decimal payment)
		{
			//no account has been specified for this player. So...
			//get all accounts for this player, sorted by largest balance
			var playerAccounts = BankingPlugin.Instance.GetAllBankAccountsForPlayer(playerName)
									.Where(ac => ac.Balance > 0m)
									.OrderByDescending(ac => ac.Balance);

			//deduct tax from account
			foreach (var acct in playerAccounts)
			{
				if (payment <= 0m)
					break;

				if (payment <= acct.Balance)
				{
					PayTax(acct, payment);
					payment = 0m;
				}
				else
				{
					var subPayment = acct.Balance;
					PayTax(acct, subPayment);
					payment -= subPayment;
				}
			}

			//at this point, anything not covered is debt.
			return payment;
		}

		public void PayTax(BankAccount sourceAccount, decimal payment)
		{
			if (sourceAccount == null)
				return;

			var remainder = payment;

			if (IsEnabled)
			{
				var taxCollectors = plugin.database.GetTaxCollectors();
				var cut = payment / (taxCollectors.Count > 0 ? taxCollectors.Count : 1m);

				Debug.Print($"Tax payment is {remainder}");
				Debug.Print($"{taxCollectors.Count} tax collectors get {cut} each.");

				//split revenue between the tax collectors.
				foreach (var tc in taxCollectors)
				{
					var collectorAccount = BankingPlugin.Instance.GetBankAccount(tc.PlayerName, sourceAccount.Name);

					if (collectorAccount != null)
					{
						//skip the transfer if the source account belongs to a tax collector 
						if (sourceAccount == collectorAccount)
						{
							remainder -= cut;//still take cut so it doesn't go to world account.
											 //Debug.Print($"Skipping transfer, account belongs to tax collector {tc.PlayerName}");
						}
						else
						{
							sourceAccount.TryTransferTo(collectorAccount, cut);
							remainder -= cut;
							Debug.Print($"Paid {cut} tax to player {tc.PlayerName}'s {collectorAccount.Name} account.");
						}
					}
				}
			}

			//pay balance to world account
			sourceAccount.TryTransferTo(BankingPlugin.Instance.GetWorldAccount(), remainder);
			Debug.Print($"Paid remaining {remainder} tax to world('Server') account.");
		}

		public static void TaxCommand(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var subcommand = parameters.Count > 0 ? parameters[0] : "";
			var plugin = HousingPlugin.Instance;
			var taxService = plugin.TaxService;
			var taxCollectors = plugin.database.GetTaxCollectors();

			if (subcommand.Equals("list", StringComparison.OrdinalIgnoreCase))
			{
				player.SendInfoMessage($"There are {taxCollectors.Count} registered tax collectors.");

				foreach (var tc in taxCollectors)
				{
					player.SendInfoMessage(tc.PlayerName);
				}

				return;
			}
			else if (subcommand.Equals("enable", StringComparison.OrdinalIgnoreCase))
			{
				player.SendInfoMessage($"Tax collectors will now receive taxes.");
				taxService.IsEnabled = true;
				return;
			}
			else if (subcommand.Equals("disable", StringComparison.OrdinalIgnoreCase))
			{
				player.SendInfoMessage($"Tax collectors will no longer receive taxes.");
				taxService.IsEnabled = false;
				return;
			}

			if (parameters.Count == 2)
			{
				var name = parameters[1];

				if (subcommand.Equals("add", StringComparison.OrdinalIgnoreCase))
				{
					var tc = plugin.database.AddTaxCollector(name);
					if (tc == null)
						player.SendErrorMessage($"Unable to add TaxCollector {name}.");

					return;
				}
				else if (subcommand.Equals("remove", StringComparison.OrdinalIgnoreCase))
				{
					var tc = plugin.database.GetTaxCollector(name);
					if (tc != null)
						plugin.database.Remove(tc);

					return;
				}
			}

			ShowCommandSyntax(player);
		}

		private static void ShowCommandSyntax(TSPlayer player)
		{
			if (player == null)
			{
				//is there a better way to report to console??
				Console.WriteLine($"Syntax: {Commands.Specifier}tax list");
				Console.WriteLine($"Syntax: {Commands.Specifier}tax enable");
				Console.WriteLine($"Syntax: {Commands.Specifier}tax disable");
				Console.WriteLine($"Syntax: {Commands.Specifier}tax add <player-name>");
				Console.WriteLine($"Syntax: {Commands.Specifier}tax remove <player-name>");
				return;
			}

			player.SendErrorMessage($"Syntax: {Commands.Specifier}tax list");
			player.SendErrorMessage($"Syntax: {Commands.Specifier}tax enable");
			player.SendErrorMessage($"Syntax: {Commands.Specifier}tax disable");
			player.SendErrorMessage($"Syntax: {Commands.Specifier}tax add <player-name>");
			player.SendErrorMessage($"Syntax: {Commands.Specifier}tax remove <player-name>");
		}
	}
}
