using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	/// <summary>
	/// Internal collection of a player's bank accounts, keyed by account name.
	/// </summary>
	public class PlayerBankAccountMap : IEnumerable<BankAccount>
	{
		Dictionary<string, BankAccount> accountsByName;

		/// <summary>
		///		Gets a Dictionary which can be used to map account names to other BankAccounts owned by the player, for rewards and penalities purposes. 
		/// </summary>
		internal Dictionary<string, BankAccount> AccountNameOverrideMap { get; private set; }

		public string PlayerName { get; internal set; }

		public BankAccount this[string name]
		{
			get { return TryGetBankAccount(name); }
			set { accountsByName[name] = value; }
		}

		internal PlayerBankAccountMap(string playerName)
		{
			PlayerName = playerName;
			accountsByName = new Dictionary<string, BankAccount>();
			AccountNameOverrideMap = new Dictionary<string, BankAccount>();
		}

		//internal PlayerBankAccountMap(string playerName, IEnumerable<CurrencyDefinition> definitions) : this(playerName)
		//{
		//	foreach(var def in definitions)
		//	{
		//		var account = new BankAccount(playerName, def.InternalName, 0m);
		//		BankingPlugin.Instance.Bank.Database.Create(account);
		//		accountsByName.Add(def.InternalName, account);

		//		CurrencyRewardMap.Add(def.InternalName, account);
		//	}
		//}

		public BankAccount GetOrCreateBankAccount(string accountName, decimal startingAmount = 0.0m)
		{
			var account = TryGetBankAccount(accountName);

			if(account==null)
			{
				account = new BankAccount(PlayerName, accountName, startingAmount);
				BankingPlugin.Instance.Bank.Database.Create(account);
				Add(accountName, account);
			}

			return account;
		}

		public BankAccount TryGetBankAccount(string accountName)
		{
			if(AccountNameOverrideMap.TryGetValue(accountName, out var account))
				return account;

			accountsByName.TryGetValue(accountName, out account);
			return account;
		}

		internal void Add(string accountName, BankAccount account)
		{
			accountsByName.Add(accountName, account);
		}

		internal void EnsureBankAccountNamesExist(IEnumerable<string> accountNames)
		{
			foreach(var name in accountNames)
			{
				if(!accountsByName.TryGetValue(name,out var bankAccount))
				{
					bankAccount = new BankAccount(PlayerName, name, 0);
					BankingPlugin.Instance.Bank.Database.Create(bankAccount);
					Add(name, bankAccount);
				}
			}
		}
		
		/// <summary>
		/// Reroutes an account name to a specified BankAccount, if it exists for the player.
		/// </summary>
		/// <param name="overrideName">Overridden name.</param>
		/// <param name="accountName">BankAccount name.</param>
		public void SetAccountNameOverride(string overrideName, string accountName)
		{
			if( string.IsNullOrWhiteSpace(accountName) )
			{
				AccountNameOverrideMap.Remove(overrideName);
				return;
			}

			var account = TryGetBankAccount(accountName);
			AccountNameOverrideMap[overrideName] = account;
		}

		/// <summary>
		/// Returns the name of the Player's BankAccount which is routed to, using the given name.
		/// </summary>
		/// <param name="overrideName">Overriden name.</param>
		/// <returns>BankAccount name.</returns>
		public string GetAccountNameOverride(string overrideName)
		{
			AccountNameOverrideMap.TryGetValue(overrideName, out var account);
			return account?.Name;
		}

		public void ClearAccountNameOverrides()
		{
			AccountNameOverrideMap.Clear();
		}

		public IEnumerator<BankAccount> GetEnumerator()
		{
			return accountsByName.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return accountsByName.Values.GetEnumerator();
		}

		//public BankAccount GetAccountForCurrencyReward(string currencyType)
		//{
		//	AccountNameOverrideMap.TryGetValue(currencyType, out var account);
		//	return account;
		//}
	}
}
