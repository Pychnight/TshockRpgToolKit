using System.Diagnostics;
using Housing.Database;
using TShockAPI;

namespace Housing
{
    /// <summary>
    ///     Holds session data.
    /// </summary>
    public sealed class Session
    {
        private readonly TSPlayer _player;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Session" /> class with the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        public Session(TSPlayer player)
        {
            Debug.Assert(player != null, "Player must not be null.");

            _player = player;
        }

        /// <summary>
        ///     Gets or sets the current house.
        /// </summary>
        public House CurrentHouse { get; set; }

        /// <summary>
        ///     Gets or sets the currently viewed shop.
        /// </summary>
        public Shop CurrentlyViewedShop { get; set; }

        /// <summary>
        ///     Gets or sets the current shop.
        /// </summary>
        public Shop CurrentShop { get; set; }

        /// <summary>
        ///     Gets or sets the next shop's house.
        /// </summary>
        public House NextShopHouse { get; set; }

        /// <summary>
        ///     Gets or sets the next shop's name.
        /// </summary>
        public string NextShopName { get; set; }

        /// <summary>
        ///     Gets or sets the next shop's first X coordinate.
        /// </summary>
        public int NextShopX { get; set; }

        /// <summary>
        ///     Gets or sets the next shop's second X coordinate.
        /// </summary>
        public int NextShopX2 { get; set; }

        /// <summary>
        ///     Gets or sets the next shop's first Y coordinate.
        /// </summary>
        public int NextShopY { get; set; }

        /// <summary>
        ///     Gets or sets the next shop's second Y coordinate.
        /// </summary>
        public int NextShopY2 { get; set; }
    }
}
