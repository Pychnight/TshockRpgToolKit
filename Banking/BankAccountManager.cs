using Banking.Configuration;
using Banking.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking
{
	public class BankAccountManager
	{
		internal IDatabase Database;

		internal CurrencyManager CurrencyManager { get; private set; }
		private Dictionary<string, BankAccountTypeMap> bankAccounts;

		//public BankAccount WorldAccount { get; private set; }

		public BankAccountManager()
		{
			CurrencyManager = new CurrencyManager(Config.Instance.Currency);
			bankAccounts = new Dictionary<string, BankAccountTypeMap>();
			EnsureBankAccountsExist(TSPlayer.Server.Name);
			//WorldAccount.Get
			//bankAccounts.Add("World", WorldAccount);//World is the usual alias for the server account 
		}

		public void Load()
		{
			Debug.Print("BankAccountManager.Load!");
			CurrencyManager = new CurrencyManager(Config.Instance.Currency);

			//disabled

			//bankAccounts.Clear();

			//Database = new SqliteDatabase(Config.Instance.Database.ConnectionString);

			//var accounts = Database.Load();

			//foreach(var acc in accounts)
			//{
			//	bankAccounts.Add(acc.OwnerName, acc);
			//}
			
			//WorldAccount = GetOrCreateBankAccount(TSPlayer.Server.Name);
			//bankAccounts.Add("World", WorldAccount);//World is the usual alias for the server account 
		}

		public void Save()
		{
			Debug.Print("BankAccountManager.Save!");
			//Database.Save(bankAccounts.Values.ToArray());
		}

		public void EnsureBankAccountsExist(string name)
		{
			if( !bankAccounts.TryGetValue(name, out var accountTypes) )
			{
				Debug.Print($"Creating bank account types for user {name}...");
				accountTypes = new BankAccountTypeMap(name, CurrencyManager.Definitions.Values);

				//Database.Create(accountTypes);
				bankAccounts.Add(name, accountTypes);
			}
			else
			{
				accountTypes.EnsureBankAccountTypesExist(CurrencyManager.Definitions.Values);
			}
		}

		public BankAccount GetOrCreateBankAccount(string name, string accountType)
		{
			BankAccountTypeMap accountTypes = null;
			BankAccount account = null;

			if( !bankAccounts.TryGetValue(name, out accountTypes) )
			{
				Debug.Print($"Creating bank account types for user {name}...");
				accountTypes = new BankAccountTypeMap(name, CurrencyManager.Definitions.Values);

				//Database.Create(accountTypes);
				bankAccounts.Add(name, accountTypes);
			}

			account = accountTypes.GetOrCreateBankAccount(name,accountType,0);

			return account;
		}

		public BankAccount GetBankAccount(string name, string accountType)
		{
			if(bankAccounts.TryGetValue(name, out var accountTypes))
			{
				if(accountTypes.TryGetValue(accountType, out var bankAccount))
				{
					return bankAccount;
				}
			}
			
			return null;
		}

		//public BankAccount GetBankAccount(TSPlayer player, string accountType)
		//{
		//	return GetBankAccount(player.Name);
		//}
	}
}
