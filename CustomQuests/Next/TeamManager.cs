using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Next
{
	public class TeamManager : IEnumerable<Team>
	{
		Party party;
		List<Team> teams;

		internal TeamManager(Party party)
		{
			this.party = party;
			teams = new List<Team>();
		}

		public int Count => teams.Count;
		public void Clear() => teams.Clear();

		public IEnumerator<Team> GetEnumerator()
		{
			return teams.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
