using Banking.Configuration;
using System;
using System.Collections.Generic;
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
			httpClient.BaseAddress = new Uri("https://terraria-servers.com/api/");
			httpClient.Timeout = TimeSpan.FromSeconds(10);
		}

		public async Task<bool> HasPlayerVotedAsync(string playerName)
		{
			var requestUri = $"?object=votes&element=claim&key={Config.Instance.Voting.ApiKey}&username={playerName}";
			var response = await httpClient.GetStringAsync(requestUri);

			switch(response)
			{
				case "0"://not found
					return false;
				case "1"://has voted, not claimed
					return true;
				case "2"://has voted, claimed
					return false;
				default:
					return false;
			}
		}

	}
}
