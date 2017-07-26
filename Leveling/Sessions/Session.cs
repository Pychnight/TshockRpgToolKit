using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Leveling.Classes;
using Leveling.Levels;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace Leveling.Sessions
{
    /// <summary>
    ///     Holds session information.
    /// </summary>
    public sealed class Session
    {
        private static readonly TimeSpan CombatTextPeriod = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan ExpReportPeriod = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan ItemCheckPeriod = TimeSpan.FromSeconds(1);

        private readonly Dictionary<Class, Level> _classToLevel = new Dictionary<Class, Level>();
        private readonly object _combatTextLock = new object();
        private readonly Queue<CombatText> _combatTexts = new Queue<CombatText>();
        private readonly SessionDefinition _definition;
        private readonly object _expLock = new object();
        private readonly TSPlayer _player;
        private readonly object _saveLock = new object();

        private Class _class;
        private long _expToReport;
        private DateTime _lastCombatTextTime;
        private DateTime _lastExpReportTime;
        private DateTime _lastItemCheckTime;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Session" /> class with the specified player and definition.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        public Session(TSPlayer player, SessionDefinition definition)
        {
            Debug.Assert(player != null, "Player must not be null.");
            Debug.Assert(definition != null, "Definition must not be null.");

            _player = player;
            _definition = definition;
        }

        /// <summary>
        ///     Gets or sets the class, which must not be <c>null</c>.
        /// </summary>
        public Class Class
        {
            get => _class;
            set
            {
                Debug.Assert(value != null, "Value must not be null.");

                _class = value;
                _definition.CurrentClassName = value.Name;
                UpdateItemsAndPermissions();
            }
        }

        /// <summary>
        ///     Gets or sets the EXP based on the current class.
        /// </summary>
        public long Exp
        {
            get => _definition.ClassNameToExp[_class.Name];
            set => _definition.ClassNameToExp[_class.Name] = value;
        }

        /// <summary>
        ///     Gets the set of item IDs given.
        /// </summary>
        public ISet<int> ItemIdsGiven => _definition.ItemIdsGiven ?? new HashSet<int>();

        /// <summary>
        ///     Gets the set of item names allowed.
        /// </summary>
        public ISet<string> ItemNamesAllowed { get; private set; } = new HashSet<string>();

        /// <summary>
        ///     Gets or sets the level based on the current class.
        /// </summary>
        public Level Level
        {
            get => _classToLevel[_class];
            set
            {
                _classToLevel[_class] = value;
                _definition.ClassNameToLevelName[_class.Name] = value.Name;
                UpdateItemsAndPermissions();
            }
        }

        /// <summary>
        ///     Gets the set of completed classes.
        /// </summary>
        public ISet<Class> MasteredClasses { get; private set; } = new HashSet<Class>();

        /// <summary>
        ///     Gets the set of permissions granted.
        /// </summary>
        public ISet<string> PermissionsGranted { get; private set; } = new HashSet<string>();

        /// <summary>
        ///     Gets the set of unlocked classes.
        /// </summary>
        public ISet<Class> UnlockedClasses { get; private set; } = new HashSet<Class>();

        /// <summary>
        ///     Adds a combat text to the player with the specified text and color.
        /// </summary>
        /// <param name="text">The text, which must not be <c>null</c>.</param>
        /// <param name="color">The color.</param>
        /// <param name="global">A value indicating whether everyone should see the text.</param>
        public void AddCombatText(string text, Color color, bool global = true)
        {
            Debug.Assert(text != null, "Text must not be null.");

            lock (_combatTextLock)
            {
                _combatTexts.Enqueue(new CombatText(text, color, global));
            }
        }

        /// <summary>
        ///     Adds the specified amount of EXP to report.
        /// </summary>
        /// <param name="exp">The amount of EXP.</param>
        public void AddExpToReport(long exp)
        {
            lock (_expLock)
            {
                _expToReport += exp;
            }
        }

        /// <summary>
        ///     Adds an item ID given.
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        public void AddItemId(int itemId)
        {
            if (_definition.ItemIdsGiven == null)
            {
                _definition.ItemIdsGiven = new HashSet<int>();
            }
            _definition.ItemIdsGiven.Add(itemId);
            Save();
        }

        /// <summary>
        ///     Gives the specified amount of EXP.
        /// </summary>
        /// <param name="exp">The amount of EXP.</param>
        public void GiveExp(long exp)
        {
            var levelIndex = Class.Levels.IndexOf(Level);
            if (exp < 0)
            {
                Debug.WriteLine($"DEBUG: Taking {-exp} EXP from {_player.Name}");

                var newExp = Exp + exp;
                // Don't allow down-leveling if this is the first level of the class or if the previous level had a
                // special requirement for leveling up.
                if (newExp < 0 && levelIndex != 0 && Class.Levels[levelIndex - 1].ExpRequired > 0)
                {
                    LevelDown();
                    Exp = Level.ExpRequired;
                    GiveExp(newExp);
                }
                else
                {
                    Exp = Math.Max(0, newExp);
                }
            }
            else if (exp > 0)
            {
                // Don't allow up-leveling if this is the last level of the class or if the current level has a special
                // requirement for leveling up.
                if (levelIndex == Class.Levels.Count - 1 || Level.ExpRequired < 0)
                {
                    return;
                }

                Debug.WriteLine($"DEBUG: Giving {exp} EXP to {_player.Name}");

                var newExp = Exp + exp;
                if (newExp >= Level.ExpRequired)
                {
                    var surplus = newExp - Level.ExpRequired;
                    LevelUp();
                    GiveExp(surplus);
                }
                else
                {
                    Exp = newExp;
                }
            }
        }

        /// <summary>
        ///     Determines if the player has the specified level.
        /// </summary>
        /// <param name="level">The level, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the player does; otherwise, <c>false</c>.</returns>
        public bool HasLevel(Level level)
        {
            Debug.Assert(level != null, "Level must not be null.");

            var @class = level.Class;
            return @class.Levels.IndexOf(_classToLevel[@class]) >= @class.Levels.IndexOf(level);
        }

        /// <summary>
        ///     Determines if the player has the prerequisites for the specified class.
        /// </summary>
        /// <param name="class">The class, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the player does; otherwise, <c>false</c>.</returns>
        public bool HasPrerequisites(Class @class)
        {
            Debug.Assert(@class != null, "Class must not be null.");

            return @class.PrerequisiteLevels.All(HasLevel) &&
                   @class.PrerequisitePermissions.All(p => _player.HasPermission(p));
        }

        /// <summary>
        ///     Levels down the player.
        /// </summary>
        /// <returns><c>true</c> if the player successfully leveled down; otherwise, <c>false</c>.</returns>
        public bool LevelDown()
        {
            var levelIndex = Class.Levels.IndexOf(Level);
            if (levelIndex == 0)
            {
                return false;
            }

            Level = Class.Levels[levelIndex - 1];
            Exp = 0;

            // Notify the player.
            Debug.WriteLine($"DEBUG: Leveled down {_player.Name}");
            _player.SendInfoMessage($"You [c/{Color.OrangeRed.Hex3()}:leveled down] to a {Level} {Class}.");
            AddCombatText("Leveled down!", Color.OrangeRed);
            MasteredClasses.Remove(Class);
            _definition.MasteredClassNames.Remove(Class.Name);

            // Handle commands for leveling down on the current level.
            foreach (var command in Level.CommandsOnLevelDown)
            {
                var command2 = command.Replace("$name", _player.GetEscapedName());
                Debug.WriteLine($"DEBUG: Executing {command2}");
                Commands.HandleCommand(TSPlayer.Server, command2);
            }
            Save();
            return true;
        }

        /// <summary>
        ///     Levels up the player.
        /// </summary>
        /// <returns><c>true</c> if the player successfully leveled up; otherwise, <c>false</c>.</returns>
        public bool LevelUp()
        {
            var levelIndex = Class.Levels.IndexOf(Level);
            if (levelIndex == Class.Levels.Count - 1)
            {
                return false;
            }

            Level = Class.Levels[levelIndex + 1];
            Exp = 0;

            // Notify the player.
            Debug.WriteLine($"DEBUG: Leveled up {_player.Name}");
            _player.SendInfoMessage($"You [c/{Color.LimeGreen.Hex3()}:leveled up] to a {Level} {Class}.");
            if (levelIndex + 1 == Class.Levels.Count - 1)
            {
                _player.SendInfoMessage($"You have mastered the {Class} class.");
                MasteredClasses.Add(Class);
                _definition.MasteredClassNames.Add(Class.Name);
            }
            AddCombatText("Leveled up!", Color.LimeGreen);

            // Handle commands for leveling up for the new level.
            foreach (var command in Level.CommandsOnLevelUp)
            {
                var command2 = command.Replace("$name", _player.GetEscapedName());
                Debug.WriteLine($"DEBUG: Executing {command2}");
                Commands.HandleCommand(TSPlayer.Server, command2);
            }
            Save();
            return true;
        }

        /// <summary>
        ///     Resolves the session using the specified classes.
        /// </summary>
        /// <param name="classes">The classes, which must not be <c>null</c> or contain <c>null</c>.</param>
        public void Resolve(IList<Class> classes)
        {
            Debug.Assert(classes != null, "Classes must not be null.");
            Debug.Assert(!classes.Contains(null), "Classes must not contain null.");

            foreach (var kvp in _definition.ClassNameToLevelName)
            {
                var @class = classes.First(c => c.Name == kvp.Key);
                var level = @class.Levels.First(l => l.Name == kvp.Value);
                _classToLevel[@class] = level;
            }

            Class = classes.First(c => c.Name == _definition.CurrentClassName);
            MasteredClasses = new HashSet<Class>(
                _definition.MasteredClassNames.Select(mcn => classes.First(c => c.Name == mcn)));
            UnlockedClasses = new HashSet<Class>(
                _definition.UnlockedClassNames.Select(ucn => classes.First(c => c.Name == ucn)));
        }

        /// <summary>
        ///     Saves the session.
        /// </summary>
        public void Save()
        {
            lock (_saveLock)
            {
                var username = _player.User?.Name ?? _player.Name;
                var path = Path.Combine("leveling", $"{username}.session");
                File.WriteAllText(path, JsonConvert.SerializeObject(_definition, Formatting.Indented));
            }
        }

        /// <summary>
        ///     Unlocks the specified class.
        /// </summary>
        /// <param name="class">The class, which must not be <c>null</c>.</param>
        public void UnlockClass(Class @class)
        {
            Debug.Assert(@class != null, "Class must not be null.");

            Debug.WriteLine($"DEBUG: Unlocking {@class} for {_player.Name}");

            _classToLevel[@class] = @class.Levels[0];
            UnlockedClasses.Add(@class);
            _definition.ClassNameToExp[@class.Name] = 0;
            _definition.ClassNameToLevelName[@class.Name] = @class.Levels[0].Name;
            _definition.UnlockedClassNames.Add(@class.Name);
        }

        /// <summary>
        ///     Updates the session.
        /// </summary>
        public void Update()
        {
            // Check EXP reports.
            lock (_expLock)
            {
                if (Level.ExpRequired < 0)
                {
                    _expToReport = 0;
                }
                else if (DateTime.UtcNow - _lastExpReportTime > ExpReportPeriod && _expToReport != 0)
                {
                    _lastExpReportTime = DateTime.UtcNow;

                    if (_expToReport > 0)
                    {
                        AddCombatText($"+{_expToReport} EXP", Color.LimeGreen);
                    }
                    else
                    {
                        AddCombatText($"{_expToReport} EXP", Color.OrangeRed);
                    }
                    Save();
                    _expToReport = 0;
                }
            }

            // Check to see if items are usable.
            if (DateTime.UtcNow - _lastItemCheckTime > ItemCheckPeriod)
            {
                _lastItemCheckTime = DateTime.UtcNow;

                var tplayer = _player.TPlayer;
                var items = new[] {_player.SelectedItem}.Concat(tplayer.armor).Concat(tplayer.dye)
                    .Concat(tplayer.miscEquips).Concat(tplayer.miscDyes);
                foreach (var item in items)
                {
                    if (LevelingPlugin.ItemNameToLevelRequirements.TryGetValue(item.Name, out var level) &&
                        !ItemNamesAllowed.Contains(item.Name))
                    {
                        tplayer.itemRotation = 0;
                        tplayer.itemAnimation = 0;
                        _player.SetBuff(!tplayer.buffImmune[BuffID.Cursed] ? BuffID.Cursed : BuffID.Webbed, 120,
                                        true);
                        _player.SendData(PacketTypes.PlayerAnimation, "", _player.Index);
                        _player.SendErrorMessage($"You must be a {level} {level.Class} to use " +
                                                 $"[i/p{item.prefix},s{item.stack}:{item.type}].");
                    }
                }
            }

            // Check combat texts.
            lock (_combatTextLock)
            {
                if (DateTime.UtcNow - _lastCombatTextTime > CombatTextPeriod && _combatTexts.Count > 0)
                {
                    _lastCombatTextTime = DateTime.UtcNow;

                    var combatText = _combatTexts.Dequeue();
                    var tplayer = _player.TPlayer;
                    if (combatText.Global)
                    {
                        TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended, combatText.Text,
                                              (int)combatText.Color.PackedValue, tplayer.Center.X, tplayer.Center.Y);
                    }
                    else
                    {
                        _player.SendData(PacketTypes.CreateCombatTextExtended, combatText.Text,
                                         (int)combatText.Color.PackedValue, tplayer.Center.X, tplayer.Center.Y);
                    }
                }
            }
        }

        private void UpdateItemsAndPermissions()
        {
            ItemNamesAllowed = new HashSet<string>(_class.ItemNamesAllowed);
            PermissionsGranted = new HashSet<string>(_class.PermissionsGranted);
            foreach (var level in _class.Levels)
            {
                ItemNamesAllowed.UnionWith(level.ItemNamesAllowed);
                PermissionsGranted.UnionWith(level.PermissionsGranted);

                if (level == Level)
                {
                    break;
                }
            }
        }

        private sealed class CombatText
        {
            public CombatText(string text, Color color, bool global)
            {
                Debug.Assert(text != null, "Text must not be null.");

                Text = text;
                Color = color;
                Global = global;
            }

            public Color Color { get; }
            public bool Global { get; }
            public string Text { get; }
        }
    }
}
