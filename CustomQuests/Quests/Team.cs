using System.Collections;
using System.Collections.Generic;

namespace CustomQuests.Quests
{
	public class Team : IEnumerable<PartyMember>
	{
		internal TeamManager Manager;
		List<PartyMember> members;

		public int Index { get; internal set; }
		public string Name { get; set; }
		public int Count => members.Count;

		internal Team(TeamManager manager, int index, string name)
		{
			this.Manager = manager;
			Index = index;
			members = new List<PartyMember>();
			Name = string.IsNullOrWhiteSpace(name) ? getDefaultName() : name;
		}

		public IEnumerator<PartyMember> GetEnumerator() => members.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Clear()
		{
			foreach (var m in members)
				m.Team = null;

			members.Clear();
		}

		public void Add(PartyMember member)
		{
			//is this a discarded team?
			if (Manager == null)
				return;

			if (member == null)
				return;

			if (member.HasTeam)
			{
				if (member.Team == this)
					return;

				member.Team.Remove(member);
			}

			members.Add(member);
			member.Team = this;
		}

		public void Remove(PartyMember member)
		{
			if (member == null)
				return;

			if (member.Team == this)
			{
				members.Remove(member);
				member.Team = null;
			}
		}

		private string getDefaultName() => $"Team {Index}";

		public override string ToString() => string.IsNullOrWhiteSpace(Name) ? getDefaultName() : Name;
	}
}
