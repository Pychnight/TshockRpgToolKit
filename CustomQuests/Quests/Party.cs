using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TShockAPI;

namespace CustomQuests.Quests
{
	public class Party : IEnumerable<PartyMember>
	{
		List<PartyMember> partyMembers;
		//Dictionary<int, int> playerIndexToPartyIndex;

		public string Name { get; private set; }
		public int Count => partyMembers.Count;
		public PartyMember this[int index] => partyMembers[index];
		public PartyMember Leader => partyMembers[0];
		public TeamManager Teams { get; private set; }

		internal Party(string name, params TSPlayer[] players) : this(name, players.AsEnumerable())
		{
		}

		internal Party(string name, IEnumerable<TSPlayer> players)
		{
			Name = name;
			//playerIndexToPartyIndex = new Dictionary<int, int>();
			partyMembers = new List<PartyMember>(players.Count());

			//use Add() so that IsValidMember gets set( along with any future housekeeping ) 
			foreach (var p in players)
				Add(p);

			Teams = new TeamManager(this);
		}

		internal void Add(TSPlayer player)
		{
			if (!Contains(player))
			{
				var member = new PartyMember(player);
				partyMembers.Add(member);
				member.IsValidMember = true;
			}
		}

		internal void Remove(TSPlayer player)//, bool notifyLeaderChange = false)
		{
			var index = IndexOf(player);

			if (index > -1)
			{
				var member = partyMembers[index];
				partyMembers.RemoveAt(index);
				member.IsValidMember = false;

				//if(index==0 && Count > 0 && notifyLeaderChange)
				//{
				//	partyMembers.SendInfoMessage($"{Leader.Name} is the new party leader.");
				//}
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
			if (index < 1)
				return false;//already leader, or invalid index

			if (index >= partyMembers.Count)
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

			if (memberIndex == -1)
				return false;

			return SetLeader(memberIndex);
		}

		[DebuggerStepThrough]
		public IEnumerator<PartyMember> GetEnumerator() => partyMembers.GetEnumerator();

		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal void OnPreStart(QuestInfo questInfo)
		{
			foreach (var m in this)
			{
				var session = CustomQuestsPlugin.Instance.GetSession(m);

				session.IsAborting = false;
				session.HasAborted = false;

				session.AddQuestAttempt(questInfo);

				//try to restore previous status
				if (!session.QuestProgress.TryGetValue(questInfo.Name, out var statuses))
				{
					Debug.Print("Using PartyMembers quest status.");
					//copy members quest status to session.
					statuses = m.QuestStatuses;
					session.QuestProgress.Add(questInfo.Name, statuses);
				}
				else
				{
					Debug.Print("Using Session's quest status.");
					//copy sessions quest status to member.
					m.QuestStatuses = statuses;
				}
			}
		}
	}
}
