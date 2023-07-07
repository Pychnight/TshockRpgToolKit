using System.Collections;
using System.Collections.Generic;

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
				if (index > -1 && index < teams.Count)
					return teams[index];

				return null;
			}
		}

		public IEnumerator<Team> GetEnumerator() => teams.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Clear()
		{
			foreach (var t in teams)
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

		public Team CreateTeam() => CreateTeam(null);
	}
}
