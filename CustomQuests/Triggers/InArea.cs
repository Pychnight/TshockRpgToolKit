using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CustomQuests.Quests;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents an in-area trigger.
    /// </summary>
    public sealed class InArea : Trigger
    {
        private readonly bool requireEveryone;
        private readonly int _maxX;
        private readonly int _maxY;
        private readonly int _minX;
        private readonly int _minY;
		IEnumerable<PartyMember> partyMembers;
		
		/// <summary>
		///     Initializes a new instance of the <see cref="InArea" /> class with the specified party and positions.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="x">The first X position.</param>
		/// <param name="y">The first Y position.</param>
		/// <param name="x2">The second X position.</param>
		/// <param name="y2">The second Y position.</param>
		/// <param name="requireEveryone"><c>true</c> if everyone in the party must be in the area; otherwise, <c>false</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="partyMembers" /> is <c>null</c>.</exception>
		public InArea( IEnumerable<PartyMember> partyMembers, int x, int y, int x2, int y2, bool requireEveryone)
		{
			this.requireEveryone = requireEveryone;
			//this.party = party ?? throw new ArgumentNullException(nameof(party));
			this.partyMembers = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));
			_maxX = Math.Max(x, x2);
			_minX = Math.Min(x, x2);
			_maxY = Math.Max(y, y2);
			_minY = Math.Min(y, y2);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="InArea" /> class with the specified party and positions.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="x">The first X position.</param>
		/// <param name="y">The first Y position.</param>
		/// <param name="x2">The second X position.</param>
		/// <param name="y2">The second Y position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="partyMembers" /> is <c>null</c>.</exception>
		public InArea(IEnumerable<PartyMember> partyMembers, int x, int y, int x2, int y2) : this(partyMembers, x, y, x2, y2, true)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="InArea" /> class with the specified party and positions.
		/// </summary>
		/// <param name="partyMember">The party member, which must not be <c>null</c>.</param>
		/// <param name="x">The first X position.</param>
		/// <param name="y">The first Y position.</param>
		/// <param name="x2">The second X position.</param>
		/// <param name="y2">The second Y position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="partyMembers" /> is <c>null</c>.</exception>
		public InArea(PartyMember partyMember, int x, int y, int x2, int y2)
			: this(partyMember.ToEnumerable(), x, y, x2, y2, false)
		{
		}
		
		/// <inheritdoc />
		protected override void Initialize()
        {
        }

        /// <inheritdoc />
        protected internal override TriggerStatus UpdateImpl()
		{
			if( requireEveryone )
			{
				var validMembers = partyMembers.GetValidMembers();

				if( validMembers.Any())
					return validMembers.All(p => _minX <= p.TileX && p.TileX <= _maxX && _minY <= p.TileY && p.TileY <= _maxY)
										.ToTriggerStatus();
				else
					return TriggerStatus.Fail;
			}
			else
				return partyMembers.Any(p => p.IsValidMember && _minX <= p.TileX && p.TileX <= _maxX && _minY <= p.TileY && p.TileY <= _maxY)
									.ToTriggerStatus();
		}
	}
}
