using System;
using System.Diagnostics;

namespace Banking
{
	public class BalanceChangedEventArgs : EventArgs
	{
		//public BankAccount BankAccount { get; private set; }
		public string OwnerName { get; private set; }
		public string Name { get; private set; }
		//public string CurrencyType { get; private set; }
		public decimal NewBalance { get; private set; }
		public decimal PreviousBalance { get; private set; }
		public decimal Change => NewBalance - PreviousBalance;

		internal BalanceChangedEventArgs(BankAccount bankAccount, decimal newBalance, decimal previousBalance)
		{
			//BankAccount = bankAccount;
			OwnerName = bankAccount.OwnerName;
			Name = bankAccount.Name;
			//CurrencyType = bankAccount.CurrencyType;
			NewBalance = newBalance;
			PreviousBalance = previousBalance;
		}
	}
}