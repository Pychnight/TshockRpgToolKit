//using Corruption.PluginSupport;
//using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.ComponentModel;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Provides configuration and internal support for a quadrant of a Currency.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class CurrencyQuadrant
	{
		[Category("Basic")]
		[JsonProperty(Order = 0)]
		public int BaseUnitMultiplier { get; set; } = 1;

		[Category("Basic")]
		[JsonProperty(Order = 1)]
		public string FullName { get; set; } = "NewQuadrant";

		//[JsonProperty(Order = 2)]
		//public string ShortName { get; set; }

		[Category("Basic")]
		[JsonProperty(Order = 3)]
		public string Abbreviation { get; set; }

		[JsonProperty(Order = 4)]
		public string CombatText { get; set; }

		[Category("Visual")]
		[DisplayName("GainColor")]
		[JsonProperty(Order = 5, PropertyName = "GainColor")]
		public string GainColorString { get; set; } = "ffffffff";

		//public string GainColorString
		//{
		//	get { return GainColor.PackedValue.ToString("x8"); }
		//	set { GainColor = ColorHelpers.FromHexString(value); }
		//}

		//public Color GainColor { get; set; } = Color.White;

		[Category("Visual")]
		[DisplayName("LossColor")]
		[JsonProperty(Order = 6, PropertyName = "LossColor")]
		public string LossColorString { get; set; } = "ffffffff";
		//public string LossColorString
		//{
		//	get { return LossColor.PackedValue.ToString("x8"); }
		//	set { LossColor = ColorHelpers.FromHexString(value); }
		//}

		//public Color LossColor { get; set; } = Color.Red;

		public CurrencyQuadrant()
		{
		}

		public CurrencyQuadrant(CurrencyQuadrant source)
		{
			BaseUnitMultiplier = source.BaseUnitMultiplier;
			FullName = source.FullName;
			//ShortName = source.ShortName;
			Abbreviation = source.Abbreviation;
			CombatText = source.CombatText;
			GainColorString = source.GainColorString;
			LossColorString = source.LossColorString;
		}

		public override string ToString() => FullName;// ('{Abbreviation}')";
	}
}
