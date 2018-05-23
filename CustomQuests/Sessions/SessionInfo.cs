using System;
using System.Collections.Generic;
using CustomQuests.Quests;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CustomQuests.Sessions
{
    /// <summary>
    ///     Represents information about a quest.
    /// </summary>
    public sealed class SessionInfo
    {
        /// <summary>
        ///     Gets the unlocked quest names.
        /// </summary>
        [NotNull]
        public HashSet<string> UnlockedQuestNames { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets the completed quest names.
        /// </summary>
        [NotNull]
        public HashSet<string> CompletedQuestNames { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets or sets the current quest info.
        /// </summary>
        [CanBeNull]
        public QuestInfo CurrentQuestInfo { get; set; }

		//      /// <summary>
		//      ///     Gets or sets the current quest state.
		//      /// </summary>
		//      [CanBeNull]
		//public string CurrentQuestState { get; set; }

		//public Dictionary<string,SavePoint> QuestSavePoints { get; } = new Dictionary<string, SavePoint>();
		//public Dictionary<string, SavePoint> PartyLeaderSavePoints { get; } = new Dictionary<string, SavePoint>();
		//public Dictionary<string, QuestStatusManager> QuestSavePoints { get; } = new Dictionary<string, QuestStatusManager>();

		//[JsonIgnore]
		//public QuestStatusCollection QuestStatusManager { get; set; } = new QuestStatusCollection();
		
		/// <summary>
		///     Gets the number of attempts per quest.
		/// </summary>
		public Dictionary<string, int> QuestAttempts { get; } = new Dictionary<string, int>();
		
		/// <summary>
		///		Gets or sets a Dictionary containing time stamps of the first attempt at Quest.
		/// </summary>
		/// <remarks>This is used to determine quest reset times.</remarks>
		public Dictionary<string, DateTime> QuestFirstAttemptTimes { get; set; } = new Dictionary<string, DateTime>();

		//public SavePoint GetOrCreateSavePoint(string questName, bool isPartyLeader)
		//{
		//	var selectedSavePoints = isPartyLeader ? PartyLeaderSavePoints : QuestSavePoints; 

		//	if(!selectedSavePoints.TryGetValue(questName, out var savePoint))
		//	{
		//		savePoint = new SavePoint();
		//		selectedSavePoints[questName] = savePoint;
		//	}

		//	return savePoint;
		//}

		//public void RemoveSavePoint(string questName, bool isPartyLeader)
		//{
		//	var selectedSavePoints = isPartyLeader ? PartyLeaderSavePoints : QuestSavePoints;

		//	selectedSavePoints.Remove(questName);
		//}
		
		//public QuestStatusManager GetOrCreateSavePointManager(string questName)
		//{
		//	if( !QuestSavePoints.TryGetValue(questName, out var savePointManager) )
		//	{
		//		savePointManager = new QuestStatusManager();
		//		QuestSavePoints.Add(questName, savePointManager);
		//	}

		//	return savePointManager;
		//}

		//public SavePoint GetOrCreateSavePoint(string questName, bool isPartyLeader)
		//{
		//	var manager = getOrCreateSavePointManager(questName);

		//	manager.SetSavePoint(0,)

		//	return savePoint;
		//}

		//public void RemoveSavePoint(string questName, bool isPartyLeader)
		//{
		//	var manager = getOrCreateSavePointManager(questName);

		//	manager.Remove(questName);
		//}
	}
}
