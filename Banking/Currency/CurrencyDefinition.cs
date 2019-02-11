using Banking.Currency;
using Banking.Rewards;
using Banking.TileTracking;
using Corruption.PluginSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OTAPI.Tile;
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
	public class CurrencyDefinition : IValidator
	{
		[JsonProperty(Order = 0)]
		public string InternalName { get; set; }

		[JsonProperty(Order = 1)]
		public string ScriptPath { get; set; }

		[JsonProperty(Order = 2)]
		public List<CurrencyQuadrant> Quadrants { get; set; } = new List<CurrencyQuadrant>();

		[JsonProperty(Order = 3, ItemConverterType = typeof(StringEnumConverter))]
		public List<RewardReason> GainBy { get; set; } = new List<RewardReason>();

		[JsonProperty(Order = 4)]
		public bool SendCombatText { get; set; }

		[JsonProperty(Order = 5)]
		public bool SendStatus { get; set; }

		[JsonProperty(Order = 6)]
		public bool EnableStatueNpcRewards { get; set; } = false;
		
		[JsonProperty(Order = 7)]
		public float Multiplier { get; set; } = 1.0f;

		[JsonProperty(Order = 8)]
		public float DefenseBonusMultiplier { get; set; } = 0.0f;

		[JsonProperty(Order = 9)]
		public float DeathPenaltyMultiplier { get; set; }

		[JsonProperty(Order = 10)]
		public float DeathPenaltyMinimum { get; set; }

		[JsonProperty(Order = 11)]
		public float DeathPenaltyPvPMultiplier { get; set; }

		[JsonProperty(Order = 12)]
		public TimeSpan PlayingDuration { get; set; } = TimeSpan.FromMinutes(15);

		[JsonProperty(Order = 13, PropertyName = "DefaultMiningValue")]
		public string DefaultMiningValueString { get; set; } = "";
		public decimal DefaultMiningValue = 1m;

		[JsonProperty(Order = 14, PropertyName = "DefaultPlacingValue")]
		public string DefaultPlacingValueString { get; set; } = "";
		public decimal DefaultPlacingValue = 1m;

		[JsonProperty(Order = 15, PropertyName = "DefaultFishingValue")]
		public string DefaultFishingValueString { get; set; } = "";
		public decimal DefaultFishingValue = 1m;
				
		[JsonProperty(Order = 16, PropertyName = "DefaultPlayingValue")]
		public string DefaultPlayingValueString { get; set; } = "";
		public decimal DefaultPlayingValue = 1m;

		[JsonProperty(Order = 17)]
		public Dictionary<string, float> WeaponMultipliers { get; set; } = new Dictionary<string, float>();
		
		[JsonProperty(Order = 18)]
		public ValueOverrideList<string> KillingOverrides { get; set; } = new ValueOverrideList<string>();

		[JsonProperty(Order = 19)]
		public ValueOverrideList<TileKey> MiningOverrides { get; set; } = new ValueOverrideList<TileKey>();

		[JsonProperty(Order = 20)]
		public ValueOverrideList<TileKey> PlacingOverrides { get; set; } = new ValueOverrideList<TileKey>();

		[JsonProperty(Order = 21)]
		public ValueOverrideList<ItemKey> FishingOverrides { get; set; } = new ValueOverrideList<ItemKey>();

		[JsonProperty(Order = 22)]
		public GroupValueOverrides<TileKey> GroupMiningOverrides { get; set; } = new GroupValueOverrides<TileKey>();

		[JsonProperty(Order = 23)]
		public GroupValueOverrides<TileKey> GroupPlacingOverrides { get; set; } = new GroupValueOverrides<TileKey>();

		[JsonProperty(Order = 24)]
		public GroupValueOverrides<string> GroupPlayingOverrides { get; set; } = new GroupValueOverrides<string>();

		/// <summary>
		/// Gets or sets the permission a player must have to trade this currency.
		/// </summary>
		[JsonProperty(Order = 25)]
		public string TradePermission { get; set; }

		//non serialized members.

		//We want to reuse the GroupValueOverrides type, but GroupPlayingOverrides property doesn't need a specialized key.
		//In order to reuse the existing code, we give GroupPlayingOverrides a dummy key placeholder and call it a day.  
		internal const string DummyKeyString = "key";

		//used internally for fast access to currencies -- do not cache or save this.
		public int Id { get; internal set; }

		/// <summary>
		/// Gets a cached string used for display by /bank list.
		/// </summary>
		internal string DisplayString { get; private set; }

		/// <summary>
		/// Fast access to quadrants, by name.
		/// </summary>
		internal Dictionary<string, CurrencyQuadrant> NamesToQuadrants { get; private set; }

		/// <summary>
		/// Fired before a Reward is sent to the player's BankAccount for this currency.
		/// </summary>
		public event EventHandler<RewardEventArgs> PreReward;

		internal void FirePreRewardEvents(Reward reward, ref decimal rewardValue, string playerName)
		{
			if( PreReward == null )
				return;

			var args = new RewardEventArgs(reward,ref rewardValue,this, playerName);

			//Debug.Print($"RewardIn: {rewardValue}");
			
			PreReward(this, args);

			rewardValue = args.RewardValue;

			//Debug.Print($"RewardOut: {rewardValue}");
		}
				
		/// <summary>
		/// Performs necessary setup and preprocessing to use a Currency in-game.  
		/// </summary>
		/// <param name="id"></param>
		internal void OnInitialize(int id)
		{
			Id = id;
			NamesToQuadrants = createNamesToQuadrants();
			currencyConverter = new CurrencyConverter(this);

			//set defaults 
			if(currencyConverter.TryParse(DefaultMiningValueString, out var parsedResult))
				DefaultMiningValue = parsedResult;

			if( currencyConverter.TryParse(DefaultPlacingValueString, out parsedResult) )
				DefaultPlacingValue = parsedResult;

			if( currencyConverter.TryParse(DefaultFishingValueString, out parsedResult) )
				DefaultFishingValue = parsedResult;

			if( currencyConverter.TryParse(DefaultPlayingValueString, out parsedResult) )
				DefaultPlayingValue = parsedResult;

			//set overrides
			KillingOverrides.Initialize(this);
			MiningOverrides.Initialize(this);
			PlacingOverrides.Initialize(this);
			FishingOverrides.Initialize(this);

			//set group overrides for tiles
			var tileGroupOverrides = new List<GroupValueOverrides<TileKey>>()
			{
				GroupMiningOverrides,
				GroupPlacingOverrides
			};

			foreach(var go in tileGroupOverrides)
			{
				foreach(var vol in go.Values)
				{
					vol.Initialize(this);
				}
			}

			//set group overrides for playing
			foreach(var vol in GroupPlayingOverrides.Values)
				vol.Initialize(this);
						
			InitializeDisplayString();
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
		
		internal void InitializeDisplayString()
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

				DisplayString = $"{InternalName} - {quadrantInfo}";
			}
			catch
			{
				DisplayString = $"{InternalName} - Information not available.";
			}
		}

		private CurrencyConverter currencyConverter;
		public CurrencyConverter GetCurrencyConverter()
		{
			return currencyConverter;
		}

		/// <summary>
		/// Gets an override value for killing an npc, should an override exist.
		/// </summary>
		/// <remarks>This is related to the GetBase*Value() methods, but works a bit differently in that it tries to find an override value only.
		/// If one cannot be found, null is returned.</remarks>
		/// <param name="npcGivenOrTypeName">Npc given or type name string.</param>
		/// <returns>Decimal value if override exists, null otherwise.</returns>
		public decimal? GetKillingValueOverride(string npcGivenOrTypeName)
		{
			if(KillingOverrides.TryGetValue(npcGivenOrTypeName,out var valueOverride))
				return valueOverride.Value;
			else
				return null;
		}

		/// <summary>
		/// Computes the base value for mining a tile, that is, before multipliers or modifications.
		/// </summary>
		/// <param name="tileOrWallType">The tile type or wall.</param>
		/// <param name="miningTargetType">Type of tile data, either tile or wall.</param>
		/// <returns>Base value in generic units.</returns>
		public decimal GetBaseMiningValue(ushort tileOrWallType, TileSubTarget miningTargetType, string group)
		{
			var key = new TileKey(tileOrWallType, miningTargetType);

			//check group overrides first
			if(!string.IsNullOrWhiteSpace(group))
			{
				if(GroupMiningOverrides.TryGetValue(group,out var groupMiningOverride))
				{
					if(groupMiningOverride.TryGetValue(key, out var groupValueOverride))
					{
						return groupValueOverride.Value;
					}
				}
			}

			//no group override found, move to global overrides
			if( MiningOverrides.TryGetValue(key, out var valueOverride) )
				return valueOverride.Value;
			else
				return DefaultMiningValue;
		}

		/// <summary>
		/// Computes the base value for placing a tile, that is, before multipliers or modifications.
		/// </summary>
		/// <param name="tileOrWallType">The tile type or wall.</param>
		/// <param name="miningTargetType">Type of tile data, either tile or wall.</param>
		/// <returns>Base value in generic units.</returns>
		public decimal GetBasePlacingValue(ushort tileOrWallType, TileSubTarget miningTargetType, string group)
		{
			var key = new TileKey(tileOrWallType, miningTargetType);

			//check group overrides first
			if( !string.IsNullOrWhiteSpace(group) )
			{
				if( GroupPlacingOverrides.TryGetValue(group, out var groupPlacingOverride) )
				{
					if( groupPlacingOverride.TryGetValue(key, out var groupValueOverride) )
					{
						return groupValueOverride.Value;
					}
				}
			}

			if( PlacingOverrides.TryGetValue(key, out var valueOverride) )
				return valueOverride.Value;
			else
				return DefaultPlacingValue;
		}

		/// <summary>
		/// Computes the base value for catching items through fishing, that is, before multipliers or modifications.
		/// </summary>
		/// <param name="itemId">ItemId</param>
		/// <param name="prefix">Prefix</param>
		/// <returns>Base value in generic units.</returns>
		public decimal GetBaseFishingValue(int itemId, byte prefix)
		{
			var key = new ItemKey(itemId, prefix);

			if( FishingOverrides.TryGetValue(key, out var valueOverride) )
				return valueOverride.Value;
			else
				return DefaultFishingValue;
		}

		/// <summary>
		/// Computes the base value for placing a certain amount of time, before multipliers or modifications.
		/// </summary>
		/// <param name="group">Optional Group name.</param>
		/// <returns>Base value in generic units.</returns>
		public decimal GetBasePlayingValue(string group="")
		{
			//check group overrides first
			if( !string.IsNullOrWhiteSpace(group) )
			{
				if( GroupPlayingOverrides.TryGetValue(group, out var groupPlacingOverride) )
				{
					if( groupPlacingOverride.TryGetValue(DummyKeyString, out var groupValueOverride) )
					{
						return groupValueOverride.Value;
					}
				}
			}
			
			return DefaultPlayingValue;
		}

		public override string ToString() => InternalName;
		
		public static IList<CurrencyDefinition> LoadCurrencys(string currencyDirectoryPath)
		{
			//Debug.Print($"Loading CurrencyDefinitions at: {currencyDirectoryPath}");
			var currencyFiles = Directory.EnumerateFiles(currencyDirectoryPath, "*.currency");
			var results = new List<CurrencyDefinition>();
			
#if DEBUG
			if( currencyFiles.Count()<1)
			{
				BankingPlugin.Instance.LogPrint("No .currency files found, creating default 'TerrariaCoin.currency'.", TraceLevel.Warning);
				results.Add(CreateDefaultCurrency());

				SaveCurrencys(currencyDirectoryPath,results);
				return results;
			}
#endif
			
			foreach(var file in currencyFiles)
			{
				try
				{
					var json = File.ReadAllText(file);
					var currency = JsonConvert.DeserializeObject<CurrencyDefinition>(json);
					var validationResult = currency.Validate();
					int totalErrors = 0, totalWarnings = 0;
										
					validationResult.GetTotals(ref totalErrors, ref totalWarnings);

					BankingPlugin.Instance.LogPrint(validationResult);
									   
					if (totalErrors == 0)
						results.Add(currency);
					else
						BankingPlugin.Instance.LogPrint($"{file}: Disabling Currency due to errors.",TraceLevel.Error);
				}
				catch(Exception ex)
				{
					BankingPlugin.Instance.LogPrint($"{file}: {ex.Message}", TraceLevel.Error);
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
					var filePath = Path.Combine(currencyDirectoryPath, currency.InternalName + ".currency");
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
			const string DefaultCurrencyName = "TerrariaCoin";

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

		public ValidationResult Validate()
		{
			var result = new ValidationResult(this);

			if (string.IsNullOrWhiteSpace(InternalName))
				result.Errors.Add(new ValidationError($"{nameof(InternalName)} is null or whitespace."));

			if (Quadrants==null || Quadrants.Count<1)
				result.Errors.Add(new ValidationError($"{nameof(Quadrants)} is null or empty."));
			else
			{
				var i = 0;

				foreach(var quad in Quadrants)
				{
					var quadResult = quad.Validate();
					quadResult.Source = $"Quadrant[{i++}]";
					result.Children.Add(quadResult);
				}
			}

			if (GainBy == null || GainBy.Count < 1)
				result.Warnings.Add(new ValidationWarning($"{nameof(GainBy)} is null or empty."));

			return result;
		}
	}
}
