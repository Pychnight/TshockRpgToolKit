using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	[JsonObject(MemberSerialization.OptIn)]
	public class CurrencyQuadrantDefinition
	{
		[JsonProperty(Order = 0)]
		public string FullName { get; set; }

		[JsonProperty(Order = 1)]
		public string ShortName { get; set; }

		[JsonProperty(Order = 2)]
		public string Abbreviation { get; set; }

		[JsonProperty(Order = 3)]
		public string CombatText { get; set; }

		public override string ToString()
		{
			return $"{FullName} ('{Abbreviation}')";
		}
	}
}
