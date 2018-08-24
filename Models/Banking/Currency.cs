//using Banking.Currency;
//using Banking.Rewards;
//using Banking.TileTracking;
//using Corruption.PluginSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RpgToolsEditor.Controls;
//using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wexman.Design;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Provides configuration and internal support for a Currency. 
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class CurrencyDefinition : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "NewCurrency.currency";

		[Browsable(false)]
		public string Name
		{
			get => name;
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		[Category("Filesystem")]
		[Description("The file name of this Currency within the folder. Must end with a .currency extension to be discovered.")]
		public string Filename { get => Name; set => Name = value; }

		[Category("Currency")]
		[Description("The name of this Currency as used by the Banking plugin.")]
		[JsonProperty(Order = 0)]
		public string InternalName { get; set; }

		[Category("Currency")]
		[Description("Relative path to a Boo script that can react to events for this Currency.")]
		[JsonProperty(Order = 1)]
		public string ScriptPath { get; set; }

		[Category("Currency")]
		[Description("The denominations that this Currency is composed of. At least one Quadrant must be defined.")]
		[JsonProperty(Order = 2)]
		public List<CurrencyQuadrant> Quadrants { get; set; } = new List<CurrencyQuadrant>();

		[Category("Currency")]
		[Description("List of RewardReasons that determine how this Currency will be rewarded. RewardReasons such as Death, DeathPvP, and Undefined are special cases, and have no effect.")]
		[JsonProperty(Order = 3, ItemConverterType = typeof(StringEnumConverter))]
		public List<RewardReason> GainBy { get; set; } = new List<RewardReason>();

		[Category("Currency")]
		[Description("Determines if the Currency will send a player a notification via combat text for rewards.")]
		[JsonProperty(Order = 4)]
		public bool SendCombatText { get; set; }

		[Category("Currency")]
		[Description("Not implemented.")]
		[JsonProperty(Order = 5)]
		public bool SendStatus { get; set; }

		[Category("Killing")]
		[Description("Determines if killing rewards will be given when killing Npc's spawned from statues.")]
		[JsonProperty(Order = 6)]
		public bool EnableStatueNpcRewards { get; set; } = false;

		[Category("Currency")]
		[Description("A multiplier for all rewards given. Overridable at runtime.")]
		[JsonProperty(Order = 7)]
		public float Multiplier { get; set; } = 1.0f;

		[Category("Killing")]
		[JsonProperty(Order = 8)]
		public float DefenseBonusMultiplier { get; set; } = 0.0f;

		[Category("Death")]
		[Description("Scales the amount of any losses due to a player dying.")]
		[JsonProperty(Order = 9)]
		public float DeathPenaltyMultiplier { get; set; }

		[Category("Death")]
		[Description("The minimium loss allowed per player death.")]
		[JsonProperty(Order = 10)]
		public float DeathPenaltyMinimum { get; set; }

		[Category("Death")]
		[Description("Scales the amount of any losses or gains due to a player killing another player.")]
		[JsonProperty(Order = 11)]
		public float DeathPenaltyPvPMultiplier { get; set; }

		[Category("Playing")]
		[Description("Sets the amount of time a player must be playing to earn a reward from Playing, if enabled.")]
		[JsonProperty(Order = 12)]
		public TimeSpan PlayingDuration { get; set; } = TimeSpan.FromMinutes(15);

		[Category("Mining")]
		[JsonProperty(Order = 13, PropertyName = "DefaultMiningValue")]
		[Description("Sets the default value given for mining tiles, if enabled.")]
		public string DefaultMiningValueString { get; set; } = "";
		public decimal DefaultMiningValue = 1m;

		[Category("Placing")]
		[Description("Sets the default value given for placing tiles, if enabled.")]
		[JsonProperty(Order = 14, PropertyName = "DefaultPlacingValue")]
		public string DefaultPlacingValueString { get; set; } = "";
		public decimal DefaultPlacingValue = 1m;

		[Category("Fishing")]
		[Description("Sets the default value given for fishing items, if enabled.")]
		[JsonProperty(Order = 15, PropertyName = "DefaultFishingValue")]
		public string DefaultFishingValueString { get; set; } = "";
		public decimal DefaultFishingValue = 1m;

		[Category("Playing")]
		[Description("Sets the default value given for playing long enough, if enabled.")]
		[JsonProperty(Order = 16, PropertyName = "DefaultPlayingValue")]
		public string DefaultPlayingValueString { get; set; } = "";
		public decimal DefaultPlayingValue = 1m;

		[Category("Killing")]
		[Description("Scales killing rewards, per weapon, if killing is enabled.")]
		[JsonProperty(Order = 17)]
		[Editor(typeof(GenericDictionaryEditor<string, float>), typeof(UITypeEditor))]
		[GenericDictionaryEditor(Title = "Weapon Multipliers", KeyDisplayName = "Weapon", ValueDisplayName = "Multiplier")]
		public Dictionary<string, float> WeaponMultipliers { get; set; } = new Dictionary<string, float>();

		[Category("Killing")]
		[Description("Reward values per NPC, if killing is enabled.")]
		[JsonProperty(Order = 18)]
		//[Editor(typeof(StringKeyCollectionEditor), typeof(UITypeEditor))]
		[Editor(typeof(KillingCollectionEditor), typeof(UITypeEditor))]
		public ValueOverrideList<string> KillingOverrides { get; set; } = new ValueOverrideList<string>();

		[Category("Mining")]
		[Description("Reward values, per tile or wall, if Mining is enabled. These values will override the default Mining value.")]
		[JsonProperty(Order = 19)]
		//[Editor(typeof(TileKeyCollectionEditor), typeof(UITypeEditor))]
		[Editor(typeof(MiningCollectionEditor), typeof(UITypeEditor))]
		public ValueOverrideList<TileKey> MiningOverrides { get; set; } = new ValueOverrideList<TileKey>();

		[Category("Placing")]
		[Description("Reward values, per tile or wall, if Placing is enabled. These values will override the default Placing value.")]
		[JsonProperty(Order = 20)]
		//[Editor(typeof(TileKeyCollectionEditor), typeof(UITypeEditor))]
		[Editor(typeof(PlacingCollectionEditor), typeof(UITypeEditor))]
		public ValueOverrideList<TileKey> PlacingOverrides { get; set; } = new ValueOverrideList<TileKey>();

		[Category("Fishing")]
		[Description("Reward values, per item, if Fishing is enabled.")]
		[JsonProperty(Order = 21)]
		[Editor(typeof(FishingCollectionEditor), typeof(UITypeEditor))]
		public ValueOverrideList<ItemKey> FishingOverrides { get; set; } = new ValueOverrideList<ItemKey>();

		[Category("Mining")]
		[Description("Reward values, per tshock group name, per tile or wall, if Mining is enabled. This overrides values set in MiningOverrides.")]
		[JsonProperty(Order = 22)]
		[Editor(typeof(GenericDictionaryEditor<string, ValueOverrideList<TileKey>>), typeof(UITypeEditor))]
		[GenericDictionaryEditor(Title = "Group Mining Overrides", ValueEditorType = typeof(MiningCollectionEditor), KeyDisplayName = "Group", ValueDisplayName = "Mining Overrides" )]
		public GroupValueOverrides<TileKey> GroupMiningOverrides { get; set; } = new GroupValueOverrides<TileKey>();

		[Category("Placing")]
		[Description("Reward values, per tshock group name, per tile or wall, if Placing is enabled. This overrides values set in PlacingOverrides.")]
		[JsonProperty(Order = 23)]
		[Editor(typeof(GenericDictionaryEditor<string, ValueOverrideList<TileKey>>), typeof(UITypeEditor))]
		[GenericDictionaryEditor(Title = "Group Placing Overrides", ValueEditorType = typeof(PlacingCollectionEditor), KeyDisplayName = "Group", ValueDisplayName = "Placing Overrides")]
		public GroupValueOverrides<TileKey> GroupPlacingOverrides { get; set; } = new GroupValueOverrides<TileKey>();
			
		//non serialized members.

		//used internally for fast access to currencies -- do not cache or save this.
		//public int Id { get; internal set; }

		/// <summary>
		/// Gets a cached string used for display by /bank list.
		/// </summary>
		internal string DisplayString { get; private set; }
		//internal Dictionary<string, CurrencyQuadrant> NamesToQuadrants { get; private set; }

		//internal void OnInitialize(int id)
		//{
		//	Id = id;
		//	NamesToQuadrants = createNamesToQuadrants();
		//	currencyConverter = new CurrencyConverter(this);

		//	//set defaults 
		//	if( currencyConverter.TryParse(DefaultMiningValueString, out var parsedResult) )
		//		DefaultMiningValue = parsedResult;

		//	if( currencyConverter.TryParse(DefaultPlacingValueString, out parsedResult) )
		//		DefaultPlacingValue = parsedResult;

		//	if( currencyConverter.TryParse(DefaultFishingValueString, out parsedResult) )
		//		DefaultFishingValue = parsedResult;

		//	if( currencyConverter.TryParse(DefaultPlayingValueString, out parsedResult) )
		//		DefaultPlayingValue = parsedResult;

		//	//set overrides
		//	KillingOverrides.Initialize(this);
		//	MiningOverrides.Initialize(this);
		//	PlacingOverrides.Initialize(this);
		//	FishingOverrides.Initialize(this);

		//	//set group overrides ( only tiles for now )
		//	var tileGroupOverrides = new List<GroupValueOverrides<TileKey>>()
		//	{
		//		GroupMiningOverrides,
		//		GroupPlacingOverrides
		//	};

		//	foreach( var go in tileGroupOverrides )
		//	{
		//		foreach( var vol in go.Values )
		//		{
		//			vol.Initialize(this);
		//		}
		//	}

		//	InitializeDisplayString();
		//}

		//private Dictionary<string, CurrencyQuadrant> createNamesToQuadrants()
		//{
		//	var mapping = new Dictionary<string, CurrencyQuadrant>();

		//	foreach( var quad in Quadrants )
		//	{
		//		var names = new List<string>() { quad.FullName, quad.Abbreviation };

		//		foreach( var name in names )
		//		{
		//			if( !string.IsNullOrWhiteSpace(name) )
		//			{
		//				if( mapping.ContainsKey(name) )
		//				{
		//					BankingPlugin.Instance.LogPrint($"Currency {this.InternalName} already contains " +
		//													$"a quadrant using the name or abbreviation '{name}'. ",
		//													TraceLevel.Warning);
		//				}

		//				mapping[name] = quad;
		//			}
		//		}
		//	}

		//	return mapping;
		//}

		//internal void InitializeDisplayString()
		//{
		//	try
		//	{
		//		var sb = new StringBuilder();
		//		var useSeparator = false;
		//		var quadrants = Quadrants.ToList();

		//		quadrants.Reverse();

		//		foreach( var q in quadrants )
		//		{
		//			if( useSeparator )
		//				sb.Append(" | ");

		//			sb.AppendFormat("{0}({1})", q.FullName, q.Abbreviation);
		//			useSeparator = true;
		//		}

		//		var quadrantInfo = sb.ToString();

		//		DisplayString = $"{InternalName} - {quadrantInfo}";
		//	}
		//	catch
		//	{
		//		DisplayString = $"{InternalName} - Information not available.";
		//	}
		//}

		//private CurrencyConverter currencyConverter;
		//public CurrencyConverter GetCurrencyConverter()
		//{
		//	return currencyConverter;
		//}

		//private void TestArea()
		//{
		//	Wexman.Design.GenericDictionaryEditorAttribute attr = null;

		//	attr.Title

		//}

		public override string ToString()
		{
			return InternalName;
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}
	}
}
