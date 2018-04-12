using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomQuests.Next
{
	public class Party : IEnumerable<PartyMember>
	{
		List<PartyMember> partyMembers;

		public int Count => partyMembers.Count;
		public PartyMember this[int index] => partyMembers[index];
		public PartyMember Leader => partyMembers[0];
		public TeamManager Teams { get; private set; }

		internal Party(params TSPlayer[] players) : this( players.AsEnumerable())
		{
		}

		internal Party(IEnumerable<TSPlayer> players)
		{
			partyMembers = new List<PartyMember>(players.Select(p => new PartyMember(p)));
			Teams = new TeamManager(this);
		}

		public IEnumerator<PartyMember> GetEnumerator()
		{
			return partyMembers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
