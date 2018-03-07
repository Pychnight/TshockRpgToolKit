using System;

namespace Banking
{
	public class EnsurePlayerAccountsEventArgs : EventArgs
	{
		public string PlayerName { get; private set; }
		public PlayerBankAccountMap BankAccounts { get; private set; }

		internal EnsurePlayerAccountsEventArgs(string playerName, PlayerBankAccountMap bankAccounts)
		{
			PlayerName = playerName;
			BankAccounts = bankAccounts;
		}
	}
}