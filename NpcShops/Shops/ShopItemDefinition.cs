using Banking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NpcShops.Shops
{
	/// <summary>
	///     Represents a shop item definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ShopItemDefinition
	{
		/// <summary>
		///     Gets the item name.
		/// </summary>
		[JsonProperty("Item", Order = 0)]
		public string ItemName { get; private set; }

		/// <summary>
		///     Gets the stack size. A value of -1 indicates unlimited.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int StackSize { get; private set; }

		/// <summary>
		///     Gets the prefix ID.
		/// </summary>
		[JsonProperty("Prefix", Order = 2)]
		public byte PrefixId { get; private set; }

		//string unitPrice;

		/// <summary>
		///     Gets the unit price.
		/// </summary>
		[JsonProperty(Order = 3)]
		public string UnitPrice { get; set; }
		
		///// <summary>
		/////		Gets the numeric value of the UnitPrice string.
		///// </summary>
		//internal decimal UnitPriceMoney { get; private set; }
		
		/// <summary>
		///     Gets the permission required.
		/// </summary>
		[JsonProperty(Order = 4)]
        public string PermissionRequired { get; private set; }

		/// <summary>
		///		Gets the required items for purchase.
		/// </summary>
		[JsonProperty(Order = 5)]
		public List<RequiredItemDefinition> RequiredItems { get; private set; } = new List<RequiredItemDefinition>();
    }
}
