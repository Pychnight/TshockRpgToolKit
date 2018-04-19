using System;
using System.Linq;
using CustomQuests.Quests;
using JetBrains.Annotations;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents an in-area trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class InArea : Trigger
    {
        private readonly bool requireEveryone;
        private readonly int _maxX;
        private readonly int _maxY;
        private readonly int _minX;
        private readonly int _minY;
		private readonly Party party;
		
		/// <summary>
		///     Initializes a new instance of the <see cref="InArea" /> class with the specified party and positions.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="x">The first X position.</param>
		/// <param name="y">The first Y position.</param>
		/// <param name="x2">The second X position.</param>
		/// <param name="y2">The second Y position.</param>
		/// <param name="requireEveryone"><c>true</c> if everyone in the party must be in the area; otherwise, <c>false</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="party" /> is <c>null</c>.</exception>
		public InArea( Party party, int x, int y, int x2, int y2, bool requireEveryone)
		{
			this.requireEveryone = requireEveryone;
			this.party = party ?? throw new ArgumentNullException(nameof(party));
			_maxX = Math.Max(x, x2);
			_minX = Math.Min(x, x2);
			_maxY = Math.Max(y, y2);
			_minY = Math.Min(y, y2);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="InArea" /> class with the specified party and positions.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="x">The first X position.</param>
		/// <param name="y">The first Y position.</param>
		/// <param name="x2">The second X position.</param>
		/// <param name="y2">The second Y position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="party" /> is <c>null</c>.</exception>
		public InArea(Party party, int x, int y, int x2, int y2) : this(party,x,y,x2,y2,true)
		{
		}

		/// <inheritdoc />
		protected override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override bool UpdateImpl() =>
            requireEveryone
                ? party.All(p => _minX <= p.TileX && p.TileX <= _maxX && _minY <= p.TileY && p.TileY <= _maxY)
                : party.Any(p => _minX <= p.TileX && p.TileX <= _maxX && _minY <= p.TileY && p.TileY <= _maxY);
    }
}
