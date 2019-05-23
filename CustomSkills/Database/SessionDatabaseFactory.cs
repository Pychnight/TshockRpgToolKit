using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomSkills.Database
{
    public class SessionDatabaseFactory
    {
        public static ISessionDatabase LoadOrCreateDatabase(string databaseType, string connectionString)
        {
            ISessionDatabase db = null;

            switch(databaseType)
            {
				//case "redis":
				//    db = new RedisSessionDatabase(connectionString);
				//    break;

				case "mysql":
					db = new MySqlSessionDatabase(connectionString);
					break;

				case "sqlite":
                default:
                    db = new SqliteSessionDatabase(connectionString);
                    break;
            }

            return db;
        }
    }
}
