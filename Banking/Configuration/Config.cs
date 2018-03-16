using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corruption.PluginSupport;
using Newtonsoft.Json;

namespace Banking.Configuration
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Config : JsonConfig
	{
		public static Config Instance { get; internal set; }
		
		[JsonProperty(Order = 0)]
		public DatabaseConfig Database { get; private set; } = new DatabaseConfig();

		[JsonProperty(Order = 1)]
		public List<CurrencyDefinition> Currency { get; private set; } = new List<CurrencyDefinition>();
		
		[JsonProperty(Order = 2)]
		public VotingConfig Voting { get; private set; } = new VotingConfig();

		public override void Validate()
		{
			if(Currency==null || Currency.Count<1)
			{
				throw new Exception("Configuration defines no Currency.");
			}
		}
	}
}
