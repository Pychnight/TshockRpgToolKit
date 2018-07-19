using Banking.Rewards;
using Corruption.PluginSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
		const string DefaultCurrencyName = "TerrariaCoin";

		[JsonProperty(Order=0)]
		public string InternalName { get; set; }

		[JsonProperty(Order =1)]
		public string ScriptPath { get; set; }
		
		[JsonProperty(Order =2)]
		public List<CurrencyQuadrant> Quadrants { get; set; } = new List<CurrencyQuadrant>();

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

		//internal usage and non serialized members.

		//used internally for fast access to currencies -- do not cache or save this.
		public int Id { get; internal set; }

		internal string InfoString { get; private set; }
				
		internal Dictionary<string, CurrencyQuadrant> NamesToQuadrants { get; private set; }
		
		internal void OnInitialize(int id)
		{
			Id = id;
			NamesToQuadrants = createNamesToQuadrants();
			currencyConverter = new CurrencyConverter(this);
		}

		private Dictionary<string, CurrencyQuadrant> createNamesToQuadrants()
		{
			var mapping = new Dictionary<string, CurrencyQuadrant>();
			
			foreach(var quad in Quadrants)
			{
				var names = new List<string>() { quad.FullName, quad.Abbreviation };
								
				foreach(var name in names)
				{
					if( !string.IsNullOrWhiteSpace(name) )
					{
						if( mapping.ContainsKey(name) )
						{
							BankingPlugin.Instance.LogPrint($"Currency {this.InternalName} already contains " +
															$"a quadrant using the name or abbreviation '{name}'. ",
															TraceLevel.Warning);
						}

						mapping[name] = quad;
					}
				}
			}
			
			return mapping;
		}

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
			return currencyConverter;
		}

		public override string ToString()
		{
			return InternalName;
		}

		public static IList<CurrencyDefinition> LoadCurrencys(string currencyDirectoryPath)
		{
			var currencyFiles = Directory.EnumerateFiles(currencyDirectoryPath, "*.currency");
			var results = new List<CurrencyDefinition>();

			results.Add(CreateDefaultCurrency());

			foreach(var file in currencyFiles)
			{
				try
				{
					var json = File.ReadAllText(file);
					var currency = JsonConvert.DeserializeObject<CurrencyDefinition>(json);

					//never overwrite the default currency!
					if( currency.InternalName == DefaultCurrencyName )
					{
						BankingPlugin.Instance.LogPrint($"{file} attempts to override the default currency({DefaultCurrencyName}), but this is not allowed. Ignoring file.", TraceLevel.Warning);
						continue;
					}

					results.Add(currency);
				}
				catch(Exception ex)
				{
					BankingPlugin.Instance.LogPrint(ex.Message, TraceLevel.Error);
				}
			}

			return results;
		}

		public static void SaveCurrencys(string currencyDirectoryPath, IEnumerable<CurrencyDefinition> currencys)
		{
			Directory.CreateDirectory(currencyDirectoryPath);

			foreach(var currency in currencys)
			{
				try
				{
					var filePath = Path.Combine(currencyDirectoryPath, currency.InternalName,".currency");
					var json = JsonConvert.SerializeObject(currency);
					File.WriteAllText(filePath, json);
				}
				catch(Exception ex)
				{
					BankingPlugin.Instance.LogPrint(ex.Message, TraceLevel.Error);
				}
			}
		}
		
		/// <summary>
		/// This is a standard Currency that matches the Terraria Platinum/Gold/Silver/Copper currency. The n
		/// </summary>
		/// <returns></returns>
		internal static CurrencyDefinition CreateDefaultCurrency()
		{
			var result = new CurrencyDefinition();

			result.InternalName = DefaultCurrencyName;
			//result.GainBy.Add(RewardReason.Killing);
			//result.SendCombatText = true;

			var q = new CurrencyQuadrant();
			q.BaseUnitMultiplier = 1;
			q.FullName = "Copper";
			q.ShortName = "copper";
			q.Abbreviation = "c";

			result.Quadrants.Add(q);

			q = new CurrencyQuadrant();
			q.BaseUnitMultiplier = 100;
			q.FullName = "Silver";
			q.ShortName = "silver";
			q.Abbreviation = "s";

			result.Quadrants.Add(q);

			q = new CurrencyQuadrant();
			q.BaseUnitMultiplier = 10_000;
			q.FullName = "Gold";
			q.ShortName = "gold";
			q.Abbreviation = "g";

			result.Quadrants.Add(q);

			q = new CurrencyQuadrant();
			q.BaseUnitMultiplier = 1_000_000;
			q.FullName = "Platinum";
			q.ShortName = "platinum";
			q.Abbreviation = "p";

			result.Quadrants.Add(q);

			return result;
		}
	}
}
