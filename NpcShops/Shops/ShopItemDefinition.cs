using Banking;
using Corruption.PluginSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TShockAPI;

namespace NpcShops.Shops
{
	/// <summary>
	///     Represents a shop item definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ShopItemDefinition : IValidator
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
		///     Gets or sets the unit price.
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

		public ValidationResult Validate()
		{
			var result = new ValidationResult(this);

			if(string.IsNullOrWhiteSpace(ItemName))
				result.Errors.Add(new ValidationError($"{nameof(ItemName)} is null or empty."));
			else
			{
				var itemFound = TShock.Utils.GetItemByIdOrName(ItemName).SingleOrDefault();

				if(itemFound==null)
					result.Errors.Add(new ValidationError($"Cound not find Terraria Id or Name matching {nameof(ItemName)} '{ItemName}'."));
			}

			if (StackSize == 0)
				result.Warnings.Add(new ValidationWarning($"{nameof(StackSize)} is 0."));
			
			if (string.IsNullOrWhiteSpace(UnitPrice))
				result.Warnings.Add(new ValidationWarning($"{nameof(UnitPrice)} is null or empty."));
			
			return result;
		}
	}
}
