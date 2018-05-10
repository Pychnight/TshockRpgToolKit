using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomQuests.Quests;
using JetBrains.Annotations;
using TShockAPI;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Xna.Framework;
using System.Reflection;
using CustomQuests.Scripting;
using Corruption.PluginSupport;

namespace CustomQuests.Sessions
{
    /// <summary>
    ///     Holds session information.
    /// </summary>
    public sealed class Session : IDisposable
    {
		internal readonly TSPlayer _player;//made internal, as a quick fix for SessionManager needing the player in OnReload().
		private Quest _currentQuest;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Session" /> class with the specified player and session information.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <param name="sessionInfo">The session information, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="player" /> or <paramref name="sessionInfo" /> is <c>null</c>.
        /// </exception>
        public Session([NotNull] TSPlayer player, SessionInfo sessionInfo)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            SessionInfo = sessionInfo ?? throw new ArgumentNullException(nameof(sessionInfo));
        }

        /// <summary>
        ///     Gets a read-only view of the available quest names.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public IEnumerable<string> AvailableQuestNames => SessionInfo.AvailableQuestNames;

        /// <summary>
        ///     Gets a read-only view of the completed quest names.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public IEnumerable<string> CompletedQuestNames => SessionInfo.CompletedQuestNames;
			
        /// <summary>
        ///     Gets or sets the current quest.
        /// </summary>
        [CanBeNull]
        public Quest CurrentQuest
        {
            get => _currentQuest;
            set
            {
                _currentQuest = value;
                CurrentQuestInfo = _currentQuest?.QuestInfo;
                SessionInfo.CurrentQuestInfo = CurrentQuestInfo;
            }
        }

        /// <summary>
        ///     Gets the current quest info.
        /// </summary>
        [CanBeNull]
        public QuestInfo CurrentQuestInfo { get; internal set; }

        /// <summary>
        ///     Gets the current quest name.
        /// </summary>
        [CanBeNull]
        public string CurrentQuestName => CurrentQuestInfo?.Name;

        /// <summary>
        ///     Gets or sets a value indicating whether the session is aborting the quest.
        /// </summary>
        public bool IsAborting { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the session has aborted the quest.
        /// </summary>
        public bool HasAborted { get; set; }

        /// <summary>
        ///     Gets or sets the party.
        /// </summary>
        public Party Party { get; set; }

		//public QuestStatusCollection QuestStatusManager => SessionInfo.QuestStatusManager;
		
        /// <summary>
        ///     Gets the session information.
        /// </summary>
        public SessionInfo SessionInfo { get; }

        /// <summary>
        ///     Disposes the session.
        /// </summary>
        public void Dispose()
        {
            //CurrentQuest?.Dispose();
            CurrentQuest = null;
        }

        /// <summary>
        ///     Determines whether the session can see the specified quest.
        /// </summary>
        /// <param name="questInfo">The quest information, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the session can see the quest; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="questInfo" /> is <c>null</c>.</exception>
        public bool CanSeeQuest([NotNull] QuestInfo questInfo)
        {
            if (questInfo == null)
            {
                throw new ArgumentNullException(nameof(questInfo));
            }

			if( CustomQuestsPlugin.Instance.QuestLoader.IsQuestInvalid(questInfo.Name) )
				return false;

            var result = questInfo.RequiredRegionName == null ||
                   TShock.Regions.InAreaRegion(_player.TileX, _player.TileY)
                       .Any(r => r.Name == questInfo.RequiredRegionName);

			return result;
        }

		///// <summary>
		/////     Gets the quest state. This can be used in quest scripts to restore from a save point.
		///// </summary>
		///// <returns>The state.</returns>
		//[UsedImplicitly]
		//public string GetQuestState()
		//{
		//	//Debug.Print($"GetQuestState: {SessionInfo.CurrentQuestState}");
		//	//return SessionInfo.CurrentQuestState;

		//	return GetSavePoint(); 
		//}

		///// <summary>
		/////     Sets the quest state. This can be used in quest scripts to mark a specific point in the quest that has been achieved.
		///// </summary>
		///// <param name="state">The state.</param>
		//[UsedImplicitly]
		//public void SetQuestState([CanBeNull] string state)
		//{
		//	//SessionInfo.CurrentQuestState = state;
		//	//Debug.Print($"SetQuestState: {state}");
		//	SetSavePoint(state);
		//}

		///// <summary>
		/////     Gets the quest state. This can be used in quest scripts to restore from a save point.
		///// </summary>
		///// <returns>The state.</returns>
		//[UsedImplicitly]
		//public string GetSavePoint()
		//{
		//	Debug.Print($"GetSavePoint:");

		//	var isPartyLeader = _player == Party.Leader.Player;
		//	var questName = SessionInfo.CurrentQuestInfo.Name;

		//	Debug.Print("GetSavePoint() SAVEPOINT!");
		//	//var savePoint = SessionInfo.GetOrCreateSavePoint(questName,isPartyLeader);
		//	//return savePoint.SaveData;

		//	return null;
		//}

		///// <summary>
		/////     Sets the quest state. This can be used in quest scripts to mark a specific point in the quest that has been achieved.
		///// </summary>
		///// <param name="state">The state.</param>
		//[UsedImplicitly]
		//public void SetSavePoint([CanBeNull] string state)
		//{
		//	Debug.Print($"SetSavePoint:");

		//	if(SessionInfo.CurrentQuestInfo!=null)
		//	{
		//		var isPartyLeader = _player == Party.Leader.Player;
		//		var questName = SessionInfo.CurrentQuestInfo.Name;

		//		Debug.Print("SetSavePoint() SAVEPOINT!");
		//		//var savePoint = SessionInfo?.GetOrCreateSavePoint(questName,isPartyLeader);
		//		//savePoint.PartyName = Party.Name;
		//		//savePoint.SaveData = state;
		//	}
		//}

		/// <summary>
		/// Clears all quest data.
		/// </summary>
		public void Clear()
		{
			var si = SessionInfo;

			si.AvailableQuestNames.Clear();
			si.CompletedQuestNames.Clear();
			si.RepeatedQuestNames.Clear();

			si.QuestStatusManager.Clear();

			foreach( var name in CustomQuestsPlugin.Instance._config.DefaultQuestNames )
				si.AvailableQuestNames.Add(name);

			CustomQuestsPlugin.Instance._sessionManager.sessionRepository.Save(si, _player.Name);
		}

		/// <summary>
		///     Loads the quest with the specified info.
		/// </summary>
		/// <param name="questInfo">The quest info, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="questInfo" /> is <c>null</c>.</exception>
		//public void LoadQuest(QuestInfo questInfo)
  //      {
  //          if(questInfo == null)
		//		throw new ArgumentNullException(nameof(questInfo));

		//	//ensure there is a party set, even if a solo player.
		//	Party = Party ?? new Party(_player.Name, _player);
			
		//	if(!string.IsNullOrWhiteSpace(questInfo.ScriptPath))
		//	{
		//		var scriptPath = Path.Combine("quests", questInfo.ScriptPath ?? $"{questInfo.Name}.boo");
		//		var scriptAssembly = CustomQuestsPlugin.Instance.ScriptAssemblyManager.GetOrCompile(scriptPath);

		//		if(scriptAssembly!=null)
		//		{
		//			var questType = scriptAssembly.DefinedTypes.Where(dt => dt.BaseType == typeof(Quest))
		//													.Select(dt => dt.AsType())
		//													.FirstOrDefault();

		//			var quest = (Quest)Activator.CreateInstance(questType);
					
		//			//set these before, or various quest specific functions will get null ref's from within the quest.
		//			quest.QuestInfo = questInfo;
		//			quest.party = Party;
		//			CurrentQuest = quest;
		//			CurrentQuestInfo = questInfo;
					
		//			quest.Run();
		//		}
		//		else
		//		{
		//			CustomQuestsPlugin.Instance.LogPrint($"Cannot load quest '{questInfo.Name}', no assembly exists. ( Did compilation fail? ) ", TraceLevel.Error);
		//			CustomQuestsPlugin.Instance.QuestLoader.InvalidQuests.Add(questInfo.Name);
		//		}
		//	}
		//}

		public void LoadQuestX(QuestInfo questInfo)
		{
			if( questInfo == null )
				throw new ArgumentNullException(nameof(questInfo));

			//ensure there is a party set, even if a solo player.
			Party = Party ?? new Party(_player.Name, _player);

			var result = CustomQuestsPlugin.Instance.QuestRunner.Start(questInfo, Party, this);

			if(!result)
			{
				CustomQuestsPlugin.Instance.LogPrint($"Cannot load quest '{questInfo.Name}', no assembly exists. ( Did compilation fail? ) ", TraceLevel.Error);
				CustomQuestsPlugin.Instance.QuestLoader.InvalidQuests.Add(questInfo.Name);
			}
		}

		/// <summary>
		///     Revokes the quest with the specified name.
		/// </summary>
		/// <param name="name">The quest name, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		public void RevokeQuest([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            SessionInfo.AvailableQuestNames.Remove(name);
            SessionInfo.CompletedQuestNames.Remove(name);
		}
		
