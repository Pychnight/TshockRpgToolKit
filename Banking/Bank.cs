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
	public class Bank
	{
		internal IDatabase Database;

		internal CurrencyManager CurrencyManager { get; private set; }
		private Dictionary<string, PlayerBankAccountMap> playerAccountMaps;

		//public BankAccount WorldAccount { get; private set; }

		/// <summary>
		/// Raised when the Bank is checking that needed BankAccounts exist.
		/// </summary>
		public event EventHandler<EnsurePlayerAccountsEventArgs> EnsuringPlayerAccounts;
		
		/// <summary>
		/// Raised when a BankAccount is modified from A Deposit, WithDraw, or Transfer.
		/// </summary>
		public event EventHandler<BalanceChangedEventArgs> BankAccountBalanceChanged;
		
		/// <summary>
		/// Gets all BankAccounts linked to a player.
		/// </summary>
		/// <param name="playerName">Name of player.</param>
		/// <returns>PlayerBankAccountMap.</returns>
		public PlayerBankAccountMap this[string playerName]
		{
			get
			{
				playerAccountMaps.TryGetValue(playerName, out var accountMap);
				return accountMap;
			}
		}

		internal Bank()
		{
			CurrencyManager = new CurrencyManager(Config.Instance.Currency);
			playerAccountMaps = new Dictionary<string, PlayerBankAccountMap>();
			//EnsureBankAccountsExist(TSPlayer.Server.Name);
			//WorldAccount.Get
			//bankAccounts.Add("World", WorldAccount);//World is the usual alias for the server account 

			EnsuringPlayerAccounts += (s,e) =>
			{
				Debug.Print($"Ensuring accounts for player {e.PlayerName}");
			};
		}

		internal void InvokeBalanceChanged(BankAccount bankAccount, decimal newBalance, decimal previousBalance)
		{
			if( BankAccountBalanceChanged != null && bankAccount.OwnerName != "Server" )
			{
				var args = new BalanceChangedEventArgs(bankAccount, newBalance, previousBalance);
				BankAccountBalanceChanged?.Invoke(this, args);
			}
		}

		/// <summary>
		/// Recreates all BankAccounts using the configured Database.
		/// </summary>
		public void Load()
		{
			Debug.Print("BankAccountManager.Load!");
			CurrencyManager = new CurrencyManager(Config.Instance.Currency);
			
			playerAccountMaps.Clear();

			var cfg = Config.Instance.Database;

			//Database = new SqliteDatabase(Config.Instance.Database.ConnectionString);
			//Database = DatabaseFactory.LoadOrCreateDatabase("mysql", "Server=localhost;Database=db_banking;Uid=xxx;Pwd=xxx;");
			//Database = DatabaseFactory.LoadOrCreateDatabase("redis", "localhost:6379");
			Database = DatabaseFactory.LoadOrCreateDatabase(cfg.DatabaseType, cfg.ConnectionString);
			
			var accounts = Database.Load();

			foreach( var acc in accounts )
			{
				PlayerBankAccountMap playerAccounts = null;

				if(!playerAccountMaps.TryGetValue(acc.OwnerName,out playerAccounts))
				{
					playerAccounts = new PlayerBankAccountMap(acc.OwnerName);
					playerAccountMaps.Add(acc.OwnerName, playerAccounts);
				}

				playerAccounts.Add(acc.Name, acc);
			}

			EnsureBankAccountsExist(TSPlayer.Server.Name);

			foreach(var player in TShock.Players)
			{
				if(player?.IsLoggedIn == true) // && player?.Active == true)
				{
					EnsureBankAccountsExist(player.Name);
				}
			}
			
			//WorldAccount = GetOrCreateBankAccount(TSPlayer.Server.Name);
			//bankAccounts.Add("World", WorldAccount);//World is the usual alias for the server account 
		}
		
		/// <summary>
		/// For future expansion.
		/// </summary>
		public void Save()
		{
			//Debug.Print("BankAccountManager.Save!");
			//Database.Save(bankAccounts.Values.ToArray());
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Ensures that a player has BankAccounts for each configured Currency. Also raises the EnsuringPlayerAccounts event, for external plugins to ensure their own BankAccounts exist.
		/// </summary>
		/// <param name="playerName"></param>
		public void EnsureBankAccountsExist(string playerName)
		{
			if( !playerAccountMaps.TryGetValue(playerName, out var playerAccountMap) )
			{
				Debug.Print($"Creating bank account map for user {playerName}...");
				//playerAccountMap = new PlayerBankAccountMap(playerName, CurrencyManager.Definitions.Values);
				playerAccountMap = new PlayerBankAccountMap(playerName);
				playerAccountMaps.Add(playerName, playerAccountMap);
			}
			
			var currencyNames = CurrencyManager.Select(v => v.InternalName);
			playerAccountMap.EnsureBankAccountNamesExist(currencyNames);
			
			//lets set/reset a default mapping from currency's to bank accounts for reward purposes. Anything subscribed to EnsuringPlayerAccounts event
			//will have a chance to change the mapping again.
			foreach(var name in currencyNames)
			{
				playerAccountMap.SetAccountNameOverride(name, name);
			}
			
			if( EnsuringPlayerAccounts != null )
			{
				var args = new EnsurePlayerAccountsEventArgs(playerName, playerAccountMap);
				EnsuringPlayerAccounts(this, args);
			}
		}

		//public BankAccount GetOrCreateBankAccount(string ownerName, string accountName)
		//{
		//	PlayerBankAccountMap accountMap = null;
		//	BankAccount account = null;

		//	if( !playerAccountMaps.TryGetValue(ownerName, out accountMap) )
		//	{
		//		Debug.Print($"Creating bank account types for user {ownerName}...");
		//		//accountMap = new PlayerBankAccountMap(ownerName, CurrencyManager.Definitions.Values);
		//		accountMap = new PlayerBankAccountMap(ownerName);
				
		//		playerAccountMaps.Add(ownerName, accountMap);

		//		//this whole method could be problematic, since it doesnt take into account any currency reward mapping.. hmmm.

		//		//var currencyNames = CurrencyManager.Definitions.Values.Select(v => v.InternalName);
		//		//accountMap.EnsureBankAccountNamesExist(currencyNames);
		//	}

		//	account = accountMap.GetOrCreateBankAccount(accountName,0);

		//	return account;
		//}

		/// <summary>
		/// Tries to get the named BankAccount for the given player, if it exists.
		/// </summary>
		/// <param name="playerName">Player name.</param>
		/// <param name="accountName">BankAccount name.</param>
		/// <returns>BankAccount if found, null otherwise.</returns>
		public BankAccount GetBankAccount(string playerName, string accountName)
		{
			BankAccount bankAccount = null;

			if(playerAccountMaps.TryGetValue(playerName, out var accountMap))
			{
				bankAccount = accountMap.TryGetBankAccount(accountName);
			}
			
			return bankAccount;
		}

		//internal BankAccount TryGetCurrencyRewardAccount(string playerName, string currencyType)
		//{
		//	var accountMap = this[playerName];

		//	if(accountMap!=null)
		//	{
		//		var rewardAccountName = accountMap.GetAccountNameOverride(currencyType);

		//		if(!string.IsNullOrWhiteSpace(rewardAccountName))
		//		{
		//			return accountMap.TryGetBankAccount(rewardAccountName);
		//		}
		//	}
			
		//	return null;
		//}
	}
}
