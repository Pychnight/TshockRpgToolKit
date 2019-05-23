using Corruption.PluginSupport;
using CustomSkills.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Represents a logged in players current skill state. 
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class Session
	{
		internal static ConcurrentDictionary<string, Session> ActiveSessions { get; private set; } = new ConcurrentDictionary<string, Session>();
		
		/// <summary>
		/// Gets the CustomSkillsPlugin.Instance.SessionRepository
		/// </summary>
		internal static ISessionDatabase SessionRepository => CustomSkillsPlugin.Instance.SessionRepository;
		
		//Player can be wiped by the time we save(), so its best to store the name string.
		public string PlayerName { get; set; }

		//public TSPlayer Player { get; set; }

		/// <summary>
		/// Dictionary of all learned skills, and their status.
		/// </summary>
		[JsonProperty(Order = 0)]
		internal Dictionary<string, PlayerSkillInfo> PlayerSkillInfos { get; set; } = new Dictionary<string, PlayerSkillInfo>();

		//DO NOT PERSIST THIS... it should be regenerated on join/reload...
		/// <summary>
		/// Maps trigger words to skills.
		/// </summary>
		internal Dictionary<string,CustomSkillDefinition> TriggerWordsToSkillDefinitions { get; set; } = new Dictionary<string, CustomSkillDefinition>();
		
		public Session() { }

		public Session(TSPlayer player)
		{
			PlayerName = player.Name;
			//Player = player;
		}

		public static Session GetOrCreateSession(TSPlayer player)
		{
			if(!ActiveSessions.TryGetValue(player.Name, out var playerSession))
			{
				//session is not active, try to get it from the db
				playerSession = SessionRepository.Load(player.Name);

				if(playerSession==null)
				{
					//otherwise, create new
					playerSession = new Session(player);
				}
				else
				{
					//update all trigger words
					foreach(var skillName in playerSession.PlayerSkillInfos.Keys)
						playerSession.UpdateTriggerWords(skillName);
				}

				ActiveSessions.TryAdd(player.Name, playerSession);
			}

			return playerSession;
		}
				
		public static void SaveAll()
		{
			foreach(var session in ActiveSessions.Values)
				session?.Save();
		}

		public void Save()
		{
			try
			{
				SessionRepository.Save(PlayerName, this);
			}
			catch(Exception ex)
			{
				CustomSkillsPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
			}
		}

		public bool HasLearned(string skillName) => PlayerSkillInfos.ContainsKey(skillName);

		public bool LearnSkill(string skillName)
		{
			if(!HasLearned(skillName))
			{
				PlayerSkillInfos.Add(skillName, new PlayerSkillInfo());
				UpdateTriggerWords(skillName);
				return true;
			}

			return false;
		}

		public bool IsSkillReady(string skillName)
		{
			var definition = CustomSkillsPlugin.Instance.CustomSkillDefinitionLoader.TryGetDefinition(skillName);

			if(definition!=null)
			{
				if(PlayerSkillInfos.TryGetValue(skillName, out var skillInfo))
				{
					var levelDef = definition.Levels[skillInfo.CurrentLevel];

					return (DateTime.Now - skillInfo.CooldownStartTime) > levelDef.CastingCooldown;
				}
			}

			return false;
		}

		private void UpdateTriggerWords(string skillName)
		{
			if(CustomSkillsPlugin.Instance.CustomSkillDefinitionLoader.TriggeredDefinitions.TryGetValue(skillName, out var skill))
			{
				foreach(var word in skill.TriggerWords)
					TriggerWordsToSkillDefinitions[word] = skill;
			}
		}
	}
}
