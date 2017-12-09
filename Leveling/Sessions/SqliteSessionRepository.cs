using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Data.Sqlite;
using TerrariaApi.Server;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Data;

namespace Leveling.Sessions
{
	public class SqliteSessionRepository : IDisposable
	{
		const string createTableSql = @"CREATE TABLE IF NOT EXISTS sessions (
										player text PRIMARY KEY NOT NULL,
										data text )";

		SqliteConnection connection;
		public string DatabasePath { get; private set; }

		public SqliteSessionRepository(string databasePath)
		{
			try
			{
				ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, "Opening leveling sessions database...", TraceLevel.Info);

				var connectionString = $"URI=file:{databasePath}";
				connection = new SqliteConnection(connectionString);
				connection.Open();

				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = createTableSql;
					var results = cmd.ExecuteNonQuery();
				}

			}
			catch(Exception ex)
			{
				ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, "Failed to open leveling sessions database!", TraceLevel.Error);
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		public SessionDefinition Load(string userName)
		{
			ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"SqliteSessionRepository.Load({userName})", TraceLevel.Info);
			SessionDefinition result = null;
			
			if(connection!=null)
			{
				using(var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "SELECT data FROM sessions " +
										$"WHERE player='{userName}';";

					var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

					if(reader.HasRows)
					{
						try
						{
							reader.Read();
							var json = reader.GetString(0);
							result = JsonConvert.DeserializeObject<SessionDefinition>(json);
						}
						catch(Exception ex)
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
			ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"SqliteSessionRepository.Save({userName})", TraceLevel.Info);

			if(connection!=null)
			{
				var json = JsonConvert.SerializeObject(sessionDefinition, Formatting.Indented);
				
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

		#region IDisposable Support
		private bool isDisposed = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				try
				{
					if(connection != null)
					{
						connection.Close();
						connection = null;
					}
				}
				catch(Exception ex)
				{
					ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Error closing database: ({ex.Message})", TraceLevel.Error);
				}

				isDisposed = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~SqliteSessionRepository() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
