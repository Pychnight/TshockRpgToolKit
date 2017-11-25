using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Mono.Data.Sqlite;
using Leveling.Classes;
using Leveling.Levels;
using Leveling.Sessions;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using Wolfje.Plugins.SEconomy;


namespace Leveling.Database
{
    public sealed class DatabaseManager
    {
        private readonly IDbConnection _connection;
        private readonly object _lock = new object();
        private readonly List<Class> _classes = new List<Class>();
        private readonly List<Level> _shops = new List<Level>();
        private readonly List<Session> _session = new List<Session>();
        public DatabaseManager(IDbConnection connection)
        {
            _connection = connection;
            if (_connection != null)
            {
                _connection.Query("CREATE TABLE IF NOT EXISTS Classes (" +
                                 "  DispayName     TEXT," +
                                 "  AllowSwitching INTEGER," +
                                 "  PlayerID       INTEGER," +
                                 "  AllowSwitchingBeforeMastery    INTEGER," +
                                 "  DeathPenaltyMultiplierOverride REAL," +
                                 "  ExpMultiplierOverride          REAL," +
                                 "  ClassName      TEXT," +
                                 "  SEconomyCost   INTEGER," +
                                 "  Class_ID  INTEGER PRIMARY KEY NOT NULL)");
                _connection.Query("CREATE TABLE IF NOT EXISTS PrerequisiteLevelNames(" +
                                  "  LevelName      TEXT," +
                                  "  Class_ID       INTEGER NOT NULL," +
                                  "  FOREIGN KEY(Class_ID)," +
                                  "  REFERENCES Classes(Class_ID) ON DELETE CASCADE," +
                                  "  UNIQUE(LevelName) ON CONFLICT IGNORE)");
                _connection.Query("CREATE TABLE IF NOT EXISTS ClassPrerequisitePermissions (" +
                                  "  PermissionName   TEXT," +
                                  "  Class_ID         INTEGER NOT NULL," +
                                  "  FOREIGN KEY(Class_ID)," +
                                  "  REFERENCES Classes(Class_ID) ON DELETE CASCADE," +
                                  "  UNIQUE(PermissionName) ON CONFLICT IGNORE)");
                _connection.Query("CREATE TABLE IF NOT EXISTS Levels(" +
                                  "  DisplayName       TEXT," +
                                  "  Prefix            TEXT," +
                                  "  LevelName         TEXT," +
                                  "  ExpRequired       INTEGER," +
                                  "  Class_ID          INTEGER NOT NULL," +
                                  "  FOREIGN KEY(Class_ID)," +
                                  "  RFERENCES Classes(Class_ID) ON DELETE CASCADE," +
                                  "  Level_ID  INTEGER PRIMARY KEY NOT NULL)");
                _connection.Query("CREATE TABLE IF NOT EXISTS Sessions(" +
                                  "  CurrentClassName  TEXT," +
                                  "  Player_ID         INTEGER NOT NULL," +
                                  "  Session_ID        INTEGER PRIMARY KEY NOT NULL");
            }
            else
            {
                throw new Exception("Error Database Connection is null Refer to initializer in Database Manager.cs");
            }
        }
    }
}
