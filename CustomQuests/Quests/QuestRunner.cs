using Corruption.PluginSupport;
using CustomQuests.Sessions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomQuests.Quests
{
	public class QuestRunner
	{
		QuestLoader questLoader;
		ConcurrentDictionary<string, Quest> quests;

		internal QuestRunner(QuestLoader questLoader)
		{
			this.questLoader = questLoader;
			quests = new ConcurrentDictionary<string, Quest>();
		}

		public bool Start(QuestInfo info, Party party, Session session )//we have to pass the session in for now, sadly. 
		{
			if( quests.ContainsKey(party.Name) )
				throw new ArgumentException($"Party is already in a Quest.", nameof(party));

			var newQuest = questLoader.CreateInstance(info, party);

			if( newQuest != null )
			{
				if(quests.TryAdd(party.Name, newQuest))
				{
					session.CurrentQuest = newQuest;
					newQuest.Run();
					return true;
				}
			}
			
			return false;
		}

		public void Abort(Party party)
		{
			if( quests.TryGetValue(party.Name, out var quest) )
			{
				var task = quest.MainQuestTask;
				
				if( task.Status == TaskStatus.Running ||
					task.Status == TaskStatus.WaitingToRun ||
					task.Status == TaskStatus.WaitingForChildrenToComplete )
				{
					quest.OnAbort();
				}
			}
		}

		public void Update()
		{
			var questsToRemove = new List<string>();

			foreach( var quest in quests.Values )
			{
				switch( quest.MainQuestTask.Status )
				{
					case TaskStatus.Running:
						quest.Update();
						break;

					case TaskStatus.RanToCompletion:
						questsToRemove.Add(quest.party.Name);

						if( !quest.CalledComplete )
						{
							CustomQuestsPlugin.Instance.LogPrint($"'{quest.QuestInfo.Name}' MainQuestTask finished execution, but no call to Complete() was made. ( Did you forget to wait on a Task? )", TraceLevel.Error);
						}

						break;

					case TaskStatus.Canceled:
						questsToRemove.Add(quest.party.Name);
						break;

					case TaskStatus.Faulted:
						questsToRemove.Add(quest.party.Name);
						break;
				}
			}

			//remove dead quests...
			foreach( var name in questsToRemove )
			{
				if(quests.TryRemove(name, out var removedQuest))
				{
					removedQuest.Dispose();
				}
			}
		}

		//public void OnReload()
		//{

		//}

		internal Quest GetRejoinableQuest(TSPlayer player)
		{
			var rejoinableQuests = quests.Values.Where(v => v.MainQuestTask.Status == TaskStatus.Running && v.QuestInfo.AllowRejoin);
			var firstRejoinableQuest = rejoinableQuests.FirstOrDefault(q => q.RejoinablePlayers.Contains(player.Name));

			return firstRejoinableQuest;
		}
	}
}
