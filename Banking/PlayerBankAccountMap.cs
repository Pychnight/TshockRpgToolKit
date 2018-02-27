using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	/// <summary>
	/// Internal collection of a player's bank accounts, keyed by Currency type.
	/// </summary>
	internal class PlayerBankAccountMap : Dictionary<string,BankAccount>
	{
		internal string OwnerName { get; set; }

		internal PlayerBankAccountMap(string ownerName)
		{
			OwnerName = ownerName;
		}

		internal PlayerBankAccountMap(string ownerName, IEnumerable<CurrencyDefinition> definitions)
		{
			OwnerName = ownerName;
			
			foreach(var def in definitions)
			{
				var account = new BankAccount(ownerName, def.InternalName, 0m);
				BankingPlugin.Instance.BankAccountManager.Database.Create(account);
				Add(def.InternalName, account);
			}
		}

		internal BankAccount GetOrCreateBankAccount(string ownerName, string accountType, decimal amount)
		{
			if( ownerName != OwnerName )
				throw new ArgumentException("ownerName does not match OwnerName.");//this should never happen..

			if(!TryGetValue(accountType, out var account))
			{
				account = new BankAccount(ownerName, accountType, amount);
				BankingPlugin.Instance.BankAccountManager.Database.Create(account);
				Add(accountType, account);
			}

			return account;
		}

		internal void EnsureBankAccountTypesExist(IEnumerable<CurrencyDefinition> values)
		{
			foreach(var def in values)
			{
				if(!TryGetValue(def.InternalName,out var bankAccount))
				{
					bankAccount = new BankAccount(OwnerName, def.InternalName, 0);
					BankingPlugin.Instance.BankAccountManager.Database.Create(bankAccount);
					Add(def.InternalName, bankAccount);
				}
			}
		}
	}
}
