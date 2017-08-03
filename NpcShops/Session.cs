using System.Diagnostics;
using NpcShops.Shops;
using TShockAPI;

namespace NpcShops
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
        ///     Gets or sets the current shop.
        /// </summary>
        public NpcShop CurrentShop { get; set; }
    }
}
