using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	/// <summary>
	/// Provides configuration and internal support for a quadrant of a Currency.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class CurrencyQuadrantDefinition
	{
		[JsonProperty(Order = 0)]
		public int BaseUnitMultiplier { get; set; } = 1;

		[JsonProperty(Order = 1)]
		public string FullName { get; set; }

		[JsonProperty(Order = 2)]
		public string ShortName { get; set; }

		[JsonProperty(Order = 3)]
		public string Abbreviation { get; set; }

		[JsonProperty(Order = 4)]
		public string CombatText { get; set; }

		[JsonProperty(Order = 5, PropertyName = "CombatTextColor")]
		public string CombatTextColorString
		{
			get { return CombatTextColor.PackedValue.ToString("x8"); }
			set
			{
				try
				{
					var packed = Convert.ToUInt32(value, 16);
					CombatTextColor = new Color(packed);
				}
				catch
				{
					CombatTextColor = Color.White;
				}
			}
		}

		public Color CombatTextColor { get; set; } = Color.White;
		
		public override string ToString()
		{
			return $"{FullName} ('{Abbreviation}')";
		}
	}
}
