using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Wolfje.Plugins.SEconomy;

namespace Housing.Database
{
    /// <summary>
    ///     Represents a shop.
    /// </summary>
    public sealed class Shop
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Shop" /> class with the specified owner name, name, and coordinates.
        /// </summary>
        /// <param name="ownerName">The owner name.</param>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <param name="x">The first X coordinate.</param>
        /// <param name="y">The first Y coordinate.</param>
        /// <param name="x2">The second X coordinate, which must be at least the first.</param>
        /// <param name="y2">The second Y coordinate, which must be at least the second.</param>
        /// <param name="chestX">The chest X coordinate.</param>
        /// <param name="chestY">The chest Y coordinate.</param>
        public Shop(string ownerName, string name, int x, int y, int x2, int y2, int chestX, int chestY)
        {
            Debug.Assert(ownerName != null, "Owner name must not be null.");
            Debug.Assert(name != null, "Name must not be null.");
            Debug.Assert(x2 >= x, "Second X coordinate must be at least the first.");
            Debug.Assert(y2 >= y, "Second Y coordinate must be at least the first.");

            OwnerName = ownerName;
            Name = name;
            Rectangle = new Rectangle(x, y, x2 - x + 1, y2 - y + 1);
            ChestX = chestX;
            ChestY = chestY;
        }

        /// <summary>
        ///     Gets or sets the chest X coordinate.
        /// </summary>
        public int ChestX { get; set; }

        /// <summary>
        ///     Gets or sets the chest Y coordinate.
        /// </summary>
        public int ChestY { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the shop is being changed.
        /// </summary>
        public bool IsBeingChanged { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the shop is open.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        ///     Gets the items.
        /// </summary>
        public IList<ShopItem> Items { get; } = new List<ShopItem>();

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the owner name.
        /// </summary>
        public string OwnerName { get; }

        /// <summary>
        ///     Gets or sets the rectangle.
        /// </summary>
        public Rectangle Rectangle { get; set; }

        /// <summary>
        ///     Gets the unit prices.
        /// </summary>
        public IDictionary<int, Money> UnitPrices { get; } = new Dictionary<int, Money>();

        public override string ToString() => Name;
    }
}
