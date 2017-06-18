using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TShockAPI;

namespace CustomQuests.Sessions
{
    /// <summary>
    ///     Manages sessions.
    /// </summary>
    public sealed class SessionManager : IDisposable
    {
        private readonly Config _config;
        private readonly Dictionary<string, Session> _sessions = new Dictionary<string, Session>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionManager" /> class with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config" /> is <c>null</c>.</exception>
        public SessionManager([NotNull] Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        ///     Disposes the session manager.
        /// </summary>
        public void Dispose()
        {
            foreach (var username in _sessions.Keys.ToList())
            {
                var session = _sessions[username];
                var path = Path.Combine("quests", "sessions", $"{username}.json");
                File.WriteAllText(path, JsonConvert.SerializeObject(session, Formatting.Indented));
                session.Dispose();
                _sessions.Remove(username);
            }
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
                var path = Path.Combine("quests", "sessions", $"{username}.json");
                if (File.Exists(path))
                {
                    var sessionInfo = JsonConvert.DeserializeObject<SessionInfo>(File.ReadAllText(path));
                    session = new Session(player, sessionInfo);
                }
                else
                {
                    var sessionInfo = new SessionInfo();
                    foreach (var questName in _config.DefaultQuestNames)
                    {
                        sessionInfo.AvailableQuestNames.Add(questName);
                    }
                    session = new Session(player, sessionInfo);
                }

                if (session.CurrentQuestName != null)
                {
                    session.LoadQuest(session.CurrentQuestName);
                }

                _sessions[username] = session;
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
                var path = Path.Combine("quests", "sessions", $"{username}.json");
                File.WriteAllText(path, JsonConvert.SerializeObject(session.SessionInfo, Formatting.Indented));
                session.Dispose();
                _sessions.Remove(username);
            }
        }
    }
}
