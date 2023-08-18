using System.Collections.Generic;
using System.Data;
using System.Text;
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

			using (var con = new SqliteConnection(ConnectionString))
			{
				con.Query("CREATE TABLE IF NOT EXISTS BankAccounts (" +
							"WorldId INTEGER," +
							"OwnerName TEXT," +
							"Name TEXT," +
							"Balance REAL," +
							"PRIMARY KEY ( WorldId, OwnerName, Name ) )");
			}
		}

		public void Create(BankAccount account)
		{
			using (var con = new SqliteConnection(ConnectionString))
			{
				con.Query("INSERT INTO BankAccounts ( WorldId, OwnerName, Name, Balance ) " +
							"VALUES ( @0, @1, @2, @3 )",
							Main.worldID, account.OwnerName, account.Name, account.Balance);
			}
		}

		public void Create(IEnumerable<BankAccount> accounts)
		{
			//later use transaction
			foreach (var acc in accounts)
				Create(acc);
		}

		public void Delete(BankAccount account)
		{
			using (var con = new SqliteConnection(ConnectionString))
			{
				con.Query("DELETE FROM BankAccounts " +
							"WHERE WorldId=@0 AND OwnerName=@1 AND Name=@2",
							Main.worldID, account.OwnerName, account.Name);
			}
		}

		public void Delete(IEnumerable<BankAccount> accounts)
		{
			//later use transaction
			foreach (var acc in accounts)
				Delete(acc);
		}

		public void Update(BankAccount account)
		{
			using (var con = new SqliteConnection(ConnectionString))
			{
				con.Query("UPDATE BankAccounts SET Balance = @0 " +
							"WHERE WorldId=@1 AND OwnerName=@2 AND Name=@3",
							account.Balance, Main.worldID, account.OwnerName, account.Name);
			}
		}

		public void Update(IEnumerable<BankAccount> accounts)
		{
			//later use transaction
			foreach (var acc in accounts)
				Update(acc);
		}

		public IEnumerable<BankAccount> Load()
		{
			var results = new List<BankAccount>();

			using (var con = new SqliteConnection(ConnectionString))
			{
				using (var cmd = new SqliteCommand(con))
				{
					cmd.CommandText = "SELECT * FROM BankAccounts WHERE WorldId=@ID";
					cmd.Parameters.Add("@ID", DbType.Int32);
					cmd.Parameters["@ID"].Value = Main.worldID;

					con.Open();

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							while (reader.Read())
							{
								var ownerName = reader.GetString(1);
								var name = reader.GetString(2);
								var balance = reader.GetDecimal(3);

								var account = new BankAccount(ownerName, name, balance);
								results.Add(account);
							}
						}
					}
				}
			}

			return results;
		}

		public void Save(IEnumerable<BankAccount> accounts) => Update(accounts);
	}
}
