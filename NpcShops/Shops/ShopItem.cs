using Corruption.PluginSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TShockAPI;
//using Wolfje.Plugins.SEconomy;

namespace NpcShops.Shops
{
	/// <summary>
	///     Represents a shop item.
	/// </summary>
	public sealed class ShopItem : ShopProduct
	{
		private readonly ShopItemDefinition _definition;

		/// <summary>
		///     Initializes a new instance of the <see cref="ShopItem" /> class with the specified definition.
		/// </summary>
		/// <param name="definition">The definition, which must not be <c>null</c>.</param>
		public ShopItem(ShopItemDefinition definition)
		{
			Debug.Assert(definition != null, "Definition must not be null.");

			_definition = definition;

			IsValid = TryResolvePricing(_definition.UnitPrice);
			if( !IsValid )
			{
				NpcShopsPlugin.Instance.LogPrint($"Unable to find Currency for ShopItem {_definition.ItemName}, hiding item.", TraceLevel.Warning);
			}

			//we don't really need to do anything past here, if IsValid is false, but for now...
			
			var item = TShock.Utils.GetItemByIdOrName(definition.ItemName).SingleOrDefault();

			if(item==null)
			{
				throw new KeyNotFoundException($"Could not find Id or Name '{definition.ItemName}'.");
			}

			ItemId = item.type;
			MaxStackSize = item.maxStack;
			StackSize = definition.StackSize;
			RequiredItems = new List<RequiredItem>();

			var distinct = definition.RequiredItems.Distinct();
			var items = distinct.Select(d => new RequiredItem(d));

			RequiredItems.AddRange(items);
		
		}

		/// <summary>
		///     Gets the item ID.
		/// </summary>
		public int ItemId { get; }

		/// <summary>
		///     Gets the maximum stack size.
		/// </summary>
		public int MaxStackSize { get; }

		/// <summary>
		///     Gets the permission required.
		/// </summary>
		public string PermissionRequired => _definition.PermissionRequired;

		/// <summary>
		///     Gets the prefix ID.
		/// </summary>
		public byte PrefixId => _definition.PrefixId;
		
		/// <summary>
		///     Restocks the shop item.
		/// </summary>
		public override void Restock()
        {
            StackSize = _definition.StackSize;
        }
	}
}
