using Newtonsoft.Json;
//using OTAPI.Tile;
using System;
using System.ComponentModel;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Represents a value to be used for an item, instead of relying on the default value generation.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject(MemberSerialization.OptIn)]
	public class ValueOverride<TKey> : ICloneable where TKey : ICloneable //where TKey : new() -- cant use this because of System.String...
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

		public object Clone()
		{
			var dest = new ValueOverride<TKey>();

			dest.Key = (TKey)Key?.Clone();
			dest.ValueString = ValueString;

			return dest;
		}

		public override string ToString() => $"{Key} = {ValueString}";
	}
}
