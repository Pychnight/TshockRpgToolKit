using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Quests
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

		private int nextTeamIndex => teams.Count;

		public int Count => teams.Count;

		public Team this[int index]
		{
			get
			{
				if( index > -1 && index < teams.Count )
					return teams[index];

				return null;
			}
		}
		
		public IEnumerator<Team> GetEnumerator()
		{
			return teams.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Clear()
		{
			foreach( var t in teams )
			{
				t.Clear();
				t.Manager = null;
			}
		}

		public Team CreateTeam(string name)
		{
			var team = new Team(this, nextTeamIndex, name);
			
			teams.Add(team);
						
			return team;
		}

		public Team CreateTeam()
		{
			return CreateTeam(null);
		}
	}
}
