using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Terraria;

namespace Banking.Database
{
	public class RedisDatabase : IDatabase
	{
		public string ConnectionString { get; set; }

		private ConnectionMultiplexer redis;
		private ConfigurationOptions configOptions;

		public RedisDatabase(string connectionString)
		{
			ConnectionString = connectionString;
			configOptions = ConfigurationOptions.Parse(connectionString);
			configOptions.AllowAdmin = true;

			redis = ConnectionMultiplexer.Connect(configOptions);
		}

		private string getKey(int worldId, string playerName, string accountName) => $"/{worldId}/{playerName}/{accountName}";

		private string getKey(BankAccount account) => getKey(Main.worldID, account.OwnerName, account.Name);

		public void Create(BankAccount account)
		{
			var db = redis.GetDatabase();
			var key = getKey(account);
			var val = account.Balance.ToString();

			db.StringSet(key, val);
		}

		public void Create(IEnumerable<BankAccount> accounts)
		{
			foreach (var acc in accounts)
				Create(acc);
		}

		public void Delete(BankAccount account)
		{
			var db = redis.GetDatabase();
			var key = getKey(account);

			db.KeyDelete(key);
		}

		public void Delete(IEnumerable<BankAccount> accounts)
		{
			foreach (var acc in accounts)
				Delete(acc);
		}

		public IEnumerable<BankAccount> Load()
		{
			var result = new List<BankAccount>();
			var hostAndPort = "localhost:6379";
			var endPoint = configOptions.EndPoints.FirstOrDefault();

			if (endPoint is IPEndPoint)
			{
				var ip = endPoint as IPEndPoint;
				hostAndPort = $"{ip.Address}:{ip.Port}";
			}
			else if (endPoint is DnsEndPoint)
			{
				var ip = endPoint as DnsEndPoint;
				hostAndPort = $"{ip.Host}:{ip.Port}";
			}

			var server = redis.GetServer(hostAndPort);
			var db = redis.GetDatabase();
			var keyPattern = $"/{Main.worldID}/*";

			var accounts = new Dictionary<string, BankAccount>();

			foreach (var key in server.Keys(pattern: keyPattern))
			{
				var parts = ((string)key).Split('/');
				//var value = (string)db.StringGet(key);
				var ownerName = parts[2];
				var name = parts[3];
				//var funds = decimal.Parse(value);
				var account = new BankAccount(ownerName, name, 0);

				accounts.Add((string)key, account);
			}

			foreach (var kvp in accounts)
			{
				var value = (string)db.StringGet(kvp.Key);
				var funds = decimal.Parse(value);
				kvp.Value.Balance = funds;
				result.Add(kvp.Value);
			}

			return result;
		}

		public void Save(IEnumerable<BankAccount> accounts) => throw new NotImplementedException();

		public void Update(BankAccount account) => Create(account);

		public void Update(IEnumerable<BankAccount> accounts) => Create(accounts);
	}
}
