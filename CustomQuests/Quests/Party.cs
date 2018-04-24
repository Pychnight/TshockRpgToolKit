using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomQuests.Quests
{
	public class Party : IEnumerable<PartyMember>
	{
		List<PartyMember> partyMembers;
		//Dictionary<int, int> playerIndexToPartyIndex;

		#region OldParty Compatibility
		
		public string Name { get; private set; }
		
		#endregion

		public int Count => partyMembers.Count;
		public PartyMember this[int index] => partyMembers[index];
		public PartyMember Leader => partyMembers[0];
		public TeamManager Teams { get; private set; }

		internal Party(string name, params TSPlayer[] players) : this( name, players.AsEnumerable())
		{
		}

		internal Party(string name, IEnumerable<TSPlayer> players)
		{
			Name = name;
			//playerIndexToPartyIndex = new Dictionary<int, int>();
			partyMembers = new List<PartyMember>(players.Select(p => new PartyMember(p)));
			Teams = new TeamManager(this);
		}

		internal void Add(TSPlayer player)
		{
			if( !Contains(player) )
			{
				var member = new PartyMember(player);
				partyMembers.Add(member);
			}

			//if(!Contains(player))
			//{
			//	var member = new PartyMember(player);
			//	partyMembers.Add(member);
			//	playerIndexToPartyIndex.Add(player.Index, partyMembers.Count-1);
			//}
		}

		internal void Remove(TSPlayer player)
		{
			var index = IndexOf(player);

			if(index>-1)
			{
				partyMembers.RemoveAt(index);
			}
		}

		public bool Contains(TSPlayer player)
		{
			var result = IndexOf(player) > -1;
			return result;
		}

		public bool Contains(string playerName)
		{
			var result = IndexOf(playerName) > -1;
			return result;
		}

		public int IndexOf(TSPlayer player)
		{
			var result = partyMembers.FindIndex(pm => pm.Player.Index == player.Index);
			return result;
			//if( !playerIndexToPartyIndex.TryGetValue(player.Index, out var result) )
			//	return -1;
			//else
			//	return result;
		}

		public int IndexOf(PartyMember member)
		{
			var result = partyMembers.FindIndex(pm => pm == member);
			return result;
			//return IndexOf(member.Player);
		}

		public int IndexOf(string playerName)
		{
			var result = partyMembers.FindIndex(pm => pm.Player.Name == playerName);
			return result;
		}

		public bool SetLeader(int index)
		{
			if( index < 1 )
				return false;//already leader, or invalid index

			if( index >= partyMembers.Count )
				return false;

			var currentLeader = Leader;
			var newLeader = partyMembers[index];
			partyMembers[index] = currentLeader;
			partyMembers[0] = newLeader;

			return true;
		}

		public bool SetLeader(PartyMember member)
		{
			var memberIndex = IndexOf(member);

			if( memberIndex == -1 )
				return false;

			return SetLeader(memberIndex);
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
