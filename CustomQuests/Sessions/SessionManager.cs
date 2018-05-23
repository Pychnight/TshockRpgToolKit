#define SQLITE_SESSION_REPOSITORY

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TShockAPI;
using TerrariaApi.Server;
using CustomQuests.Quests;
using Corruption.PluginSupport;
using System.Diagnostics;

namespace CustomQuests.Sessions
{
    /// <summary>
    ///     Manages sessions.
    /// </summary>
    public sealed class SessionManager : IDisposable
    {
		private readonly Config _config;
        private readonly Dictionary<string, Session> _sessions = new Dictionary<string, Session>();
        internal readonly SessionRepository sessionRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionManager" /> class with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config" /> is <c>null</c>.</exception>
        public SessionManager([NotNull] Config config, TerrariaPlugin plugin)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

#if SQLITE_SESSION_REPOSITORY
			sessionRepository = new SqliteSessionRepository(Path.Combine("quests","sessions.db"),plugin);
#else
			sessionRepository = new FileSessionRepository(Path.Combine("quests","sessions"),plugin);
#endif
		}

        /// <summary>
        ///     Disposes the session manager.
        /// </summary>
        public void Dispose()
        {
            foreach (var username in _sessions.Keys.ToList())
            {
                var session = _sessions[username];
				//var path = Path.Combine("quests", "sessions", $"{username}.json");
				//File.WriteAllText(path, JsonConvert.SerializeObject(session, Formatting.Indented));
				sessionRepository.Save(session.SessionInfo, username);

                session.Dispose();
                _sessions.Remove(username);
            }

			sessionRepository.Dispose();
        }

        /// <summary>
        ///     Gets the session associated with the specified player, or creates it if it does not exist.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <returns>The session associated with the player.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
        [NotNull]
        public Session GetOrCreate([NotNull] TSPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var username = player.User?.Name ?? player.Name;
            if (!_sessions.TryGetValue(username, out var session))
            {
				var sessionInfo = sessionRepository.Load(username);

                if (sessionInfo == null)
                {
                    sessionInfo = new SessionInfo();
                    foreach (var questName in _config.DefaultQuestNames)
                    {
                        sessionInfo.UnlockedQuestNames.Add(questName);
                    }
                }

                session = new Session(player, sessionInfo);
                if (session.CurrentQuestInfo != null)
                {
					//throw new NotImplementedException("Rejoining quests is currently disabled.");
					//session.LoadQuest(session.CurrentQuestInfo);

					Debug.Print("DEBUG: Rejoining quests is currently disabled.");
					//session.LoadQuestX(session.CurrentQuestInfo);
				}

                _sessions[username] = session;
            }
            else
            {
                foreach (var questName in _config.DefaultQuestNames)
                {
                    var sessionInfo = session.SessionInfo;
                    if (!sessionInfo.CompletedQuestNames.Contains(questName))
                    {
                        sessionInfo.UnlockedQuestNames.Add(questName);
                    }
                }
            }
            return session;
        }

        /// <summary>
        ///     Removes the session associated with the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
        public void Remove([NotNull] TSPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var username = player.User?.Name ?? player.Name;
            if (_sessions.TryGetValue(username, out var session))
            {
                //var path = Path.Combine("quests", "sessions", $"{username}.json");
                //File.WriteAllText(path, JsonConvert.SerializeObject(session.SessionInfo, Formatting.Indented));

				sessionRepository.Save(session.SessionInfo, username);

                session.Dispose();
                _sessions.Remove(username);
            }
        }

		public void OnReload()
		{
			foreach(var session in _sessions.Values)
			{
				//var questName = s.CurrentQuestName;
				var player = session._player;

				var quest = session.CurrentQuest;
				if (quest != null)
				{
					session.IsAborting = true;
					
					try
					{
						var bquest = session.CurrentQuest;
						bquest.Abort();
					}
					catch( Exception ex )
					{
						CustomQuestsPlugin.Instance.LogPrint(ex.ToString());
					}

					session.HasAborted = true;
					player.SendSuccessMessage("Server reload, quest aborted.");
				}
			}
		}
	}
}
