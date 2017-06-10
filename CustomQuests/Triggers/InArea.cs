using System;
using System.Linq;
using JetBrains.Annotations;

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
        private readonly Party _party;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InArea" /> class with the specified party and positions.
        /// </summary>
        /// <param name="party">The party, which must not be <c>null</c>.</param>
        /// <param name="x">The first X position.</param>
        /// <param name="y">The first Y position.</param>
        /// <param name="x2">The second X position.</param>
        /// <param name="y2">The second Y position.</param>
        /// <exception cref="ArgumentNullException"><paramref name="party" /> is <c>null</c>.</exception>
        public InArea([NotNull] Party party, int x, int y, int x2, int y2)
        {
            _party = party ?? throw new ArgumentNullException(nameof(party));
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
            _party.All(p => _minX <= p.TileX && p.TileX <= _maxX && _minY <= p.TileY && p.TileY <= _maxY);
    }
}
