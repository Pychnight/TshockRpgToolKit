using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	internal class BankAccountTypeMap : Dictionary<string,BankAccount>
	{
		internal string OwnerName { get; set; }

		internal BankAccountTypeMap(string ownerName, IEnumerable<CurrencyDefinition> definitions)
		{
			OwnerName = ownerName;

			foreach(var def in definitions)
			{
				var account = new BankAccount(ownerName, def.InternalName, 0m);
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
					Add(def.InternalName, bankAccount);
				}
			}
		}
	}
}
