using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Wolfje.Plugins.SEconomy;

namespace Housing.HousingEntites
{ 
    /// <summary>
    ///     Represents a shop item.
    /// </summary>
    public class ShopItem : HousingEntity
    {
            /// <summary>
            ///     Initializes a new instance of the <see cref="ShopItem" /> class with the specified item ID, stack size, and prefix.
            /// </summary>
            /// <param name="index">The index, which must be non-negative and in range.</param>
            /// <param name="itemId">The item ID, which must be non-negative and in range.</param>
            /// <param name="stackSize">The stack size, which must be non-negative.</param>
            /// <param name="prefixId">The prefix ID, which must be in range.</param>
            public ShopItem(int index, int itemId, int stackSize, byte prefixId)
            {
                Debug.Assert(index >= 0, "Index must be non-negative.");
                Debug.Assert(index < Chest.maxItems, "Index must be in range.");
                Debug.Assert(itemId >= 0, "Item ID must be non-negative.");
                Debug.Assert(itemId < ItemID.Count, "Item ID must be in range.");
                Debug.Assert(stackSize >= 0, "Stack size must be non-negative.");
                Debug.Assert(prefixId < PrefixID.Count, "Prefix ID must be in range.");

                Index = index;
                ItemId = itemId;
                StackSize = stackSize;
                PrefixId = prefixId;
            }

            /// <summary>
            ///     Gets the index.
            /// </summary>
            public int Index { get; }

            /// <summary>
            ///     Gets or sets the item ID.
            /// </summary>
            public int ItemId { get; set; }

            /// <summary>
            ///     Gets or sets the prefix ID.
            /// </summary>
            public byte PrefixId { get; set; }

            /// <summary>
            ///     Gets or sets the stack size.
            /// </summary>
            public int StackSize { get; set; }
    }
}