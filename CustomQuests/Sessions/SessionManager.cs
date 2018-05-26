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
using CustomQuests.Sessions;
using CustomQuests.Database;
using CustomQuests.Configuration;

namespace CustomQuests.Sessions
{
	/// <summary>
	///     Manages sessions.
	/// </summary>
	public sealed class SessionManager : IDisposable
	{
		private readonly Config _config;
		private readonly Dictionary<string, Session> activeSessions;
		internal IDatabase database;

		/// <summary>
		///     Initializes a new instance of the <see cref="SessionManager" /> class with the specified configuration.
		/// </summary>
		/// <param name="config">The configuration, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="config" /> is <c>null</c>.</exception>
		public SessionManager(Config config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			activeSessions = new Dictionary<string, Session>();

			//var databasePath = Path.Combine("quests", "sessionsNew.db");
			//var connectionString = $"URI=file:{databasePath}";
			//database = new SqliteDatabase(connectionString);
			//database = new SqliteJsonDatabase(connectionString);

			//var connectionString = "Server=localhost;Port=3306;Database=db_quests;Uid=root;Pwd=root;";
			//database = new MySqlJsonDatabase(connectionString);

			//var dbConfig = _config.Database;
			//database = DatabaseFactory.LoadOrCreateDatabase(dbConfig.DatabaseType, dbConfig.ConnectionString);

			UseDatabase(config);
		}

		//workaround to preserve parties, while not rewriting a bunch of stuff. See CustomQuestsPlugin.load()...
		internal void UseDatabase(Config config)
		{
			var dbConfig = _config.Database;
			database = DatabaseFactory.LoadOrCreateDatabase(dbConfig.DatabaseType, dbConfig.ConnectionString);
		}

		/// <summary>
		///     Disposes the session manager.
		/// </summary>
		public void Dispose()
		{
			foreach( var username in activeSessions.Keys.ToList() )
			{
				var session = activeSessions[username];

				database.Write(session.SessionInfo, username);

				session.Dispose();
				activeSessions.Remove(username);
			}

			//database.Dispose();
		}

		/// <summary>
		///     Gets the session associated with the specified player, or creates it if it does not exist.
		/// </summary>
		/// <param name="player">The player, which must not be <c>null</c>.</param>
		/// <returns>The session associated with the player.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
		public Session GetOrCreate(TSPlayer player)
		{
			if( player == null )
				throw new ArgumentNullException(nameof(player));

			var username = player.User?.Name ?? player.Name;
			if( !activeSessions.TryGetValue(username, out var session) )
			{
				var sessionInfo = database.Read(username) ?? new SessionInfo();

				session = new Session(player, sessionInfo);
				if( session.CurrentQuestInfo != null )
				{
					//throw new NotImplementedException("Rejoining quests is currently disabled.");
					//session.LoadQuest(session.CurrentQuestInfo);

					Debug.Print("DEBUG: Rejoining quests is currently disabled.");
					//session.LoadQuestX(session.CurrentQuestInfo);
				}

				session.SessionInfo.AddDefaultQuestNames(_config.DefaultQuestNames);
				activeSessions[username] = session;
			}

			return session;
		}

		/// <summary>
		///     Removes the session associated with the specified player.
		/// </summary>
		/// <param name="player">The player, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
		public void Remove(TSPlayer player)
		{
			if( player == null )
				throw new ArgumentNullException(nameof(player));

			var username = player.User?.Name ?? player.Name;
			if( activeSessions.TryGetValue(username, out var session) )
			{
				database.Write(session.SessionInfo, username);

				session.Dispose();
				activeSessions.Remove(username);
			}
		}
		
		internal void Save(Session session)
		{
			if( session == null )
				throw new ArgumentNullException();

			database.Write(session.SessionInfo, session._player.Name);
		}

		internal void SaveAll()
		{
			foreach( var session in activeSessions.Values )
				database.Write(session.SessionInfo, session._player.Name);
		}

		public void OnReload()
		{
			foreach( var session in activeSessions.Values )
			{
				//var questName = s.CurrentQuestName;
				var player = session._player;

				var quest = session.CurrentQuest;
				if( quest != null )
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
