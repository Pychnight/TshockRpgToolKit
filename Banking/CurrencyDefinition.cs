using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	[JsonObject(MemberSerialization.OptIn)]
	public class CurrencyDefinition
	{
		[JsonProperty(Order=0)]
		public string InternalName { get; set; }

		[JsonProperty(Order =1)]
		public string ScriptPath { get; set; }
		
		[JsonProperty(Order =2)]
		public List<CurrencyQuadrantDefinition> Quadrants { get; set; } = new List<CurrencyQuadrantDefinition>();
		
		[JsonProperty(Order = 3)]
		public string GainBy { get; set; }

		[JsonProperty(Order = 4)]
		public bool SendCombatTXT { get; set; }

		[JsonProperty(Order = 5)]
		public bool SendStatus { get; set; }

		[JsonProperty(Order = 6)]
		public float Multiplier { get; set; } = 1.0f;

		[JsonProperty(Order = 7)]
		public float DeathPenaltyMultiplier { get; set; }

		[JsonProperty(Order = 8)]
		public float DeathPenaltyMinimum { get; set; }

		[JsonProperty(Order = 9)]
		public float DeathPenaltyPvPMultiplier { get; set; }

		public override string ToString()
		{
			return InternalName;
		}
	}
}
