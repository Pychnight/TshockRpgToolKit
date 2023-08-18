using Newtonsoft.Json;
using System;

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

		public bool Equals(RequiredItemDefinition other) => ItemName == other.ItemName && PrefixId == other.PrefixId;
	}
}
