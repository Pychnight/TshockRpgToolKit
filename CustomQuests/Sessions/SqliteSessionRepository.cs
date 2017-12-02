using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using Newtonsoft.Json;

namespace CustomQuests.Sessions
{
    internal class SqliteSessionRepository : SessionRepository, IDisposable
    {
		const string createTableSql = @"CREATE TABLE IF NOT EXISTS sessions (
										player text PRIMARY KEY NOT NULL,
										data text )";
		string connectionString;
		SqliteConnection connection;
		
		internal string DatabasePath { get; private set; }

		internal SqliteSessionRepository(string databasePath)
		{
			DatabasePath = databasePath;
						
			try
			{
				connectionString = $"URI=file:{databasePath}";
				connection = new SqliteConnection(connectionString);

				Console.WriteLine("Opening quest sessions database...");
				connection.Open();

				//if table does not exist, create it.
				using(var cmd = connection.CreateCommand())
				{
					cmd.CommandText = createTableSql;
					var results = cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to open quest sessions database!");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		internal override void OnDispose(bool isDisposing)
		{
			if(connection!=null)
			{
				Console.WriteLine("Closing quest sessions database connection.");
				connection.Close();
			}
		}

		internal override SessionInfo Load(string userName)
        {
			Console.WriteLine($"SqliteSessionRepo.Load({userName})");
			SessionInfo result = null;
			
			if(connection!=null)
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "SELECT data FROM sessions " +
										$"WHERE player='{userName}';";

					var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
					
					if(reader.HasRows)
					{
						//Console.WriteLine("Has rows!");

						reader.Read();

						try
						{
							var json = reader.GetString(0);

							//Console.WriteLine($"json = {json}");

							result = JsonConvert.DeserializeObject<SessionInfo>(json);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Load error: {ex.Message}");
						}
					}

					reader.Close();
				}
			}

			return result;
        }

		internal override void Save(SessionInfo sessionInfo, string userName)
		{
			Console.WriteLine($"SqliteSessionRepo.Save [sessionInfo] ({userName})");
			
			if(connection!=null)
			{
				var json = JsonConvert.SerializeObject(sessionInfo, Formatting.Indented);
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "INSERT OR REPLACE INTO sessions ( player, data ) " +
										$"VALUES ('{userName}','{json}');";

					cmd.ExecuteNonQuery();
				}
			}
		}

		internal override void Save(Session session, string userName)
        {
			Console.WriteLine($"SqliteSessionRepo.Save [session] ({userName})");
			
			if(connection!=null)
			{
				var json = JsonConvert.SerializeObject(session, Formatting.Indented);

				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "INSERT OR REPLACE INTO sessions ( player, data ) " +
										$"VALUES ('{userName}','{json}');";

					//Console.WriteLine($"CommandText: {cmd.CommandText}");

					cmd.ExecuteNonQuery();
				}
			}
        }
	}
}
