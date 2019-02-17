using CustomQuests.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace CustomQuests.Triggers
{
	public class CatchNpcs : Trigger
	{
		IEnumerable<PartyMember> partyMembers;
		int amount;
		HashSet<string> npcTypes;
		
		public CatchNpcs(IEnumerable<PartyMember> partyMembers, int amount, params object[] npcTypes)
		{
			this.partyMembers = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));
			this.amount = amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
			this.npcTypes = BuildNpcNameHashSet(npcTypes);
		}

		public CatchNpcs(IEnumerable<PartyMember> partyMembers, int amount)
			: this(partyMembers, amount, null)
		{
		}

		public CatchNpcs(PartyMember partyMember, int amount, params object[] npcTypes)
			: this(partyMember.ToEnumerable(), amount, npcTypes)
		{
		}

		public CatchNpcs(PartyMember partyMember, int amount)
			: this(partyMember, amount, null)
		{
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				CustomQuestsPlugin.Instance.CatchNpc -= onCatchNpc;
			}

			base.Dispose(disposing);
		}

		/// <inheritdoc />
		protected override void Initialize()
		{
			CustomQuestsPlugin.Instance.CatchNpc += onCatchNpc;
		}

		private void onCatchNpc(object sender, CatchNpcEventArgs e)
		{
			if( partyMembers.Any(m => m.IsValidMember && m.Index == e.PlayerIndex) )
			{
				if( npcTypes != null )
				{
					var name = Main.npc[e.NpcId]?.GivenOrTypeName;

					if(name!=null && npcTypes.Contains(name))
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

		protected internal override TriggerStatus UpdateImpl()
		{
			return amount < 1 ? TriggerStatus.Success : TriggerStatus.Running;
		}
	}
}
