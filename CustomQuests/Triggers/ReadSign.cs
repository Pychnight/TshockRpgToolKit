using System;
using System.Collections.Generic;
using System.Linq;
using CustomQuests.Quests;

namespace CustomQuests.Triggers
{
	/// <summary>
	///     Trigger that requires a sign be read.
	/// </summary>
	public sealed class ReadSign : Trigger
	{
		IEnumerable<PartyMember> partyMembers;
		private readonly bool requireEveryone;
		private readonly int signX;
		private readonly int signY;
		HashSet<int> playersWhoRead; //set of player indices who've read the sign.
				
		/// <summary>
		///     Initializes a new instance of the <see cref="ReadSign" /> class with the specified party and sign position.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="x">The sign X position, in tiles.</param>
		/// <param name="y">The sign Y position, in tiles.</param>
		/// <param name="requireEveryone"><c>true</c> if everyone in the party must read the sign; otherwise, <c>false</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="partyMembers" /> is <c>null</c>.</exception>
		public ReadSign(IEnumerable<PartyMember> partyMembers, int x, int y, bool requireEveryone)
		{
			this.partyMembers = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));
			signX = x;
			signY = y;
			this.requireEveryone = requireEveryone;
			playersWhoRead = new HashSet<int>();
		}

		public ReadSign(IEnumerable<PartyMember> partyMembers, int x, int y)
			: this(partyMembers, x, y, true)
		{
		}

		public ReadSign(PartyMember partyMember, int x, int y)
			: this(partyMember.ToEnumerable(),x,y)
		{
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if( disposing )
				CustomQuestsPlugin.Instance.SignRead += onSignRead;
			
			base.Dispose(disposing);
		}

		/// <inheritdoc />
		protected override void Initialize()
		{
			CustomQuestsPlugin.Instance.SignRead += onSignRead;
		}

		private void onSignRead(object sender, SignReadEventArgs args)
		{
			if( args.X != signX || args.Y != signY )
				return;

			var inParty = partyMembers.FirstOrDefault(m => m.Index == args.PlayerIndex);

			if( inParty != null )
				playersWhoRead.Add(args.PlayerIndex);
		}

		/// <inheritdoc />
		protected internal override bool UpdateImpl()
		{
			if( requireEveryone )
				return partyMembers.All(m => playersWhoRead.Contains(m.Index));
			else
				return playersWhoRead.Count > 0;
		}
	}
}
