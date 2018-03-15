using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace NpcShops
{
    /// <summary>
    ///     Represents a configuration. This class is a singleton.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config
    {
        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        public static Config Instance { get; internal set; } = new Config();

		/// <summary>
		/// Gets the default Currency type for shops. 
		/// </summary>
		[JsonProperty(Order = 0)]
		public string CurrencyType { get; internal set; } = "None";

		/// <summary>
		///		Gets the maximum distance in tiles, in which a player may talk to a Shopkeeper NPC.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int ShopNpcMaxTalkRange { get; internal set; } = 32;

		/// <summary>
		///		Gets the duration a Shopkeeper NPC will stand still, in millseconds. -1 = indefinite.
		/// </summary>
		[JsonProperty(Order = 2)]
		public int ShopNpcPauseDuration { get; internal set; } = 8000;

		public static void LoadOrCreate(string configPath)
		{
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
				NpcShopsPlugin.Instance.LogPrint($"Error: {ex.Message}", TraceLevel.Error);
				NpcShopsPlugin.Instance.LogPrint($"Error while loading config at '{configPath}'; Using default config.", TraceLevel.Error);
			}

			Instance = new Config();
			Instance.Save(configPath);
		}

		public void Save(string configPath)
		{
			var dir = Path.GetDirectoryName(configPath);

			try
			{
				Directory.CreateDirectory(dir);

				var json = JsonConvert.SerializeObject(this,Formatting.Indented);
				File.WriteAllText(configPath, json);
			}
			catch( Exception ex )
			{
				NpcShopsPlugin.Instance.LogPrint($"Error: {ex.Message}", TraceLevel.Error);
				NpcShopsPlugin.Instance.LogPrint($"Error while saving config at '{configPath}'; Using default config.", TraceLevel.Error);
			}
		}
	}
}
