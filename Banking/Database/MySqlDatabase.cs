using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Banking.Database
{
	internal class MySqlDatabase : IDatabase
	{
		public string ConnectionString { get; set; }

		public MySqlDatabase(string connectionString)
		{
			ConnectionString = connectionString;

			using( var con = new MySqlConnection(ConnectionString) )
			{
				using( var cmd = con.CreateCommand() )
				{
					cmd.CommandText = "CREATE TABLE IF NOT EXISTS BankAccounts (" +
										"WorldId INTEGER," +
										"OwnerName VARCHAR(128),"  +
										"CurrencyType VARCHAR(128)," +
										"Balance REAL," +
										"PRIMARY KEY ( WorldId, OwnerName, CurrencyType ) )";

					con.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void Create(BankAccount account)
		{
			using( var con = new MySqlConnection(ConnectionString) )
			{
				using( var cmd = con.CreateCommand() )
				{
					cmd.CommandText = "INSERT INTO BankAccounts ( WorldId, OwnerName, CurrencyType, Balance ) " +
										"VALUES ( @WORLDID, @OWNERNAME, @CURRENCYTYPE, @BALANCE )";

					cmd.Parameters.AddWithValue("@WORLDID", Main.worldID);
					cmd.Parameters.AddWithValue("@OWNERNAME", account.OwnerName);
					cmd.Parameters.AddWithValue("@CURRENCYTYPE", account.CurrencyType);
					cmd.Parameters.AddWithValue("@BALANCE", account.Balance);

					con.Open();
					cmd.ExecuteNonQuery();
				}
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
			using( var con = new MySqlConnection(ConnectionString) )
			{
				using( var cmd = con.CreateCommand() )
				{
					cmd.CommandText = "DELETE FROM BankAccounts " +
										"WHERE WorldId=@WORLDID AND OwnerName=@OWNERNAME AND CurrencyType=@CURRENCYTYPE";
					
					cmd.Parameters.AddWithValue("@WORLDID", Main.worldID);
					cmd.Parameters.AddWithValue("@OWNERNAME", account.OwnerName);
					cmd.Parameters.AddWithValue("@CURRENCYTYPE", account.CurrencyType);
					
					con.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void Delete(IEnumerable<BankAccount> accounts)
		{
			//later use transaction
			foreach( var acc in accounts )
				Delete(acc);
		}

		public IEnumerable<BankAccount> Load()
		{
			var results = new List<BankAccount>();

			using( var con = new MySqlConnection(ConnectionString) )
			{
				using( var cmd = con.CreateCommand() )
				{
					cmd.CommandText = "SELECT * FROM BankAccounts WHERE WorldId=@ID";
					cmd.Parameters.AddWithValue("@ID", Main.worldID);

					con.Open();

					using( var reader = cmd.ExecuteReader() )
					{
						if( reader.HasRows )
						{
							while( reader.Read() )
							{
								var ownerName = reader.GetString(1);
								var currencyType = reader.GetString(2);
								var balance = reader.GetDecimal(3);

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

		public void Update(BankAccount account)
		{
			using( var con = new MySqlConnection(ConnectionString) )
			{
				using( var cmd = con.CreateCommand() )
				{
					cmd.CommandText = "UPDATE BankAccounts SET Balance = @BALANCE " +
										"WHERE WorldId=@WORLDID AND OwnerName=@OWNERNAME AND CurrencyType=@CURRENCYTYPE";

					cmd.Parameters.AddWithValue("@BALANCE", account.Balance);
					cmd.Parameters.AddWithValue("@WORLDID", Main.worldID);
					cmd.Parameters.AddWithValue("@OWNERNAME", account.OwnerName);
					cmd.Parameters.AddWithValue("@CURRENCYTYPE", account.CurrencyType);

					con.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void Update(IEnumerable<BankAccount> accounts)
		{
			//later use transaction
			foreach( var acc in accounts )
				Update(acc);
		}
	}
}
