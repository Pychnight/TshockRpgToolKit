using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	/// <summary>
	/// Configures and supports how a Currency will reward players.
	/// </summary>
	[JsonObject]
	public class CurrencyRewardDefinition
	{
		//[JsonProperty(Order=0)]
		//public string GainBy { get; set; } = "None";

		[JsonProperty(Order = 1)]
		public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
				
		[JsonProperty(Order=2)]
		public HashSet<string> Ignore { get; set; } = new HashSet<string>();

		//internal use only, keeps us from having to parse the values on each lookup
		internal Dictionary<string, decimal> ParsedValues = new Dictionary<string, decimal>();

		internal void PreParseValues(CurrencyDefinition parent)
		{
			ParsedValues.Clear();

			foreach( var kvp in Values )
			{
				decimal unitValue;
				
				if(parent.GetCurrencyConverter().TryParse(kvp.Value,out unitValue))
				{
					ParsedValues.Add(kvp.Key, unitValue);
				}
			}
		}
	}
}
