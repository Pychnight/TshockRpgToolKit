using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomSkills
{
	public class SessionManager
	{
		ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();

		public Session GetOrCreateSession(TSPlayer player)
		{
			if(!sessions.TryGetValue(player.Name, out var playerSession))
			{
				playerSession = new Session(player);
				sessions.TryAdd(player.Name, playerSession);
			}

			return playerSession;
		}
	}

	public class Session
	{
		public TSPlayer Player { get; internal set; }
		public HashSet<string> SkillsLearned { get; set; } = new HashSet<string>();
		public Dictionary<string, LevelInfo> SkillLevelInfo { get; set; } = new Dictionary<string, LevelInfo>(); 

		public Session(TSPlayer player)
		{
			Player = player;
		}

		public class LevelInfo
		{
			public int CurrentLevel { get; set; }
			public int CurrentUses { get; set; }
		}
	}
}
