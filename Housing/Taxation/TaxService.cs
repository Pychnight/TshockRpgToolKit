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
		HousingPlugin housingPlugin;

		//public static Taxman Instance { get; private set; }
		/// <summary>
		/// Gets or sets whether tax collectors will recieve tax payments.
		/// </summary>
		public bool IsEnabled { get; set; }

		/// <summary>
		/// Gets the list of tax collector player names.
		/// </summary>
		public HashSet<string> TaxCollectorPlayerNames { get; private set; }

		public TaxService(HousingPlugin plugin)
		{
			housingPlugin = plugin;
			TaxCollectorPlayerNames = new HashSet<string>();

			//Instance = this;

			Debug.Print("Created TaxService.");
		}

		/// <summary>
		/// Helper to set player names in one go.
		/// </summary>
		/// <param name="config"></param>
		public void SetPlayerNames(IEnumerable<string> newNames)
		{
			TaxCollectorPlayerNames.Clear();
			newNames?.ForEach(n => TaxCollectorPlayerNames.Add(n));
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
				Debug.Print($"{TaxCollectorPlayerNames.Count} tax collectors get {cut} each.");

				//split revenue between the tax collectors.
				foreach (var playerName in TaxCollectorPlayerNames)
				{
					//var playerAccount = SEconomyPlugin.Instance.WorldAccount;
					var playerAccount = SEconomyPlugin.Instance.RunningJournal.GetBankAccountByName(playerName);
					
					if (playerAccount!=null)
					{
						//skip the transfer if the source account belongs to a tax collector 
						if(sourceAccount==playerAccount)
						{
							remainder -= cut;//still take cut so it doesn't go to world account.
							Debug.Print($"Skipping transfer, account belongs to tax collector {playerName}");
						}
						else
						{
							sourceAccount.TransferTo(playerAccount, (Money)cut, options, transactionMessage, journalMessage);
							remainder -= cut;
							Debug.Print($"Paid {cut} tax to player {playerName}");
						}
					}
				}
			}
			
			//pay balance to world account
			sourceAccount.TransferTo( SEconomyPlugin.Instance.WorldAccount, (Money)remainder, options, transactionMessage, journalMessage);
			Debug.Print($"Paid remaining {remainder} tax to world account.");
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
					//TaxCollectorPlayerNames.Add(name);
					housingPlugin._database.AddTaxCollector(name);
					return;
				}
				else if (subcommand.Equals("remove", StringComparison.OrdinalIgnoreCase))
				{
					//TaxCollectorPlayerNames.Remove(name);
					housingPlugin._database.RemoveTaxCollector(name);
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
