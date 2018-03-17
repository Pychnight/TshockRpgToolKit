using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leveling.Sessions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TerrariaApi.Server;

namespace Leveling.Database
{
	public class MySqlSessionDatabase : ISessionDatabase
	{
		const string DefaultDatabaseName = "db_leveling";

		const string createTableSql = @"CREATE TABLE IF NOT EXISTS sessions (
										player VARCHAR(80) PRIMARY KEY NOT NULL,
										data TEXT )";

		public string ConnectionString { get; private set; }

		public MySqlSessionDatabase(string connectionString)
		{
			try
			{
				ConnectionString = ensureDatabase(connectionString);

				using( var connection = new MySqlConnection(ConnectionString) )
				{
					using( var cmd = connection.CreateCommand() )
					{
						cmd.CommandText = createTableSql;

						connection.Open();
						var results = cmd.ExecuteNonQuery();
					}
				}
			}
			catch( Exception ex )
			{
				ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, "Failed to open leveling sessions database!", TraceLevel.Error);
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		private string ensureDatabase(string connectionString)
		{
			var builder = new MySqlConnectionStringBuilder(connectionString);
			var dbName = string.IsNullOrWhiteSpace(builder.Database) ? DefaultDatabaseName : builder.Database;

			builder.Database = null;

			//try to create db
			using( var con = new MySqlConnection(builder.ConnectionString) )
			using( var cmd = con.CreateCommand() )
			{
				cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {dbName}";
				con.Open();
				cmd.ExecuteNonQuery();
			}

			builder.Database = dbName;

			return builder.ConnectionString;
		}

		public SessionDefinition Load(string userName)
		{
			Debug.Print($"MySqlSessionRepository.Load({userName})");
			SessionDefinition result = null;

			using( var connection = new MySqlConnection(ConnectionString) )
			{
				using( var cmd = connection.CreateCommand() )
				{
					cmd.CommandText = "SELECT data FROM sessions " +
										$"WHERE player='{userName}';";

					connection.Open();
					var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

					if( reader.HasRows )
					{
						try
						{
							reader.Read();
							var json = reader.GetString(0);
							result = JsonConvert.DeserializeObject<SessionDefinition>(json);
						}
						catch( Exception ex )
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
			Debug.Print($"MySqlSessionRepository.Save({userName})");
			using( var connection = new MySqlConnection(ConnectionString) )
			{
				//we now try to copy the session definition, in hopes of minimizing "rare" exceptions from definition collections being touched during serialization.
				//we also catch the exception, and just log the error. Hopefully next call to save will work.
				try
				{
					var defCopy = new SessionDefinition(sessionDefinition);
					var json = JsonConvert.SerializeObject(defCopy, Formatting.Indented);

					using( var cmd = connection.CreateCommand() )
					{
						cmd.CommandText = "REPLACE INTO sessions ( player, data ) " +
											"VALUES ( @player, @data );";

						cmd.Parameters.AddWithValue("@player", userName);
						cmd.Parameters.AddWithValue("@data", json);

						connection.Open();
						cmd.ExecuteNonQuery();
					}
				}
				catch( Exception ex )
				{
					ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Error: {ex.Message}", TraceLevel.Error);
					ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Session data not saved.", TraceLevel.Error);
				}
			}
		}
	}
}
