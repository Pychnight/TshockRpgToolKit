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
        ///     Gets a read-only view of the unlocked quest names.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public IEnumerable<string> UnlockedQuestNames => SessionInfo.UnlockedQuestNames;

        /// <summary>
        ///     Gets a read-only view of the completed quest names.
        /// </summary>
        [ItemNotNull]
        [NotNull]
        public IEnumerable<string> CompletedQuestNames => SessionInfo.CompletedQuestNames;
		
        /// <summary>
        ///     Gets or sets the current quest.
        /// </summary>
        public Quest CurrentQuest
        {
            get => _currentQuest;
            set
            {
                _currentQuest = value;
                SessionInfo.CurrentQuestInfo = CurrentQuestInfo;
            }
        }

		/// <summary>
		///     Gets the current quest info.
		/// </summary>
		public QuestInfo CurrentQuestInfo => CurrentQuest?.QuestInfo;

        /// <summary>
        ///     Gets the current quest name.
        /// </summary>
        public string CurrentQuestName => CurrentQuest?.QuestInfo.Name;

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
		public Dictionary<string, QuestStatusCollection> QuestProgress = new Dictionary<string, QuestStatusCollection>();
		
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

		public bool CanRepeatQuest(QuestInfo questInfo)
		{
			if( questInfo == null )
			{
				throw new ArgumentNullException(nameof(questInfo));
			}

			//can repeat quest?
			if( questInfo.MaxRepeats > -1 )
			{
				if( SessionInfo.QuestAttempts.TryGetValue(questInfo.Name, out var attempts) )
				{
					if( attempts > questInfo.MaxRepeats )
					{
						if( SessionInfo.QuestFirstAttemptTimes.TryGetValue(questInfo.Name, out var firstAttemptTime) )
						{
							if( DateTime.Now - firstAttemptTime < questInfo.RepeatResetInterval )
							{
								return false;//we've hit max allowed attempts, but timer hasnt reset
							}
							//else
							//{
							//	SessionInfo.RepeatedQuestNames.Remove(questInfo.Name);
							//}
						}
					}
				}
			}

			return true;
		}

		public bool CanAcceptQuest(QuestInfo questInfo)
		{
			if( questInfo == null )
			{
				throw new ArgumentNullException(nameof(questInfo));
			}

			if( CustomQuestsPlugin.Instance.QuestLoader.IsQuestInvalid(questInfo.Name) )
				return false;

			if( !CanRepeatQuest(questInfo) )
				return false;
						
			var result = questInfo.RequiredRegionName == null ||
					TShock.Regions.InAreaRegion(_player.TileX, _player.TileY)
						.Any(r => r.Name == questInfo.RequiredRegionName);

			return result;
		}

        /// <summary>
        ///     Determines whether the session can see the specified quest.
        /// </summary>
        /// <param name="questInfo">The quest information, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the session can see the quest; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="questInfo" /> is <c>null</c>.</exception>
        public bool CanSeeQuest(QuestInfo questInfo)
        {
			//         if (questInfo == null)
			//         {
			//             throw new ArgumentNullException(nameof(questInfo));
			//         }

			//if( CustomQuestsPlugin.Instance.QuestLoader.IsQuestInvalid(questInfo.Name) )
			//	return false;

			//         var result = questInfo.RequiredRegionName == null ||
			//                TShock.Regions.InAreaRegion(_player.TileX, _player.TileY)
			//                    .Any(r => r.Name == questInfo.RequiredRegionName);

			//return result;

			return CanAcceptQuest(questInfo);
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

		public void RejoinQuest()
		{
			var quest = CustomQuestsPlugin.Instance.QuestRunner.GetRejoinableQuest(_player);

			if(quest!=null)
			{
				_player.SendInfoMessage($"Rejoin the '{quest.party.Name}' party, on quest '{quest.QuestInfo.FriendlyName}'?");
				_player.SendInfoMessage("Use /accept or /decline ");
				_player.AwaitingResponse.Add("accept", args2 =>
				{
					if( Party != null )
					{
						_player.SendErrorMessage("You cannot rejoin the quest if you are already in a party.");
						_player.AwaitingResponse.Remove("decline");
						return;
					}

					if(quest.MainQuestTask.Status!=TaskStatus.Running)
					{
						_player.SendErrorMessage("Sorry, but the quest is over now.");
						_player.AwaitingResponse.Remove("decline");
						return;
					}

					Party = quest.party;
					CurrentQuest = quest;

					//Debug.Print("TEAM: PartyInvite()... link party to new player!");
					_player.TPlayer.team = 1;
					quest.party.SendData(PacketTypes.PlayerTeam, "", _player.Index);
					_player.TPlayer.team = 0;

					Party.SendInfoMessage($"{_player.Name} has rejoined the party.");
					Party.Add(_player);

					foreach( var member in Party )
					{
						//Debug.Print("TEAM: PartyInvite()... link new player to existing party member!");
						member.Player.TPlayer.team = 1;
						_player.SendData(PacketTypes.PlayerTeam, "", member.Index);
						member.Player.TPlayer.team = 0;
					}

					_player.SendSuccessMessage($"Rejoined the '{Party.Name}' party.");
					_player.AwaitingResponse.Remove("decline");
				});
				_player.AwaitingResponse.Add("decline", args2 =>
				{
					_player.SendSuccessMessage("The party will continue on without you.");
					_player.AwaitingResponse.Remove("accept");
				});
			}
		}

		/// <summary>
		/// Clears all quest data.
		/// </summary>
		public void Clear()
		{
			var si = SessionInfo;

			si.UnlockedQuestNames.Clear();
			si.CompletedQuestNames.Clear();
			si.QuestAttempts.Clear();
			si.QuestFirstAttemptTimes.Clear();

			this.QuestProgress.Clear();

			foreach( var name in CustomQuestsPlugin.Instance._config.DefaultQuestNames )
				si.UnlockedQuestNames.Add(name);

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

		public void LoadQuest(QuestInfo questInfo)
		{
			if( questInfo == null )
				throw new ArgumentNullException(nameof(questInfo));

			//ensure there is a party set, even if a solo player.
			Party = Party ?? new Party(_player.Name, _player);
			Party.OnPreStart(questInfo);
						
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

            SessionInfo.UnlockedQuestNames.Remove(name);
            SessionInfo.CompletedQuestNames.Remove(name);
			this.QuestProgress.Remove(name);
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

            SessionInfo.UnlockedQuestNames.Add(name);
        }

        /// <summary>
        ///     Checks to see if the current quest has ended, and sets closing values if so.
        /// </summary>
        public void CheckQuestCompleted()
        {
			if (CurrentQuest == null)
				return;
            
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
			
            if (CurrentQuest.IsEnded)
            {
				//remove save point
				Debug.Print("UpdateQuest() Should remove savepoints here!");
				//SessionInfo.RemoveSavePoint(this.CurrentQuestInfo.Name, isPartyLeader);

				QuestProgress.Remove(CurrentQuestName);
												
				if (CurrentQuest.IsSuccessful)
                {
                    _player.SendSuccessMessage("Quest completed!");
                }
                else
                {
					_player.SendErrorMessage("Quest failed.");
                }

                Dispose();
            }
        }

		public void CheckRepeatInterval()
		{
			if( CurrentQuest != null )
				return;

			var removalList = new List<string>();
			var now = DateTime.Now;

			foreach(var kvp in SessionInfo.QuestFirstAttemptTimes)
			{
				var questInfo = CustomQuestsPlugin.Instance.QuestLoader[kvp.Key];

				if( questInfo == null )
					continue;

				if( now - kvp.Value >= questInfo.RepeatResetInterval )
				{
					removalList.Add(questInfo.Name);
					Debug.Print($"Resetting {this._player.Name}'s attempts for {questInfo.Name}.");
				}
			}

			foreach( var name in removalList )
			{
				SessionInfo.QuestAttempts.Remove(name);
				SessionInfo.QuestFirstAttemptTimes.Remove(name);
			}
		}
    }
}
