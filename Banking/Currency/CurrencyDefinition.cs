﻿using Banking.Rewards;
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

		private CurrencyConverter currencyConverter;
		public CurrencyConverter GetCurrencyConverter()
		{
			return currencyConverter ?? ( currencyConverter = new CurrencyConverter(this) );
		}
	}
}
