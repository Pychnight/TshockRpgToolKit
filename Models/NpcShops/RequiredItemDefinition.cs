using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models
{
	/// <summary>
	/// Represents a materials requirement for purchase of a ShopProduct.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class RequiredItemDefinition : IEquatable<RequiredItemDefinition>
	{
		/// <summary>
		///     Gets the item name.
		/// </summary>
		[TypeConverter(typeof(ItemNameStringConverter))]
		[JsonProperty("Item", Order = 0)]
		public string ItemName { get; set; }

		/// <summary>
		///     Gets the stack size. 
		/// </summary>
		[JsonProperty(Order = 1)]
		public int StackSize { get; set; } = 1;

		/// <summary>
		///     Gets the prefix ID.
		/// </summary>
		[JsonProperty("Prefix", Order = 2)]
		public byte PrefixId { get; set; }

		public bool Equals(RequiredItemDefinition other)
		{
			return ItemName == other.ItemName && PrefixId == other.PrefixId;
		}

		public RequiredItemDefinition()
		{
		}

		public RequiredItemDefinition(RequiredItemDefinition other)
		{
			ItemName = other.ItemName;
			StackSize = other.StackSize;
			PrefixId = other.PrefixId;
		}

		public override string ToString()
		{
			if( string.IsNullOrWhiteSpace(ItemName) )
				return $"<No Item> x {StackSize}";
			else
				return $"{ItemName} x {StackSize}";
		}
	}
}
