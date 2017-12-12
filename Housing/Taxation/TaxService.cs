using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace Housing
{
	public class TaxService
	{
		//public static Taxman Instance { get; private set; }
		public bool IsEnabled { get; set; }
		public HashSet<string> PlayerNames { get; private set; }

		public TaxService()
		{
			PlayerNames = new HashSet<string>()
			{
				"Tim"
			};

			//Instance = this;

			IsEnabled = true;

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
				var cut = (double)payment / PlayerNames.Count > 0 ? (double)PlayerNames.Count : 1d;
				
				//split revenue between the taxmen.
				foreach(var playerName in PlayerNames)
				{
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
	}
}
