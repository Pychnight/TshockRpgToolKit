using Banking.Rewards;
using Corruption.PluginSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	/// <summary>
	/// Provides configuration and internal support for a Currency. 
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class CurrencyDefinition
	{
		[JsonProperty(Order=0)]
		public string InternalName { get; set; }

		[JsonProperty(Order =1)]
		public string ScriptPath { get; set; }
		
		[JsonProperty(Order =2)]
		public List<CurrencyQuadrantDefinition> Quadrants { get; set; } = new List<CurrencyQuadrantDefinition>();

		//[JsonProperty(Order = 3)]
		//public Dictionary<string,CurrencyRewardDefinition> Rewards { get; set; } = new Dictionary<string,CurrencyRewardDefinition>();

		[JsonProperty(Order = 3, ItemConverterType = typeof(StringEnumConverter))]
		public List<RewardReason> GainBy { get; set; } = new List<RewardReason>();
		
		[JsonProperty(Order = 4)]
		public bool SendCombatText { get; set; }

		[JsonProperty(Order = 5)]
		public bool SendStatus { get; set; }

		[JsonProperty(Order = 6)]
		public float Multiplier { get; set; } = 1.0f;

		[JsonProperty(Order = 7)]
		public float DefenseBonusMultiplier { get; set; } = 0.0f;

		[JsonProperty(Order = 8)]
		public float DeathPenaltyMultiplier { get; set; }

		[JsonProperty(Order = 8)]
		public float DeathPenaltyMinimum { get; set; }

		[JsonProperty(Order = 10)]
		public float DeathPenaltyPvPMultiplier { get; set; }

		[JsonProperty(Order = 11)]
		public Dictionary<string, float> WeaponMultipliers { get; set; } = new Dictionary<string, float>();

		[JsonProperty(Order = 12)]
		public bool EnableStatueNpcRewards { get; set; } = false;

		public override string ToString()
		{
			return InternalName;
		}

		public string InfoString { get; private set; }

		internal void UpdateInfoString()
		{
			try
			{

				var sb = new StringBuilder();
				var useSeparator = false;
				var quadrants = Quadrants.ToList();

				quadrants.Reverse();

				foreach( var q in quadrants )
				{
					if( useSeparator )
						sb.Append(" | ");

					sb.AppendFormat("{0}({1})", q.FullName, q.Abbreviation);
					useSeparator = true;
				}

				var quadrantInfo = sb.ToString();

				InfoString = $"{InternalName} - {quadrantInfo}";
			}
			catch
			{
				InfoString = $"{InternalName} - Information not available.";
			}
		}

		private CurrencyConverter currencyConverter;
		public CurrencyConverter GetCurrencyConverter()
		{
			return currencyConverter ?? ( currencyConverter = new CurrencyConverter(this) );
		}

		internal static CurrencyDefinition CreateDefaultCurrency()
		{
			var result = new CurrencyDefinition();

			result.InternalName = "TerrariaCoin";
			result.GainBy.Add(RewardReason.Killing);
			result.SendCombatText = true;

			var q = new CurrencyQuadrantDefinition();
			q.BaseUnitMultiplier = 1;
			q.FullName = "Copper Coin";
			q.ShortName = "Copper";
			q.Abbreviation = "c";

			result.Quadrants.Add(q);

			q = new CurrencyQuadrantDefinition();
			q.BaseUnitMultiplier = 100;
			q.FullName = "Silver Coin";
			q.ShortName = "Silver";
			q.Abbreviation = "s";

			result.Quadrants.Add(q);

			q = new CurrencyQuadrantDefinition();
			q.BaseUnitMultiplier = 10_000;
			q.FullName = "Gold Coin";
			q.ShortName = "Gold";
			q.Abbreviation = "g";

			result.Quadrants.Add(q);

			q = new CurrencyQuadrantDefinition();
			q.BaseUnitMultiplier = 1_000_000;
			q.FullName = "Platinum Coin";
			q.ShortName = "Platinum";
			q.Abbreviation = "p";

			result.Quadrants.Add(q);

			return result;
		}
	}
}
