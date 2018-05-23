using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Database
{
	public class DatabaseFactory
	{
		public static IDatabase LoadOrCreateDatabase(string databaseType, string connectionString)
		{
			IDatabase db = null;

			switch( databaseType )
			{
				//case "redis":
				//	db = new RedisDatabase(connectionString);
				//	break;

				//case "mysql":
				//	db = new MySqlDatabase(connectionString);
				//	break;

				//case "sqlite":
				//default:
				//	db = new SqliteDatabase(connectionString);
				//	break;
			}

			return db;
		}
	}
}
