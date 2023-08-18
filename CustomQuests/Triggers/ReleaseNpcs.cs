using CustomQuests.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomQuests.Triggers
{
	public class ReleaseNpcs : Trigger
	{
		IEnumerable<PartyMember> partyMembers;
		int amount;
		HashSet<string> npcTypes;
		//Rectangle releaseArea; //because Triggers are composable, we dont have a need for release area right now... TBD.

		public ReleaseNpcs(IEnumerable<PartyMember> partyMembers, int amount, params object[] npcTypes)
		{
			this.partyMembers = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));
			this.amount = amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
			this.npcTypes = BuildNpcNameHashSet(npcTypes);
		}

		public ReleaseNpcs(IEnumerable<PartyMember> partyMembers, int amount)
			: this(partyMembers, amount, null)
		{
		}

		public ReleaseNpcs(PartyMember partyMember, int amount, params object[] npcTypes)
			: this(partyMember.ToEnumerable(), amount, npcTypes)
		{
		}

		public ReleaseNpcs(PartyMember partyMember, int amount)
			: this(partyMember, amount, null)
		{
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CustomQuestsPlugin.Instance.ReleaseNpc -= onReleaseNpc;
			}

			base.Dispose(disposing);
		}

		/// <inheritdoc />
		protected override void Initialize() => CustomQuestsPlugin.Instance.ReleaseNpc += onReleaseNpc;

		private void onReleaseNpc(object sender, ReleaseNpcEventArgs e)
		{
			if (partyMembers.Any(m => m.IsValidMember && m.Index == e.PlayerIndex))
			{
				if (npcTypes != null)
				{
					var name = GetNPCName((int)e.NpcType);

					if (!string.IsNullOrWhiteSpace(name) && npcTypes.Contains(name))
					{
						amount--;
					}
				}
				else
				{
					amount--;
				}
			}
		}

		protected internal override TriggerStatus UpdateImpl() => amount < 1 ? TriggerStatus.Success : TriggerStatus.Running;
	}
}
