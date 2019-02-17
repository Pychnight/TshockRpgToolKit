using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.PluginSupport
{
	/// <summary>
	/// Common configuration for plugin databases.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class DatabaseConfig
	{
		public const string DefaultDatabaseType = "sqlite";
		public const string DefaultConnectionString = "uri=file://db.sqlite,Version=3";

		[JsonProperty(Order = 0)]
		public string DatabaseType { get; set; }

		[JsonProperty(Order = 1)]
		public string ConnectionString { get; set; }

		public DatabaseConfig()
		{
		}

		public DatabaseConfig(string databaseType = DefaultDatabaseType, string connectionString = DefaultConnectionString)
		{
			DatabaseType = databaseType;
			ConnectionString = connectionString;
		}
	}
}