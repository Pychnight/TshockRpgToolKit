using Newtonsoft.Json;
//using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Represents a value to be used for an item, instead of relying on the default value generation.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject(MemberSerialization.OptIn)]
	public class ValueOverride<TKey> //where TKey : new() -- cant use this because of System.String...
	{
		/// <summary>
		/// Gets or sets the key. 
		/// </summary>
		[JsonProperty(Order = 0)]
		public TKey Key { get; set; } //= new TKey();

		/// <summary>
		/// Gets or sets the string representation of the value.
		/// </summary>
		[DisplayName("Value")]
		[JsonProperty(Order = 1, PropertyName = "Value")]
		public string ValueString { get; set; }

		/// <summary>
		/// Gets or sets the parsed value, in generic units.
		/// </summary>
		[Browsable(false)]
		public decimal Value { get; set; }
		
		///// <summary>
		///// Performs necessary initialization in order to use this ValueOverride. The base implementation attempts to parse the ValueString
		///// into a generic unit Value from the Currency. If this fails, it tries to parse a raw decimal value from the string. If this also fails,
		///// Value is left  untouched.
		///// </summary>
		///// <param name="currency">Currency this override belongs to.</param>
		//public virtual void Initialize(CurrencyDefinition currency)
		//{
		//	Debug.Assert(currency != null, "CurrencyDefinition must not be null.");

		//	if( string.IsNullOrWhiteSpace(ValueString) )
		//		return;

		//	var converter = currency.GetCurrencyConverter();

		//	if( converter.TryParse(ValueString, out var newValue) )
		//	{
		//		Value = newValue;
		//		return;
		//	}

		//	if( decimal.TryParse(ValueString, out newValue) )
		//	{
		//		Value = newValue;
		//		return;
		//	}
		//}

		public override string ToString()
		{
			return $"{Key} = {ValueString}";
		}
	}
}
