using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

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
                Remove(username);
            }
        }

        /// <summary>
        ///     Gets the session associated with the specified username, or creates it if it does not exist.
        /// </summary>
        /// <param name="username">The username, which must not be <c>null</c>.</param>
        /// <returns>The session associated with the username.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="username" /> is <c>null</c>.</exception>
        [NotNull]
        public Session GetOrCreate([NotNull] string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (!_sessions.TryGetValue(username, out var session))
            {
                var path = Path.Combine("quests", "sessions", $"{username}.json");
                if (File.Exists(path))
                {
                    session = JsonConvert.DeserializeObject<Session>(File.ReadAllText(path));
                }
                else
                {
                    session = new Session();
                    foreach (var questName in _config.DefaultQuestNames)
                    {
                        session.UnlockQuestName(questName);
                    }
                }
                _sessions[username] = session;
            }
            return session;
        }

        /// <summary>
        ///     Removes the session associated with the specified username.
        /// </summary>
        /// <param name="username">The username, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="username" /> is <c>null</c>.</exception>
        public void Remove([NotNull] string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (_sessions.TryGetValue(username, out var session))
            {
                var path = Path.Combine("quests", "sessions", $"{username}.json");
                File.WriteAllText(path, JsonConvert.SerializeObject(session, Formatting.Indented));
                session.Dispose();
                _sessions.Remove(username);
            }
        }
    }
}
