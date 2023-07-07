namespace CustomQuests.Database
{
	public class DatabaseFactory
	{
		public static IDatabase LoadOrCreateDatabase(string databaseType, string connectionString)
		{
			IDatabase db = null;

			switch (databaseType)
			{
				//case "redis":
				//	db = new RedisDatabase(connectionString);
				//	break;

				case "mysql":
					//connectionString = "hijacked";
					db = new MySqlJsonDatabase(connectionString);
					break;

				case "sqlite":
				default:
					db = new SqliteJsonDatabase(connectionString);
					break;
			}

			return db;
		}
	}
}
