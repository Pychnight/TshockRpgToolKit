using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using Terraria;
using TShockAPI.DB;

namespace Banking.Database
{
	internal class SqliteDatabase : IDatabase
	{
		object locker = new object();

		public string ConnectionString { get; set; }

		public SqliteDatabase(string connectionString)
		{
			ConnectionString = connectionString;

			using(var con = new SqliteConnection(ConnectionString))
			{
				con.Query("CREATE TABLE IF NOT EXISTS BankAccounts (" +
							"WorldId INTEGER," +
							"OwnerName TEXT," +
							"CurrencyType TEXT," +
							"Balance REAL," +
							"PRIMARY KEY ( WorldId, OwnerName, CurrencyType ) )");
			}
		}

		public void Create(BankAccount account)
		{
			using( var con = new SqliteConnection(ConnectionString) )
			{
				con.Query("INSERT INTO BankAccounts ( WorldId, OwnerName, CurrencyType, Balance ) " +
							"VALUES ( @0, @1, @2, @3 )",
							Main.worldID, account.OwnerName, account.CurrencyType, account.Balance);
			}
		}

		public void Create(IEnumerable<BankAccount> accounts)
		{
			//later use transaction
			foreach( var acc in accounts )
				Create(acc);
		}

		public void Delete(BankAccount account)
		{
			using( var con = new SqliteConnection(ConnectionString) )
			{
				con.Query("DELETE FROM BankAccounts " +
							"WHERE WorldId=@0 AND OwnerName=@1 AND CurrencyType=@2",
							Main.worldID, account.OwnerName, account.CurrencyType);
			}
		}

		public void Delete(IEnumerable<BankAccount> accounts)
		{
			//later use transaction
			foreach( var acc in accounts )
				Delete(acc);
		}
		
		public void Update(BankAccount account)
		{
			using( var con = new SqliteConnection(ConnectionString) )
			{
				con.Query("UPDATE BankAccounts SET Balance = @0 " +
							"WHERE WorldId=@1 AND OwnerName=@2 AND CurrencyType=@3",
							account.Balance, Main.worldID, account.OwnerName, account.CurrencyType);
			}
		}

		public void Update(IEnumerable<BankAccount> accounts)
		{
			//later use transaction
			foreach( var acc in accounts )
				Update(acc);
		}

		public IEnumerable<BankAccount> Load()
		{
			var results = new List<BankAccount>();

			using( var con = new SqliteConnection(ConnectionString))
			{
				using( var cmd = new SqliteCommand(con) )
				{
					cmd.CommandText = "SELECT * FROM BankAccounts WHERE WorldId=@ID";
					cmd.Parameters.Add("@ID", DbType.Int32);
					cmd.Parameters["@ID"].Value = Main.worldID;

					con.Open();

					using( var reader = cmd.ExecuteReader() )
					{
						if(reader.HasRows)
						{
							while(reader.Read())
							{
								var ownerName = reader.GetString(1);
								var currencyType = reader.GetString(2);
								var balance = reader.GetDecimal(3);

								//throw new NotImplementedException();

								var account = new BankAccount(ownerName, currencyType, balance);
								results.Add(account);
							}
						}
					}
				}
			}
			
			return results;
		}

		public void Save(IEnumerable<BankAccount> accounts)
		{
			Update(accounts);
		}
	}
}
