using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Data.Sqlite;
using TerrariaApi.Server;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Data;

namespace CustomSkills.Database
{
    public class SqliteSessionDatabase : ISessionDatabase
    {
        public string ConnectionString { get; private set; }

        public SqliteSessionDatabase(string connectionString)
        {
			const string CreateTableSql = "CREATE TABLE IF NOT EXISTS skill_sessions (" +
											"player text PRIMARY KEY NOT NULL," +
											"data text );";

			try
            {
                ConnectionString = connectionString;

                using(var connection = new SqliteConnection(ConnectionString))
                {
                    using(var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = CreateTableSql;

                        connection.Open();
                        var results = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                ServerApi.LogWriter.PluginWriteLine(CustomSkillsPlugin.Instance, "Failed to open skill_sessions database!", TraceLevel.Error);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
			
        public Session Load(string userName)
        {
            Session result = null;

            using(var connection = new SqliteConnection(ConnectionString))
            {
                using(var cmd = connection.CreateCommand())
                {
                   	cmd.CommandText = "SELECT data FROM skill_sessions " +
										$"WHERE player=@player;";

					cmd.Parameters.AddWithValue("@player", userName);

					connection.Open();
                    var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                    if(reader.HasRows)
                    {
                        try
                        {
                            reader.Read();
                            var json = reader.GetString(0);
                            result = JsonConvert.DeserializeObject<Session>(json);
                        }
                        catch(Exception ex)
                        {
                            ServerApi.LogWriter.PluginWriteLine(CustomSkillsPlugin.Instance, $"Load error: ({ex.Message})", TraceLevel.Error);
                        }
                    }
                }
            }

            return result;
        }

        public void Save(string userName, Session session)
        {
            //Debug.Print($"SqliteSessionRepository.Save({userName})");
            using(var connection = new SqliteConnection(ConnectionString))
            {
				try
				{
					var json = JsonConvert.SerializeObject(session);//, Formatting.Indented);

                    using(var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "INSERT OR REPLACE INTO skill_sessions ( player, data ) " +
                                            "VALUES ( @player, @data );";

                        cmd.Parameters.AddWithValue("@player", userName);
                        cmd.Parameters.AddWithValue("@data", json);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch(Exception ex)
                {
                    ServerApi.LogWriter.PluginWriteLine(CustomSkillsPlugin.Instance, $"Error: {ex.Message}", TraceLevel.Error);
                    ServerApi.LogWriter.PluginWriteLine(CustomSkillsPlugin.Instance, $"Session data not saved.", TraceLevel.Error);
                }
            }
        }
    }
}
