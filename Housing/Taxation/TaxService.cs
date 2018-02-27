using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
//using Wolfje.Plugins.SEconomy;
//using Wolfje.Plugins.SEconomy.Journal;
using Housing.Extensions;
using Banking;

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
		
		public void PayTax(string sourceAccountName, decimal payment)
		{
			PayTax(BankingPlugin.Instance.GetBankAccount(sourceAccountName, Config.Instance.CurrencyType),payment);
		}

		public void PayTax(BankAccount sourceAccount, decimal payment)
		{
			if( sourceAccount == null )
				return;

			var remainder = payment;

			if( IsEnabled )
			{
				var taxCollectors = plugin.database.GetTaxCollectors();
				var cut = payment / ( taxCollectors.Count > 0 ? taxCollectors.Count : 1m );

				Debug.Print($"Tax payment is {remainder}");
				Debug.Print($"{taxCollectors.Count} tax collectors get {cut.ToMoneyString()} each.");

				//split revenue between the tax collectors.
				foreach( var tc in taxCollectors )
				{
					//var playerAccount = SEconomyPlugin.Instance.WorldAccount;
					var playerAccount = BankingPlugin.Instance.GetBankAccount(tc.PlayerName,Config.Instance.CurrencyType);

					if( playerAccount != null )
					{
						//skip the transfer if the source account belongs to a tax collector 
						if( sourceAccount == playerAccount )
						{
							remainder -= cut;//still take cut so it doesn't go to world account.
							Debug.Print($"Skipping transfer, account belongs to tax collector {tc.PlayerName}");
						}
						else
						{
							sourceAccount.TryTransferTo(playerAccount, (decimal)cut);
							remainder -= cut;
							Debug.Print($"Paid {cut.ToMoneyString()} tax to player {tc.PlayerName}");
						}
					}
				}
			}

			//pay balance to world account
			//sourceAccount.TransferTo(SEconomyPlugin.Instance.WorldAccount, (Money)remainder, options, transactionMessage, journalMessage);
			sourceAccount.TryTransferTo(BankingPlugin.Instance.GetWorldAccount(), remainder);
			Debug.Print($"Paid remaining {remainder.ToMoneyString()} tax to world('Server') account.");
		}

		public void TaxCmd(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var subcommand = parameters.Count > 0 ? parameters[0] : "";
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
					var tc = plugin.database.AddTaxCollector(name);
					if( tc == null )
						player.SendErrorMessage($"Unable to add TaxCollector {name}.");

					return;
				}
				else if (subcommand.Equals("remove", StringComparison.OrdinalIgnoreCase))
				{
					var tc = plugin.database.GetTaxCollector(name);
					if(tc!=null)
						plugin.database.Remove(tc);

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
