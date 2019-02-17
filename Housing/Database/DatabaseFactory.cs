using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Housing.Database
{
	public static class DatabaseFactory
	{
		public static IDatabase LoadOrCreateDatabase(Config config)
		{
			IDatabase result = null;
			//Debug.Print($"connectionstring: {config.Database.ConnectionString}");
			
			switch(config.Database.DatabaseType)
			{
				case "mysql":
					var connectionString = config.Database.ConnectionString;
					result = new MySqlDatabase(connectionString);
					break;

				case "sqlite":
				default:
					var connection = new SqliteConnection(config.Database.ConnectionString);
					result = new SqliteDatabase(connection);
					break;
			}
			
			return result;
		}
	}
}
