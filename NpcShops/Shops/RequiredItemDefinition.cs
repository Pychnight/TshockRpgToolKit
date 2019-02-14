using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcShops.Shops
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
		[JsonProperty("Item", Order = 0)]
		public string ItemName { get; private set; }

		/// <summary>
		///     Gets the stack size. 
		/// </summary>
		[JsonProperty(Order = 1)]
		public int StackSize { get; private set; }

		/// <summary>
		///     Gets the prefix ID.
		/// </summary>
		[JsonProperty("Prefix", Order = 2)]
		public byte PrefixId { get; private set; }

		public bool Equals(RequiredItemDefinition other)
		{
			return ItemName == other.ItemName && PrefixId == other.PrefixId;
		}
	}
}
