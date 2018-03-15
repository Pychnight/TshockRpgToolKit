using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using TerrariaApi.Server;

namespace Leveling
{
    /// <summary>
    ///     Represents the configuration. This class is a singleton.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config
    {
		/// <summary>
		///     Gets the configuration instance.
		/// </summary>
		public static Config Instance { get; internal set; } = new Config();

		[JsonProperty(Order = 0)]
		public DatabaseConfig DatabaseConfig { get; private set; } = new DatabaseConfig();

		/// <summary>
		///     Gets the default class name.
		/// </summary>
		[JsonProperty("DefaultClass", Order = 1)]
		public string DefaultClassName { get; private set; } = "ranger";
		
		/// <summary>
		///     Gets the mapping of NPC names to EXP rewards.
		/// </summary>
		[JsonProperty("NpcToExpReward", Order = 2)]
        public IDictionary<string, long> NpcNameToExpReward = new Dictionary<string, long>();
		
        /// <summary>
        ///     Gets the global death penalty minimum.
        /// </summary>
        [JsonProperty("DeathPenaltyMinimum", Order = 5)]
        public long DeathPenaltyMinimum { get; private set; }

        /// <summary>
        ///     Gets or sets the global death penalty multiplier.
        /// </summary>
        [JsonProperty("DeathPenaltyMultiplier", Order = 4)]
        public double DeathPenaltyMultiplier { get; set; } = 0.33;

        /// <summary>
        ///     Gets or sets the global death penalty multiplier from PvP.
        /// </summary>
        [JsonProperty("DeathPenaltyPvPMultiplier", Order = 6)]
        public double DeathPenaltyPvPMultiplier { get; set; } = 0.10;

        
        /// <summary>
        ///     Gets or sets the global EXP multiplier.
        /// </summary>
        [JsonProperty("ExpMultiplier", Order = 3)]
        public double ExpMultiplier { get; set; } = 1.0;

		//internal void Save(string configPath)
		//{
		//	var dir = Path.GetDirectoryName(configPath);

		//	try
		//	{
		//		Directory.CreateDirectory(dir);

		//		var json = JsonConvert.SerializeObject(this, Formatting.Indented);
		//		File.WriteAllText(configPath, json);
		//	}
		//	catch( Exception ex )
		//	{
		//		LevelingPlugin.Instance.LogPrint($"Error: {ex.Message}", TraceLevel.Error);
		//		LevelingPlugin.Instance.LogPrint($"Error while saving config at '{configPath}'; Using default config.", TraceLevel.Error);
		//	}
		//}

		public static void LoadOrCreate(string configPath)
		{
			var plugin = LevelingPlugin.Instance;
			var dir = Path.GetDirectoryName(configPath);

			try
			{
				Directory.CreateDirectory(dir);

				if( File.Exists(configPath) )
				{
					var json = File.ReadAllText(configPath);
					Instance = JsonConvert.DeserializeObject<Config>(json);
					return;
				}
			}
			catch( Exception ex )
			{
				ServerApi.LogWriter.PluginWriteLine(plugin,$"Error: {ex.Message}", TraceLevel.Error);
				ServerApi.LogWriter.PluginWriteLine(plugin,$"Error while loading config at '{configPath}'; Using default config.", TraceLevel.Error);
			}

			Instance = new Config();
			Instance.Save(configPath);
		}

		public void Save(string configPath)
		{
			var plugin = LevelingPlugin.Instance;
			var dir = Path.GetDirectoryName(configPath);

			try
			{
				Directory.CreateDirectory(dir);

				var json = JsonConvert.SerializeObject(this, Formatting.Indented);
				File.WriteAllText(configPath, json);
			}
			catch( Exception ex )
			{
				ServerApi.LogWriter.PluginWriteLine(plugin,$"Error: {ex.Message}", TraceLevel.Error);
				ServerApi.LogWriter.PluginWriteLine(plugin,$"Error while saving config at '{configPath}'; Using default config.", TraceLevel.Error);
			}
		}
	}
}
