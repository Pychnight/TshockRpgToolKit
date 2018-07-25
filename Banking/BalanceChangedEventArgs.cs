using System;
using System.Diagnostics;

namespace Banking
{
	public class BalanceChangedEventArgs : EventArgs
	{
		public BankAccount BankAccount { get; private set; }
		public string OwnerName => BankAccount.OwnerName;
		public string AccountName => BankAccount.Name;
		public decimal NewBalance { get; private set; }
		public decimal PreviousBalance { get; private set; }
		public decimal Change => NewBalance - PreviousBalance;

		internal BalanceChangedEventArgs(BankAccount bankAccount, ref decimal newBalance, ref decimal previousBalance)
		{
			BankAccount = bankAccount;
			NewBalance = newBalance;
			PreviousBalance = previousBalance;
		}
	}
}