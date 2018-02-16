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

		//SqliteConnection connection;
		public string ConnectionString { get; private set; }
		public string DatabasePath { get; private set; }

		public SqliteSessionRepository(string databasePath)
		{
			try
			{
				//ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, "Opening leveling sessions database...", TraceLevel.Info);

				ConnectionString = $"URI=file:{databasePath}";
				
				using( var connection = new SqliteConnection(ConnectionString) )
				{
					using( var cmd = connection.CreateCommand() )
					{
						cmd.CommandText = createTableSql;

						connection.Open();
						var results = cmd.ExecuteNonQuery();
					}
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
			Debug.Print($"SqliteSessionRepository.Load({userName})");
			SessionDefinition result = null;
			
			using(var connection = new SqliteConnection(ConnectionString))
			{
				using(var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "SELECT data FROM sessions " +
										$"WHERE player='{userName}';";

					connection.Open();
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
			//Debug.Print($"SqliteSessionRepository.Save({userName})");

			using(var connection = new SqliteConnection(ConnectionString))
			{
				//we now try to copy the session definition, in hopes of minimizing "rare" exceptions from definition collections being touched during serialization.
				//we also catch the exception, and just log the error. Hopefully next call to save will work.
				try
				{
					var defCopy = new SessionDefinition(sessionDefinition);
					var json = JsonConvert.SerializeObject(defCopy, Formatting.Indented);

					using( var cmd = connection.CreateCommand() )
					{
						cmd.CommandText = "INSERT OR REPLACE INTO sessions ( player, data ) " +
											"VALUES ( @player, @data );";

						cmd.Parameters.AddWithValue("@player", userName);
						cmd.Parameters.AddWithValue("@data", json);

						connection.Open();

						//Console.WriteLine($"CommandText: {cmd.CommandText}");
						cmd.ExecuteNonQuery();
					}
				}
				catch(Exception ex)
				{
					ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Error: {ex.Message}", TraceLevel.Error);
					ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Session data not saved.", TraceLevel.Error);
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
				isDisposed = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		~SqliteSessionRepository()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
