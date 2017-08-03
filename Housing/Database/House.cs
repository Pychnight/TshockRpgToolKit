using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Wolfje.Plugins.SEconomy;

namespace Housing.Database
{
    /// <summary>
    ///     Represents a house.
    /// </summary>
    public sealed class House
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="House" /> class with the specified owner name, name, and coordinates.
        /// </summary>
        /// <param name="ownerName">The owner name.</param>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <param name="x">The first X coordinate.</param>
        /// <param name="y">The first Y coordinate.</param>
        /// <param name="x2">The second X coordinate, which must be at least the first.</param>
        /// <param name="y2">The second Y coordinate, which must be at least the second.</param>
        public House(string ownerName, string name, int x, int y, int x2, int y2)
        {
            Debug.Assert(ownerName != null, "Owner name must not be null.");
            Debug.Assert(name != null, "Name must not be null.");
            Debug.Assert(x2 >= x, "Second X coordinate must be at least the first.");
            Debug.Assert(y2 >= y, "Second Y coordinate must be at least the first.");

            OwnerName = ownerName;
            Name = name;
            Rectangle = new Rectangle(x, y, x2 - x + 1, y2 - y + 1);
        }

        /// <summary>
        ///     Gets the set of allowed user names.
        /// </summary>
        public ISet<string> AllowedUsernames { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets the area.
        /// </summary>
        public int Area => Rectangle.Width * Rectangle.Height;

        /// <summary>
        ///     Gets or sets the debt.
        /// </summary>
        public Money Debt { get; set; }

        /// <summary>
        ///     Gets or sets the last time the house was taxed.
        /// </summary>
        public DateTime LastTaxed { get; set; } = DateTime.UtcNow;

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

        public override string ToString() => Name;
    }
}
