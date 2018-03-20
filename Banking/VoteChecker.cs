using Banking.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	public class VoteChecker
	{
		HttpClient httpClient;

		public VoteChecker()
		{
			httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(Config.Instance.Voting.BaseAddress);//new Uri("https://terraria-servers.com/api/");
			httpClient.Timeout = TimeSpan.FromSeconds(10);
		}

		public async Task<VoteStatus> HasPlayerVotedAsync(string playerName)
		{
			var statusUri = $"?object=votes&element=claim&key={Config.Instance.Voting.ApiKey}&username={playerName}";
			var response = await httpClient.GetStringAsync(statusUri);

			switch(response)
			{
				case "1"://has voted, not claimed
					return VoteStatus.Unclaimed;

				case "2"://has voted, claimed
					return VoteStatus.Claimed;

				case "0"://not found
				default:
					return VoteStatus.NotFound;
			}
		}

		public async Task ClaimPlayerVoteAsync(string playerName)
		{
			//if( claimVote )
			{
				Debug.WriteLine($"Trying to claim vote for {playerName}...");
				var claimUri = $"?action=post&object=votes&element=claim&key={Config.Instance.Voting.ApiKey}&username={playerName}";

				var content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("action", "post"),
					new KeyValuePair<string, string>("object", "votes"),
					new KeyValuePair<string, string>("element","claim"),
					new KeyValuePair<string, string>("key",Config.Instance.Voting.ApiKey),
					new KeyValuePair<string, string>("username",playerName)
				});

				Debug.WriteLine($"Content:{content}");

				var postResponse = await httpClient.PostAsync(claimUri, content);

				//response = await httpClient.GetStringAsync(claimUri);
				Debug.WriteLine($"ClaimVote response: {postResponse.Content}");
			}
		}
	}

	public enum VoteStatus
	{
		NotFound,
		Claimed,
		Unclaimed
	}
}
