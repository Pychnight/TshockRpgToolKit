using Corruption.PluginSupport;
using CustomQuests.Sessions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Terraria;

namespace CustomQuests.Database
{
	public class MySqlJsonDatabase : IDatabase
	{
		internal const string DefaultDatabaseName = "db_quests";

		public string ConnectionString { get; set; }
		public bool Formatted { get; private set; }

		public MySqlJsonDatabase(string connectionString)
		{
			//ConnectionString = connectionString;
			ConnectionString = ensureDatabase(connectionString);

			using (var con = new MySqlConnection(ConnectionString))
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "CREATE TABLE IF NOT EXISTS QuestSessions (" +
										"WorldId INTEGER," +
										"PlayerName VARCHAR(64)," +
										"SessionData TEXT," +
										"PRIMARY KEY ( WorldId, PlayerName ) )";

					con.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		private string ensureDatabase(string connectionString)
		{
			var builder = new MySqlConnectionStringBuilder(connectionString);
			var dbName = string.IsNullOrWhiteSpace(builder.Database) ? DefaultDatabaseName : builder.Database;

			builder.Database = null;

			//try to create db
			using (var con = new MySqlConnection(builder.ConnectionString))
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {dbName}";
				con.Open();
				cmd.ExecuteNonQuery();
			}

			builder.Database = dbName;

			return builder.ConnectionString;
		}

		public SessionInfo Read(string playerName)
		{
			SessionInfo result = null;

			try
			{
				using (var connection = new MySqlConnection(ConnectionString))
				{
					connection.Open();

					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "SELECT SessionData FROM QuestSessions " +
											$"WHERE WorldId=@WorldId and PlayerName=@Player;";

						cmd.Parameters.AddWithValue("@WorldId", Main.worldID);
						cmd.Parameters.AddWithValue("@Player", playerName);

						var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

						if (reader.HasRows)
						{
							reader.Read();

							var json = reader.GetString(0);
							//Console.WriteLine($"json = {json}");
							//var id = reader.GetInt32(0);

							result = JsonConvert.DeserializeObject<SessionInfo>(json);
						}

						reader.Close();
					}
				}
			}
			catch (Exception ex)
			{
				CustomQuestsPlugin.Instance.LogPrint($"Failed to read Session: ({ex.Message})", TraceLevel.Error);
			}

			return result;
		}

		public void Write(SessionInfo sessionInfo, string playerName)
		{
			//Debug.Print($"SqliteJsonDatabase.Write({userName})");

			try
			{
				var json = JsonConvert.SerializeObject(sessionInfo, Formatted ? Formatting.Indented : Formatting.None);
				//Debug.Print(json);

				//temp kludge to hide latency, since ExecuteNonQuery() blocks...
				Task.Run(() =>
				{
					using (var connection = new MySqlConnection(ConnectionString))
					{
						connection.Open();

						using (var cmd = connection.CreateCommand())
						{
							cmd.CommandText = "REPLACE INTO QuestSessions ( WorldId, PlayerName, SessionData ) " +
												"VALUES ( @WorldId, @Player, @Data );";

							cmd.Parameters.AddWithValue("@WorldId", Main.worldID);
							cmd.Parameters.AddWithValue("@Player", playerName);
							cmd.Parameters.AddWithValue("@Data", json);

							cmd.ExecuteNonQuery();
						}
					}
				});
			}
			catch (Exception ex)
			{
				CustomQuestsPlugin.Instance.LogPrint($"Failed to write Session: ({ex.Message})", TraceLevel.Error);
			}
		}
	}
}
