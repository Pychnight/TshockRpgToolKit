using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace Housing
{
	public class TaxService
	{
		//public static Taxman Instance { get; private set; }
		/// <summary>
		/// Gets or sets whether tax collectors will recieve tax payments.
		/// </summary>
		public bool IsEnabled { get; set; }

		/// <summary>
		/// Gets the list of tax collector player names.
		/// </summary>
		public HashSet<string> TaxCollectorPlayerNames { get; private set; }

		public TaxService(Config config)
		{
			TaxCollectorPlayerNames = new HashSet<string>();

			if(config!=null)
			{
				IsEnabled = config.EnableTaxService;

				if(config.TaxCollectorPlayerNames!=null)
				{
					config.TaxCollectorPlayerNames.ForEach(n => TaxCollectorPlayerNames.Add(n));
				}
			}
			else
			{
				IsEnabled = false;
			}
			
			//Instance = this;
			
			Debug.Print("Created TaxService.");
		}

		public void PayTax(string sourceAccountName, Money payment, BankAccountTransferOptions options = BankAccountTransferOptions.None, string transactionMessage = "", string journalMessage = "")
		{
			PayTax(SEconomyPlugin.Instance?.RunningJournal.GetBankAccountByName(sourceAccountName),
					payment);
		}

		public void PayTax(IBankAccount sourceAccount, Money payment, BankAccountTransferOptions options = BankAccountTransferOptions.None, string transactionMessage = "", string journalMessage = "")
		{
			if(sourceAccount == null)
				return;

			var remainder = (double)payment;

			if(IsEnabled)
			{
				var cut = (double)payment / ( TaxCollectorPlayerNames.Count > 0 ? (double)TaxCollectorPlayerNames.Count : 1d );

				Debug.Print($"Tax payment is {remainder}");
				Debug.Print($"Tax collectors get {cut} each.");

				//split revenue between the taxmen.
				foreach (var playerName in TaxCollectorPlayerNames)
				{
					//var playerAccount = SEconomyPlugin.Instance.WorldAccount;
					//players have to be online to collect tax. Maybe there is a better way to get accounts?
					var playerAccount = SEconomyPlugin.Instance.GetPlayerBankAccount(playerName);

					if (playerAccount!=null)
					{
						sourceAccount.TransferTo(playerAccount, (Money)cut, options, transactionMessage, journalMessage);
						remainder -= cut;
						Debug.Print($"Paid {cut} tax to player {playerName}");
					}
				}
			}
			
			//pay balance to world account
			sourceAccount.TransferTo( SEconomyPlugin.Instance.WorldAccount, (Money)remainder, options, transactionMessage, journalMessage);
			Debug.Print($"Paid {remainder} tax to world account.");
		}

		public void TaxCmd(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var subcommand = parameters.Count > 0 ? parameters[0] : "";

			if (subcommand.Equals("list", StringComparison.OrdinalIgnoreCase))
			{
				player.SendInfoMessage($"There are {TaxCollectorPlayerNames.Count} registered tax collectors.");

				foreach (var name in TaxCollectorPlayerNames)
				{
					player.SendInfoMessage(name);
				}

				return;
			}
			else if(subcommand.Equals("enable", StringComparison.OrdinalIgnoreCase))
			{
				player.SendInfoMessage($"Tax collectors will now receive taxes.");
				IsEnabled = true;
				return;
			}
			else if (subcommand.Equals("disable", StringComparison.OrdinalIgnoreCase))
			{
				player.SendInfoMessage($"Tax collectors will no longer receive taxes.");
				IsEnabled = false;
				return;
			}

			if (parameters.Count == 2)
			{
				var name = parameters[1];

				if (subcommand.Equals("add", StringComparison.OrdinalIgnoreCase))
				{
					TaxCollectorPlayerNames.Add(name);
					return;
				}
				else if (subcommand.Equals("remove", StringComparison.OrdinalIgnoreCase))
				{
					TaxCollectorPlayerNames.Remove(name);
					return;
				}
			}

			ShowCommandSyntax(player);
		}

		private void ShowCommandSyntax(TSPlayer player)
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
