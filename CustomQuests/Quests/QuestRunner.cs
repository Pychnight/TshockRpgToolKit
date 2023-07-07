using Corruption.PluginSupport;
using CustomQuests.Sessions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
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

		public bool Start(QuestInfo info, Party party, Session session)//we have to pass the session in for now, sadly. 
		{
			if (quests.ContainsKey(party.Name))
				throw new ArgumentException($"Party is already in a Quest.", nameof(party));

			var newQuest = questLoader.CreateInstance(info, party);

			if (newQuest != null)
			{
				if (quests.TryAdd(party.Name, newQuest))
				{
					session.CurrentQuest = newQuest;
					newQuest.Run();
					return true;
				}
			}

			return false;
		}

		//this never got integrated...
		//public void Abort(Party party)
		//{
		//	if( quests.TryGetValue(party.Name, out var quest) )
		//	{
		//		var task = quest.MainQuestTask;

		//		if( task.Status == TaskStatus.Running ||
		//			task.Status == TaskStatus.WaitingToRun ||
		//			task.Status == TaskStatus.WaitingForChildrenToComplete )
		//		{
		//			quest.OnAbort();
		//		}
		//	}
		//}

		public void Update()
		{
			var currentQuests = quests.Values.ToArray();

			for (var i = 0; i < currentQuests.Length; i++)
			{
				var quest = currentQuests[i];

				//HACK work around against a race condition?? MainQuest isn't always set by this point.
				if (quest.MainQuestTask == null)
					continue;

				switch (quest.MainQuestTask.Status)
				{
					case TaskStatus.WaitingForChildrenToComplete:
						quest.Update();
						break;

					case TaskStatus.Running:
						quest.Update();
						break;

					case TaskStatus.RanToCompletion:
						RemoveQuest(quest.party.Name);

						if (!quest.CalledComplete)
							CustomQuestsPlugin.Instance.LogPrint($"'{quest.QuestInfo.Name}' MainQuestTask finished execution, but no call to Complete() was made. ( Did you forget to wait on a Task? )", TraceLevel.Error);

						break;

					case TaskStatus.Canceled:
						RemoveQuest(quest.party.Name);
						break;

					case TaskStatus.Faulted:
						RemoveQuest(quest.party.Name);

						if (quest.MainQuestTask?.Exception != null)
						{
							CustomQuestsPlugin.Instance.LogPrint($"'{quest.QuestInfo.Name}' MainQuestTask terminated due to errors or cancellation.", TraceLevel.Warning);

							foreach (var ex in quest.MainQuestTask.Exception.InnerExceptions)
								CustomQuestsPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Warning);

							//quest.OnAbort("Quest aborted due to error. Please let the server admin know.");
						}

						quest.OnAbort();

						break;
				}
			}
		}

		private void RemoveQuest(string partyName)
		{
			if (quests.TryRemove(partyName, out var removedQuest))
			{
				removedQuest.Dispose();
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
