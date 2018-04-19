using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Quests
{
	public class Team : IEnumerable<PartyMember>
	{
		TeamManager manager;
		List<PartyMember> members;

		public string Name { get; set; }

		internal Team(TeamManager manager)
		{
			this.manager = manager;
			members = new List<PartyMember>();
		}

		public IEnumerator<PartyMember> GetEnumerator()
		{
			return members.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
