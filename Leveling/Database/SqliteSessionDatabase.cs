using Leveling.Database;
using Leveling.Sessions;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using TerrariaApi.Server;

namespace Leveling
{
	public class SqliteSessionDatabase : ISessionDatabase
	{
		const string createTableSql = @"CREATE TABLE IF NOT EXISTS sessions (
										player text PRIMARY KEY NOT NULL,
										data text )";

		public string ConnectionString { get; private set; }

		public SqliteSessionDatabase(string connectionString)
		{
			try
			{
				ConnectionString = connectionString;

				using (var connection = new SqliteConnection(ConnectionString))
				{
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = createTableSql;

						connection.Open();
						var results = cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, "Failed to open leveling sessions database!", TraceLevel.Error);
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		public SessionDefinition Load(string userName)
		{
			Debug.Print($"SqliteSessionRepository.Load({userName})");
			SessionDefinition result = null;

			using (var connection = new SqliteConnection(ConnectionString))
			{
				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "SELECT data FROM sessions " +
										$"WHERE player='{userName}';";

					connection.Open();
					var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

					if (reader.HasRows)
					{
						try
						{
							reader.Read();
							var json = reader.GetString(0);
							result = JsonConvert.DeserializeObject<SessionDefinition>(json);
						}
						catch (Exception ex)
						{
							ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Load error: ({ex.Message})", TraceLevel.Error);
						}
					}
				}
			}

			return result;
		}

		public void Save(string userName, SessionDefinition sessionDefinition)
		{
			//Debug.Print($"SqliteSessionRepository.Save({userName})");
			using (var connection = new SqliteConnection(ConnectionString))
			{
				//we now try to copy the session definition, in hopes of minimizing "rare" exceptions from definition collections being touched during serialization.
				//we also catch the exception, and just log the error. Hopefully next call to save will work.
				try
				{
					var defCopy = new SessionDefinition(sessionDefinition);
					var json = JsonConvert.SerializeObject(defCopy, Formatting.Indented);

					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "INSERT OR REPLACE INTO sessions ( player, data ) " +
											"VALUES ( @player, @data );";

						cmd.Parameters.AddWithValue("@player", userName);
						cmd.Parameters.AddWithValue("@data", json);

						connection.Open();
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Error: {ex.Message}", TraceLevel.Error);
					ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Session data not saved.", TraceLevel.Error);
				}
			}
		}
	}
}
