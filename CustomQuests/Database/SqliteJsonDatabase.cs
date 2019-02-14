using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corruption.PluginSupport;
using CustomQuests.Sessions;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using Terraria;
using TShockAPI.DB;

namespace CustomQuests.Database
{
	public class SqliteJsonDatabase : IDatabase
	{
		public string ConnectionString { get; set; }
		public bool Formatted { get; private set; }

		public SqliteJsonDatabase(string connectionString)
		{
			ConnectionString = connectionString;

			using( var con = new SqliteConnection(ConnectionString) )
			{
				con.Query("CREATE TABLE IF NOT EXISTS QuestSessions (" +
							"WorldId INTEGER," +
							"PlayerName TEXT, " +
							"SessionData TEXT, " +
							"PRIMARY KEY(WorldId,PlayerName)" +
							")");
			}
		}
		
		public SessionInfo Read(string playerName)
		{
			SessionInfo result = null;

			try
			{
				using( var connection = new SqliteConnection(ConnectionString) )
				{
					connection.Open();

					using( var cmd = connection.CreateCommand() )
					{
						cmd.CommandText = "SELECT SessionData FROM QuestSessions " +
											$"WHERE WorldId=@WorldId and PlayerName=@Player;";

						cmd.Parameters.AddWithValue("@WorldId", Main.worldID);
						cmd.Parameters.AddWithValue("@Player", playerName);

						var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

						if( reader.HasRows )
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
			catch(Exception ex)
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

				using( var connection = new SqliteConnection(ConnectionString) )
				{
					connection.Open();

					using( var cmd = connection.CreateCommand() )
					{
						cmd.CommandText = "INSERT OR REPLACE INTO QuestSessions ( WorldId, PlayerName, SessionData ) " +
											"VALUES ( @WorldId, @Player, @Data );";

						cmd.Parameters.AddWithValue("@WorldId", Main.worldID);
						cmd.Parameters.AddWithValue("@Player", playerName);
						cmd.Parameters.AddWithValue("@Data", json);

						cmd.ExecuteNonQuery();
					}
				}
			}
			catch(Exception ex)
			{
				CustomQuestsPlugin.Instance.LogPrint($"Failed to write Session: ({ex.Message})", TraceLevel.Error);
			}
		}
	}
}
