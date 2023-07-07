using Newtonsoft.Json;
using System.Collections.Generic;

namespace Banking.Configuration
{
	[JsonObject(MemberSerialization.OptIn)]
	public class VotingConfig
	{
		[JsonProperty(Order = 0)]
		public bool Enabled { get; set; } = true;

		[JsonProperty(Order = 1)]
		public string BaseAddress { get; set; } = "https://terraria-servers.com/api/";

		[JsonProperty(Order = 2)]
		public string ApiKey { get; set; } = "<your-api-key>";

		[JsonProperty(Order = 3)]
		public string RewardMessage { get; private set; } = "Thank you for voting!";

		[JsonProperty(Order = 4)]
		public string NoRewardMessage { get; private set; } = "No vote was found.";

		[JsonProperty(Order = 5)]
		public Dictionary<string, string> Rewards { get; private set; } = new Dictionary<string, string>();
	}
}
