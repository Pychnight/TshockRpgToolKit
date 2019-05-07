using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Represents a logged in players current skill state. 
	/// </summary>
	public class Session
	{
		internal static ConcurrentDictionary<string, Session> ActiveSessions { get; private set; } = new ConcurrentDictionary<string, Session>();
		
		public TSPlayer Player { get; set; }
		internal Dictionary<string, PlayerSkillInfo> PlayerSkillInfos { get; set; } = new Dictionary<string, PlayerSkillInfo>();

		//DO NOT PERSIST THIS... it should be regenerated on join/reload...
		internal Dictionary<string,CustomSkillDefinition> TriggerWordsToSkillDefinitions { get; set; } = new Dictionary<string, CustomSkillDefinition>();
		//internal HashSet<string> ActiveTriggerWords { get; set; } = new HashSet<string>();

		public Session(TSPlayer player)
		{
			Player = player;
			LearnSkill("WindBreaker");
		}

		public static Session GetOrCreateSession(TSPlayer player)
		{
			if(!ActiveSessions.TryGetValue(player.Name, out var playerSession))
			{
				playerSession = new Session(player);
				ActiveSessions.TryAdd(player.Name, playerSession);
			}

			return playerSession;
		}

		public static void OnReload()
		{
			//nop for now...but may need to reset/clear some per session data, unsure yet.
			//ActiveSessions.Values.ForEach(s => s.PlayerSkillInfos.OnReload());
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
