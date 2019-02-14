using Corruption.PluginSupport;
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
	public class CurrencyQuadrant : IValidator
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

		//formerly "CombatTextColor"
		[JsonProperty(Order = 5, PropertyName = "GainColor")]
		public string GainColorString
		{
			get { return GainColor.PackedValue.ToString("x8"); }
			set { GainColor = ColorHelpers.FromHexString(value); }
		}

		public Color GainColor { get; set; } = Color.White;

		[JsonProperty(Order = 6, PropertyName = "LossColor")]
		public string LossColorString
		{
			get { return LossColor.PackedValue.ToString("x8"); }
			set { LossColor = ColorHelpers.FromHexString(value); }
		}

		public Color LossColor { get; set; } = Color.Red;

		public override string ToString()
		{
			return FullName;// ('{Abbreviation}')";
		}

		public ValidationResult Validate()
		{
			var result = new ValidationResult(this);

			if (string.IsNullOrWhiteSpace(FullName))
				result.Errors.Add(new ValidationError($"{nameof(FullName)} is null or whitespace."));

			if (string.IsNullOrWhiteSpace(Abbreviation))
				result.Errors.Add(new ValidationError($"{nameof(Abbreviation)} is null or whitespace."));

			if (BaseUnitMultiplier==0)
				result.Warnings.Add(new ValidationWarning($"{nameof(BaseUnitMultiplier)} is 0."));

			return result;
		}
	}
}
