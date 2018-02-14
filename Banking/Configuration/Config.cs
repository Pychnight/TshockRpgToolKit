using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Banking.Configuration
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Config
	{
		public static Config Instance { get; private set; }
		
		[JsonProperty(Order = 0)]
		public DatabaseConfig Database { get; private set; } = new DatabaseConfig();
		
		public static void LoadOrCreate(string configPath)
		{
			var dir = Path.GetDirectoryName(configPath);

			try
			{
				Directory.CreateDirectory(dir);

				if(File.Exists(configPath))
				{
					var json = File.ReadAllText(configPath);
					Instance = JsonConvert.DeserializeObject<Config>(json);
					return;
				}
			}
			catch(Exception)
			{
				Debug.Print($"Error while loading config at '{configPath}'; Using default config.");
			}

			Instance = new Config();
		}

		public static void Save(string configPath)
		{
			var dir = Path.GetDirectoryName(configPath);

			try
			{
				Directory.CreateDirectory(dir);

				var json = JsonConvert.SerializeObject(Config.Instance,Formatting.Indented);
				File.WriteAllText(configPath,json);
			}
			catch( Exception )
			{
				Debug.Print($"Error while saving config at '{configPath}'.");
			}
		}
	}
}
