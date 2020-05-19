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
using TShockAPI;
using Banking;

namespace Leveling.Sessions
{
    /// <summary>
    ///     Holds session information.
    /// </summary>
    public sealed class Session
    {
        private static readonly TimeSpan CombatTextPeriod = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan ExpReportPeriod = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan ItemCheckPeriod = TimeSpan.FromSeconds(0.5);

		private readonly object combatTextLock = new object();
		private readonly object expLock = new object();

        private readonly Dictionary<Class, Level> _classToLevel = new Dictionary<Class, Level>();
        private readonly Queue<CombatText> _combatTexts = new Queue<CombatText>();
        internal readonly SessionDefinition _definition;//just so we can dump it for debugging.
        private readonly TSPlayer _player;
        
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

				var lastClass = _class;

                _class = value;
                _definition.CurrentClassName = value.Name;
				UpdateItemsAndPermissions();
				SetBankAccountForClass(_class);

				if( !_definition.UsedClassNames.Contains(value.Name))
				{
					foreach( var command in _class.CommandsOnClassChangeOnce )
					{
						var command2 = command.Replace("$name", _player.GetEscapedName());
						Debug.WriteLine($"DEBUG: Executing {command2}");
						Commands.HandleCommand(TSPlayer.Server, command2);
					}
					_definition.UsedClassNames.Add(value.Name);
				}
								
