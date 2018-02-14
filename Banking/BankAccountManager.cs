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
		private Dictionary<string, BankAccount> bankAccounts;

		public BankAccount WorldAccount { get; private set; }

		public BankAccountManager()
		{
			bankAccounts = new Dictionary<string, BankAccount>();
			//WorldAccount = GetOrCreateBankAccount(TSPlayer.Server.Name);
			//bankAccounts.Add("World", WorldAccount);//World is the usual alias for the server account 
		}

		public void Load()
		{
			//Debug.Print("BankAccountManager.Load!");
			bankAccounts.Clear();

			Database = new SqliteDatabase(Config.Instance.Database.ConnectionString);

			var accounts = Database.Load();

			foreach(var acc in accounts)
			{
				bankAccounts.Add(acc.OwnerName, acc);
			}
			
			WorldAccount = GetOrCreateBankAccount(TSPlayer.Server.Name);
			//bankAccounts.Add("World", WorldAccount);//World is the usual alias for the server account 
		}

		public void Save()
		{
			//Debug.Print("BankAccountManager.Save!");
			Database.Save(bankAccounts.Values.ToArray());
		}

		public BankAccount GetOrCreateBankAccount(string name)
		{
			BankAccount account = null;

			if( !bankAccounts.TryGetValue(name, out account) )
			{
				Debug.Print($"Creating BankAccount for {name}.");
				account = new BankAccount(name, 100);

				Database.Create(account);

				bankAccounts.Add(name, account);
			}

			return account;
		}
		
		public BankAccount GetBankAccount(string name)
		{
			bankAccounts.TryGetValue(name, out var account);
			return account;
		}

		public BankAccount GetBankAccount(TSPlayer player)
		{
			return GetBankAccount(player.Name);
		}
	}
}
