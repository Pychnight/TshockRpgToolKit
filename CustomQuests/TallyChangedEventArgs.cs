using CustomQuests.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests
{
	/// <summary>
	/// Provides information on a <see cref="CustomQuests.Quests.PartyMember"/>'s contribution( or losses )
	/// for Triggers that require a quota to be met. 
	/// </summary>
	public class TallyChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the <see cref="CustomQuests.Quests.PartyMember"/> who made the gain or loss.
		/// </summary>
		public PartyMember PartyMember { get; private set; }

		/// <summary>
		/// Gets the gain or loss made by the <see cref="PartyMember"/>.
		/// </summary>
		public int TallyChange { get; private set; }

		public TallyChangedEventArgs(PartyMember partyMember, int tallyChange )
		{
			PartyMember = partyMember;
			TallyChange = tallyChange;
		}
	}
}