				if(_class!=lastClass)
				{
					//_class?.Definition?.OnClassChange(_player,_class,lastClass);
					_class?.OnClassChange(_player, lastClass);
				}
            }
        }

        /// <summary>
        ///     Gets or sets the EXP based on the current class.
        /// </summary>
        public long Exp
        {
            get
			{
				var account = BankingPlugin.Instance.GetBankAccount(this._player.Name, "Exp");
				//Debug.Print($"Get Exp account: {account.Balance}");

				//return _definition.ClassNameToExp[_class.Name];
				return (long)account.Balance;
			}
            set
			{
				var account = BankingPlugin.Instance.GetBankAccount(this._player.Name, "Exp");
				//account.Deposit(value);
				Debug.Print($"Set Exp account: {account.Balance}");

				//_definition.ClassNameToExp[_class.Name] = value;
				account.SetBalance(value);
			}
		}

        /// <summary>
        ///     Gets the set of item IDs given.
        /// </summary>
        public ISet<int> ItemIdsGiven => _definition.ItemIdsGiven;

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

			lock(combatTextLock)
			{
				_combatTexts.Enqueue(new CombatText(text, color, global));
			}
        }

		/// <summary>
		/// Handles the Banking AccountDeposit and AccountWithdraw events.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		internal static void OnBankAccountBalanceChanged(object sender, BalanceChangedEventArgs args)
		{
			if( args.IsServerAccount )
				return;

			//Debug.Print($"BankAccountBalanceChanged! Player {args.OwnerName} - {args.AccountName}.");

			//we're only interested in non experience account transactions. ( or put differently, we want publicly visible accounts, not our hidden shadow accounts.)
			if( args.AccountName.StartsWith(LevelingPlugin.BankAccountNamePrefix) )
				return;

			//dont bother with session for disconnected players ( bank may update accounts on players not logged in )
			var player = TSPlayer.FindByNameOrID(args.OwnerName).FirstOrDefault();
			if( player == null )
				return;
							
			var session = LevelingPlugin.Instance.GetOrCreateSession(player);
			var lvlCurrency = session.Class.LevelingCurrency;
			
			//if this currency is not the leveling currency, then ignore
			if( lvlCurrency?.InternalName != args.AccountName )
				return;

			session.Exp += (long)args.Change;
			session.ExpUpdated(args);
		}

		/// <summary>
		/// Updates all experience related members, by looking at the latest Exp balance change.
		/// </summary>
		internal void ExpUpdated(BalanceChangedEventArgs args)
		{
			//Debug.Print($"ExpUpdated!");

			var levelIndex = Class.Levels.IndexOf(Level);

			if( Exp<0 )
			{
				if(levelIndex > 0 && Class.Levels[levelIndex - 1].ExpRequired > 0 )
				{
					//we leveled down
					var delta = Exp;

					LevelDown();

					//set previous level's new exp
					Exp = Level.ExpRequired;
					GiveExp(delta);
				}
				else
				{
					Exp = 0;
				}
			}
			else
			{
				// Don't allow up-leveling if this is the last level of the class or if the current level has a special
				// requirement for leveling up.
				if( levelIndex == Class.Levels.Count - 1 || Level.ExpRequired < 0 )
				{
					//clip overage
					if( Level.ExpRequired>0 && Exp > Level.ExpRequired )
						Exp = Level.ExpRequired;

					return;
				}

				//Debug.WriteLine($"DEBUG: Giving {exp} EXP to {_player.Name}");

				//var newExp = Exp + exp;
				if( Exp >= Level.ExpRequired )
				{
					var surplus = Exp - Level.ExpRequired;
					LevelUp();
					GiveExp(surplus);
				}
			}
		}

        /// <summary>
        ///     Adds the specified amount of EXP to report.
        /// </summary>
        /// <param name="exp">The amount of EXP.</param>
        public void AddExpToReport(long exp)
        {
			//Debug.Print($"AddExpToReport would be {exp} ( currently disabled. )");

			lock( expLock )
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
			var account = BankingPlugin.Instance.GetBankAccount(_player, "Exp" );

			Debug.Assert(account != null, "Tried to GiveExp, but Exp BankAccount is null.");
			
			if(exp<0)
			{
				var result = account.TryWithdraw(Math.Abs(exp), withdrawalMode: WithdrawalMode.AllowOverdraw);

				Debug.Assert(result, "Tried to take Exp, but BankAccount withdraw operation failed.");
			}
			else if(exp>0)
				account.Deposit(exp);
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
            if (!_classToLevel.ContainsKey(level.Class))
            {
                return false;
            }

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
		/// Resets all player leveling data, moving them back to the first level of the default class.
		/// </summary>
		public void LevelReset()
		{
			//Debug.Print("LevelReset!");
			var def = _definition;
			
			MasteredClasses.Clear();
			UnlockedClasses.Clear();
			PermissionsGranted.Clear();
			def.ItemIdsGiven?.Clear();

			def.LevelNamesObtained.Clear();
			def.MasteredClassNames.Clear();
			def.UnlockedClassNames.Clear();
			def.UsedClassNames.Clear();
									
			Level = Class.Levels.FirstOrDefault();
			Exp = 0;

			def.Initialize();
			Resolve(LevelingPlugin.Instance._classes);
			Save();
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
						
			Class.OnLevelDown(_player, levelIndex - 1);

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

			var def = Class.Definition;

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
								
				Class.OnClassMastered(_player);
			}
            AddCombatText("Leveled up!", Color.LimeGreen);

            // Handle commands for leveling up for the new level.
            foreach (var command in Level.CommandsOnLevelUp)
            {
                var command2 = command.Replace("$name", _player.GetEscapedName());
                Debug.WriteLine($"DEBUG: Executing {command2}");
                Commands.HandleCommand(TSPlayer.Server, command2);
            }
            if (!_definition.LevelNamesObtained.Contains(Level.Name))
            {
                foreach (var command in Level.CommandsOnLevelUpOnce)
                {
                    var command2 = command.Replace("$name", _player.GetEscapedName());
                    Debug.WriteLine($"DEBUG: Executing {command2}");
                    Commands.HandleCommand(TSPlayer.Server, command2);
                }
            }
            _definition.LevelNamesObtained.Add(Level.Name);
            Save();
						
			Class.OnLevelUp(_player, levelIndex + 1);
			
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

			//ensure bank accounts..
			EnsureBankAccounts(classes);
			SetBankAccountForClass(Class);
		}

		/// <summary>
		/// Creates bank accounts for each Class, for the Player if none exist.
		/// </summary>
		/// <param name="classes"></param>
		internal void EnsureBankAccounts(IEnumerable<Class> classes)
		{
			var bank = BankingPlugin.Instance?.Bank;

			if( bank != null )
			{
				var accountMap = bank[this._player.Name];

				foreach(var cl in classes)
				{
					var account = accountMap.GetOrCreateBankAccount(GetBankAccountNameForClass(cl));
				}
			}
		}

		/// <summary>
		/// Sets the bank account used for the Class. 
		/// </summary>
		/// <remarks>For now, we remap the bank account name "Exp" to any number of user-hidden, "exp_class" bank accounts.</remarks>
		/// <param name="klass">Class</param>
		private void SetBankAccountForClass(Class klass)
		{
			var bank = BankingPlugin.Instance?.Bank;

			if( bank != null )
			{
				var accountMap = bank[this._player.Name];
				var currentClassAccountName = GetBankAccountNameForClass(klass);

				//Debug.Print($"Overriding Exp to route to: {currentClassAccountName}");
				accountMap.SetAccountNameOverride("Exp", currentClassAccountName);
			}
		}
		
		private static string GetBankAccountNameForClass(Class klass)
		{
			return $"Exp_{klass.Name}";
		}

        /// <summary>
        ///     Saves the session.
        /// </summary>
        public void Save()
        {
			var userName = _player.Name ?? _player.Name;
			LevelingPlugin.Instance.SessionRepository.Save(userName,_definition);
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
			lock( expLock )
			{
				if( Level.ExpRequired < 0 )
				{
					_expToReport = 0;
				}
				else if( DateTime.UtcNow - _lastExpReportTime > ExpReportPeriod && _expToReport != 0 )
				{
					_lastExpReportTime = DateTime.UtcNow;

					if( _expToReport > 0 )
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
                        _player.SendData(PacketTypes.PlayerAnimation, "", _player.Index);
                        _player.Disable("", DisableFlags.None);
                        _player.SendErrorMessage($"You must be a {level} {level.Class} to use " +
                                                 $"[i/p{item.prefix},s{item.stack}:{item.type}].");
                    }
                }
            }

			lock( combatTextLock )
			{
				if( DateTime.UtcNow - _lastCombatTextTime > CombatTextPeriod && _combatTexts.Count > 0 )
				{
					_lastCombatTextTime = DateTime.UtcNow;

					var combatText = _combatTexts.Dequeue();
					var tplayer = _player.TPlayer;
					if( combatText.Global )
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