        /// <summary>
        ///     Unlocks the quest with the specified name.
        /// </summary>
        /// <param name="name">The quest name, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        public void UnlockQuest([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            SessionInfo.AvailableQuestNames.Add(name);
        }

        /// <summary>
        ///     Updates the current quest.
        /// </summary>
        public void UpdateQuest()
        {
			//throw new Exception("Refactor");

            if (CurrentQuest == null)
            {
                return;
            }

            if (IsAborting)
            {
                if (HasAborted)
                {
                    IsAborting = false;
                    HasAborted = false;
                    Dispose();
                    //SetQuestState(null);
                }
                return;
            }

			//var isPartyLeader = _player == Party.Leader.Player;

			//if( Party == null || isPartyLeader )
			//{
			//	CurrentQuest.Update();
			//}
			
            if (CurrentQuest.IsEnded)
            {
				//remove save point
				Debug.Print("UpdateQuest() Should remove savepoints here!");
				//SessionInfo.RemoveSavePoint(this.CurrentQuestInfo.Name, isPartyLeader);
				
				if (CurrentQuest.IsSuccessful)
                {
                    var repeatedQuests = SessionInfo.RepeatedQuestNames;
                    
                    if (CurrentQuestInfo.MaxRepeats >= 0)
                    {
                        if (repeatedQuests.TryGetValue(CurrentQuestName, out var repeats))
                        {
                            repeatedQuests[CurrentQuestName] = repeats + 1;
                        }
                        else
                        {
                            repeatedQuests[CurrentQuestName] = 1;
                        }
                        if (repeatedQuests[CurrentQuestName] > CurrentQuestInfo.MaxRepeats)
                        {
                            SessionInfo.AvailableQuestNames.Remove(CurrentQuestName);
                            SessionInfo.CompletedQuestNames.Add(CurrentQuestName);
                            repeatedQuests.Remove(CurrentQuestName);
                        }
                    }
                    _player.SendSuccessMessage("Quest completed!");
                }
                else
                {
                    _player.SendErrorMessage("Quest failed.");
                }

                Dispose();
            }
        }
    }
}
