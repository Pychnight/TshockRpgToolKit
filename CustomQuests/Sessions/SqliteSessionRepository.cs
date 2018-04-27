using Mono.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Diagnostics;
using TerrariaApi.Server;

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

		internal SqliteSessionRepository(string databasePath, TerrariaPlugin plugin)
		{
			this.plugin = plugin;

			DatabasePath = databasePath;
						
			try
			{
				connectionString = $"URI=file:{databasePath}";
				connection = new SqliteConnection(connectionString);
								
				ServerApi.LogWriter.PluginWriteLine(plugin, "Opening quest sessions database...", TraceLevel.Info);
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
				ServerApi.LogWriter.PluginWriteLine(plugin, "Failed to open quest sessions database!", TraceLevel.Error);
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		internal override void OnDispose(bool isDisposing)
		{
			if(connection!=null)
			{
				try
				{
					ServerApi.LogWriter.PluginWriteLine(plugin, "Closing quest sessions database connection...", TraceLevel.Info);
					connection.Close();
				}
				catch(Exception ex)
				{
					ServerApi.LogWriter.PluginWriteLine(plugin, "Closing quest sessions database failed!", TraceLevel.Error);
					ServerApi.LogWriter.PluginWriteLine(plugin, ex.Message, TraceLevel.Error);
					ServerApi.LogWriter.PluginWriteLine(plugin, ex.StackTrace, TraceLevel.Error);
				}
			}
		}

		internal override SessionInfo Load(string userName)
        {
			Debug.Print($"SqliteSessionRepo.Load({userName})");
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
						reader.Read();

						try
						{
							var json = reader.GetString(0);
							//Console.WriteLine($"json = {json}");

							result = JsonConvert.DeserializeObject<SessionInfo>(json);
						}
						catch (Exception ex)
						{
							ServerApi.LogWriter.PluginWriteLine(plugin, $"Load error: ({ex.Message})", TraceLevel.Error);
						}
					}

					reader.Close();
				}
			}

			return result;
        }

		internal override void Save(SessionInfo sessionInfo, string userName)
		{
			Debug.Print($"SqliteSessionRepo.Save [sessionInfo] ({userName})");

			if (connection!=null)
			{
				var json = JsonConvert.SerializeObject(sessionInfo, Formatting.Indented);

				Debug.Print(json);

				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "INSERT OR REPLACE INTO sessions ( player, data ) " +
										"VALUES ( @player, @data );";

					cmd.Parameters.AddWithValue("@player", userName);
					cmd.Parameters.AddWithValue("@data", json);
					
					cmd.ExecuteNonQuery();
				}
			}
		}

		internal override void Save(Session session, string userName)
        {
			Debug.Print($"SqliteSessionRepo.Save [session] ({userName})");

			if (connection!=null)
			{
				var json = JsonConvert.SerializeObject(session, Formatting.Indented);

				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "INSERT OR REPLACE INTO sessions ( player, data ) " +
										"VALUES ( @player, @data );";

					cmd.Parameters.AddWithValue("@player", userName);
					cmd.Parameters.AddWithValue("@data", json);

					//Console.WriteLine($"CommandText: {cmd.CommandText}");
					cmd.ExecuteNonQuery();
				}
			}
        }
	}
}
