using System;
using System.Collections.Generic;
using System.Linq;
using CustomQuests.Quests;

namespace CustomQuests.Triggers
{
	/// <summary>
	///     Trigger that requires a sign be read.
	/// </summary>
	public sealed class ChangeSign : Trigger
	{
		IEnumerable<PartyMember> partyMembers;
		private readonly bool requireEveryone;
		private readonly int signX;
		private readonly int signY;
		Dictionary<int, bool> playerOkay;//player indices, and if they've passed any optional callback verification.
		Func<PartyMember, string, bool> changeFunc;

		/// <summary>
		///     Initializes a new instance of the <see cref="ReadSign" /> class with the specified party and sign position.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="x">The sign X position, in tiles.</param>
		/// <param name="y">The sign Y position, in tiles.</param>
		/// <param name="requireEveryone"><c>true</c> if everyone in the party must change the sign; otherwise, <c>false</c>.</param>
		/// <param name="changeFunc">Optional callback that runs per text change, and can succeed or fail.</param>
		/// <exception cref="ArgumentNullException"><paramref name="partyMembers" /> is <c>null</c>.</exception>
		public ChangeSign(IEnumerable<PartyMember> partyMembers, int x, int y, bool requireEveryone, Func<PartyMember,string,bool> changeFunc)
		{
			this.partyMembers = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));
			signX = x;
			signY = y;
			this.requireEveryone = requireEveryone;
			playerOkay = new Dictionary<int, bool>();

			this.changeFunc = changeFunc;
		}

		public ChangeSign(PartyMember partyMember, int x, int y, bool requireEveryone, Func<PartyMember, string, bool> changeFunc)
			: this(partyMember.ToEnumerable(), x, y, requireEveryone, changeFunc)
		{
		}

		public ChangeSign(IEnumerable<PartyMember> partyMembers, int x, int y)
			: this(partyMembers, x, y, true, null)
		{
		}

		public ChangeSign(PartyMember partyMember, int x, int y)
			: this(partyMember.ToEnumerable(), x, y)
		{
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if( disposing )
				CustomQuestsPlugin.Instance.SignChanged += onSignChanged;

			base.Dispose(disposing);
		}

		/// <inheritdoc />
		protected override void Initialize()
		{
			CustomQuestsPlugin.Instance.SignChanged += onSignChanged;
		}

		private void onSignChanged(object sender, SignChangedEventArgs args)
		{
			if( args.X != signX || args.Y != signY )
				return;

			var partyMember = partyMembers.FirstOrDefault(m => m.Index == args.PlayerIndex);

			if( partyMember != null )
			{
				if(changeFunc!=null)
				{
					var result = changeFunc(partyMember, args.Text);
					playerOkay[args.PlayerIndex] = result;
				}
				else
				{
					playerOkay[args.PlayerIndex] = true;
				}
			}
		}

		/// <inheritdoc />
		protected internal override TriggerStatus UpdateImpl()
		{
			if( requireEveryone )
			{
				var validMembers = partyMembers.GetValidMembers();

				if( validMembers.Any() )
				{
					return validMembers.All(m => playerOkay.ContainsKey(m.Index) && playerOkay[m.Index] == true)
										.ToTriggerStatus();
				}
				else
					return TriggerStatus.Fail;
			}
			else
				return partyMembers.Any(m => m.IsValidMember && playerOkay.ContainsKey(m.Index) && playerOkay[m.Index] == true)
									.ToTriggerStatus();
		}
	}
}
