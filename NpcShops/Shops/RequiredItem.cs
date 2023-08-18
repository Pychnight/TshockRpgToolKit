using System.Linq;
using TShockAPI;

namespace NpcShops.Shops
{
	/// <summary>
	/// Represents a material cost to purchase items or commands from a shop.
	/// </summary>
	public class RequiredItem
	{
		RequiredItemDefinition definition;

		/// <summary>
		///		Gets the item's id, aka type.
		/// </summary>
		public int ItemId { get; private set; }

		/// <summary>
		///     Gets the item name.
		/// </summary>
		public string ItemName => definition.ItemName;

		/// <summary>
		///     Gets the stack size. 
		/// </summary>
		public int StackSize => definition.StackSize;

		/// <summary>
		///     Gets the prefix ID.
		/// </summary>
		public byte PrefixId => definition.PrefixId;

		public RequiredItem(RequiredItemDefinition definition)
		{
			this.definition = definition;

			var item = TShock.Utils.GetItemByIdOrName(definition.ItemName).Single();
			ItemId = item.type;
		}
	}
}
