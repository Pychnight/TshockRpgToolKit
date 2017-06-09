using System;
using JetBrains.Annotations;
using TShockAPI;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents an in-area trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class InArea : Trigger
    {
        private readonly int _maxX;
        private readonly int _maxY;
        private readonly int _minX;
        private readonly int _minY;
        private readonly TSPlayer _player;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InArea" /> class with the specified player and positions.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <param name="x">The first X position.</param>
        /// <param name="y">The first Y position.</param>
        /// <param name="x2">The second X position.</param>
        /// <param name="y2">The second Y position.</param>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
        public InArea([NotNull] TSPlayer player, int x, int y, int x2, int y2)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            _player = player;
            _maxX = Math.Max(x, x2);
            _minX = Math.Min(x, x2);
            _maxY = Math.Max(y, y2);
            _minY = Math.Min(y, y2);
        }

        /// <inheritdoc />
        public override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override bool UpdateImpl() =>
            _minX <= _player.TileX && _player.TileX <= _maxX && _minY <= _player.TileY && _player.TileY <= _maxY;
    }
}
