using Leveling.Sessions;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using Terraria;
using TerrariaApi.Server;

namespace Leveling.Database
{
	public class RedisSessionDatabase : ISessionDatabase
	{
		public string ConnectionString { get; set; }
		ConfigurationOptions configOptions;
		ConnectionMultiplexer redis;

		public RedisSessionDatabase(string connectionString)
		{
			ConnectionString = connectionString;
			configOptions = ConfigurationOptions.Parse(connectionString);
			redis = ConnectionMultiplexer.Connect(configOptions);
		}

		private string getKey(int worldId, string userName) => $"/{worldId}/{userName}";

		public SessionDefinition Load(string userName)
		{
			//Debug.Print($"SqliteSessionRepository.Load({userName})");
			SessionDefinition result = null;

			try
			{
				var db = redis.GetDatabase();
				var key = getKey(Main.worldID, userName);
				var json = (string)db.StringGet(key);

				if (db.KeyExists(key))
					result = JsonConvert.DeserializeObject<SessionDefinition>(json);
				else
					result = null;
			}
			catch (Exception ex)
			{
				ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Load error: ({ex.Message})", TraceLevel.Error);
			}

			return result;
		}

		public void Save(string userName, SessionDefinition sessionDefinition)
		{
			//we now try to copy the session definition, in hopes of minimizing "rare" exceptions from definition collections being touched during serialization.
			//we also catch the exception, and just log the error. Hopefully next call to save will work.
			try
			{
				var defCopy = new SessionDefinition(sessionDefinition);
				var json = JsonConvert.SerializeObject(defCopy, Formatting.Indented);
				var key = getKey(Main.worldID, userName);

				var db = redis.GetDatabase();
				db.StringSet(key, json);
			}
			catch (Exception ex)
			{
				ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Error: {ex.Message}", TraceLevel.Error);
				ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Session data not saved.", TraceLevel.Error);
			}
		}
	}
}
