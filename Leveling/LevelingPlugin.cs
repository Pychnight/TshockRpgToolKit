using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Banking;
using Banking.Rewards;
using Corruption.PluginSupport;
using Leveling.Classes;
using Leveling.Database;
using Leveling.Levels;
//using Leveling.LoaderDsl;
using Leveling.Sessions;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Leveling
{
    [ApiVersion(2, 1)]
    public sealed class LevelingPlugin : TerrariaPlugin
    {
		/// <summary>
		/// The prefix used for BankAccounts created by this plugin.
		/// </summary>
		public const string BankAccountNamePrefix = "Exp_";

        internal const string SessionKey = "Leveling_Session";

		public static LevelingPlugin Instance { get; private set; }

        public static readonly Dictionary<string, Level> ItemNameToLevelRequirements = new Dictionary<string, Level>();

        private static readonly string ConfigPath = Path.Combine("leveling", "config.json");

		internal ISessionDatabase SessionRepository;

        private readonly ConditionalWeakTable<NPC, Dictionary<TSPlayer, int>> _npcDamages = new ConditionalWeakTable<NPC, Dictionary<TSPlayer, int>>();

        private List<ClassDefinition> _classDefinitions;
        internal List<Class> _classes;

		//internal CurrencyDefinition PurchaseCurrency { get; set; }
		internal CurrencyDefinition ExpCurrency { get; set; } 

        public LevelingPlugin(Main game) : base(game)
        {
//#if DEBUG
//            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
//#endif

			Instance = this;
        }

        public override string Author => "MarioE, Timothy Barela";
        public override string Description => "Provides RPG-styled leveling and classes.";
        public override string Name => "Leveling";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		public override void Initialize()
        {
			GeneralHooks.ReloadEvent += OnReload;
            PlayerHooks.PlayerChat += OnPlayerChat;
            PlayerHooks.PlayerPermission += OnPlayerPermission;
			ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            //ServerApi.Hooks.NetGetData.Register(this, OnNetGetData, int.MinValue);
            //ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
			
            Commands.ChatCommands.Add(new Command("leveling.addhp", AddHp, "addhp")
            {
                HelpText = $"Syntax: {Commands.Specifier}addhp <player-name> <hp-amount>\n" +
                           "Adds an amount of max HP to the specified player."
            });
            Commands.ChatCommands.Add(new Command("leveling.addmp", AddMp, "addmp")
            {
                HelpText = $"Syntax: {Commands.Specifier}addmp <player-name> <hp-amount>\n" +
                           "Adds an amount of max MP to the specified player."
            });
            Commands.ChatCommands.Add(new Command("leveling.class", ClassCmd, "class")
            {
                HelpText = $"Syntax: {Commands.Specifier}class [class-name]\n" +
                           "Shows available classes or changes your current class."
            });
            Commands.ChatCommands.Add(new Command("leveling.exp", Exp, "exp")
            {
                AllowServer = false,
                HelpText = $"Syntax: {Commands.Specifier}exp\n" +
                           "Shows your current level and EXP."
            });
            Commands.ChatCommands.Add(new Command("leveling.giveexp", GiveExp, "giveexp")
            {
                HelpText = $"Syntax: {Commands.Specifier}giveexp <player-name> <exp-amount>\n" +
                           "Gives an amount of EXP to the specified player."
            });
            Commands.ChatCommands.Add(new Command("leveling.giveonce", GiveOnce, "giveonce")
            {
                HelpText = $"Syntax: {Commands.Specifier}giveonce <player-name> <item-name> [stack] [prefix]\n" +
                           "Gives an item to the specified player, but only once."
            });
            Commands.ChatCommands.Add(new Command("leveling.leveldown", LevelDown, "leveldown")
            {
                HelpText = $"Syntax: {Commands.Specifier}leveldown <player-name>\n" +
                           "Levels down the specified player."
            });
            Commands.ChatCommands.Add(new Command("leveling.levelup", LevelUp, "levelup")
            {
                HelpText = $"Syntax: {Commands.Specifier}levelup <player-name>\n" +
                           "Levels up the specified player."
            });
			Commands.ChatCommands.Add(new Command("leveling.levelreset", LevelReset, "levelreset")
			{
				HelpText = $"Syntax: {Commands.Specifier}levelreset <player-name>\n" +
						  "Resets the player's level to default."
			});
            Commands.ChatCommands.Add(new Command("leveling.sendto", SendTo, "sendto")
            {
                HelpText = $"Syntax: {Commands.Specifier}sendto <player-name> <rrr,ggg,bbb> <text>\n" +
                           "Sends text in a certain color to the specified player."
            });
            Commands.ChatCommands.Add(new Command("leveling.setclass", SetClass, "setclass")
            {
                HelpText = $"Syntax: {Commands.Specifier}setclass <player-name> <class-name>\n" +
                           "Sets the specified player's class."
            });

			Commands.ChatCommands.Add(new Command("leveling.dump", LevelDump, "leveldump")
			{
				HelpText = $"Syntax: {Commands.Specifier}leveldump\n" +
						   "Dumps debug information about the players level to a file."
			});
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));
                foreach (var session in TShock.Players.Where(p => p?.Active == true).Select(GetOrCreateSession))
                {
                    session.Save();
                }

                GeneralHooks.ReloadEvent -= OnReload;
                PlayerHooks.PlayerChat -= OnPlayerChat;
                PlayerHooks.PlayerPermission -= OnPlayerPermission;
				ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
				ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
				//ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
				//ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            }

            base.Dispose(disposing);
        }

		private void OnGamePostInitialize(EventArgs args)
		{
			onLoad();
		}

		private void OnReload(ReloadEventArgs args)
		{
			onLoad();

			// We have to resolve sessions again.
			foreach( var session in TShock.Players.Where(p => p?.Active == true).Select(GetOrCreateSession) )
			{
				session.Resolve(_classes);
			}

			args.Player.SendSuccessMessage("[Leveling] Reloaded config!");
		}

		private void initializeBanking()
		{
			if(BankingPlugin.Instance==null)
			{
				throw new Exception($"Unable to retrieve BankingPlugin.Instance");
				//ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Error: Unable to retrieve BankingPlugin.Instance", TraceLevel.Error);
			}
						
			var bank = BankingPlugin.Instance.Bank;

			//bank.BankAccountBalanceChanged += (s, a) =>
			//{
			//	Debug.Print($"BankAccountBalanceChanged! {a.Name}");
			//};

			bank.BankAccountBalanceChanged += Session.OnBankAccountBalanceChanged;
		}

		private void onLoad()
		{
			const string classDirectory = "leveling";
						
			Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);
			
			var dbConfig = Config.Instance.DatabaseConfig;
			SessionRepository = SessionDatabaseFactory.LoadOrCreateDatabase(dbConfig.DatabaseType, dbConfig.ConnectionString);

			initializeBanking();

			Directory.CreateDirectory(classDirectory);

			//var classDefs = loadClassDefinitions(classDirectory);
			var classDefs = ClassDefinition.Load(classDirectory);
			
			//filter out duplicate names
			//var classNames = new HashSet<string>(classDefs.Select(cd => cd.Name));
			//var booDefs = loadBooClassDefinitions(classDirectory)
			//				.Where(cd => !classNames.Contains(cd.Name))
			//				.Select(cd => cd);
										
			//classDefs.AddRange(booDefs);
			//classDefs.ForEach(cd => cd.ValidateAndFix());
						
			_classDefinitions = classDefs;
			_classes = _classDefinitions.Select(cd => new Class(cd)).ToList();
						
			////if default class file does not exist, we're in an error state
			//if( _classDefinitions.Select(cd => cd.Name).
			//	FirstOrDefault(n => n == Config.Instance.DefaultClassName) == null )
			//{
			//	//throw new Exception($"DefaultClassName: '{Config.Instance.DefaultClassName}' was not found.");
			//	ServerApi.LogWriter.PluginWriteLine(this, $"DefaultClassName: '{Config.Instance.DefaultClassName}' was not found. ", TraceLevel.Error);
			//	//_classDefinitions.Clear();
			//	//_classes.Clear();
			//	//return;
			//}

			//foreach(var def in classDefs)
			//{
			//	//disabled during conversion to multi currency
			//	//def.PreParseRewardValues(ExpCurrency);
			//	def.PreParseRewardValues();
			//}
			
			ItemNameToLevelRequirements?.Clear();
			var levels = _classes.SelectMany(c => c.Levels).ToList();
			foreach( var @class in _classes )
			{
				@class.Resolve(levels, 0);
			}
			foreach( var @class in _classes )
			{
				@class.Resolve(levels, 1);
			}
			foreach( var level in levels )
			{
				foreach( var itemName in level.ItemNamesAllowed )
				{
					ItemNameToLevelRequirements[itemName] = level;
				}
			}

			//disabled during multi currency conversion
			//BankingPlugin.Instance.RewardDistributor.SetRewardModifier(ExpCurrency.InternalName, RewardReason.Killing, new ClassExpRewardEvaluator());

			//_classDefinitions = Directory.EnumerateFiles("leveling", "*.class", SearchOption.AllDirectories)
			//  .Select(p => JsonConvert.DeserializeObject<ClassDefinition>(File.ReadAllText(p))).ToList();
			//_classes = _classDefinitions.Select(cd => new Class(cd)).ToList();

			//ItemNameToLevelRequirements.Clear();
			//var levels = _classes.SelectMany(c => c.Levels).ToList();
			//foreach( var @class in _classes )
			//{
			//	@class.Resolve(levels, 0);
			//}
			//foreach( var @class in _classes )
			//{
			//	@class.Resolve(levels, 1);
			//}
			//foreach( var level in levels )
			//{
			//	foreach( var itemName in level.ItemNamesAllowed )
			//	{
			//		ItemNameToLevelRequirements[itemName] = level;
			//	}
			//}
		}
		
		//private List<ClassDefinition> loadBooClassDefinitions(string directoryPath)
		//{
		//	//Debug.Print("Loading boo classes...");
		//	var fileNames = Directory.EnumerateFiles(directoryPath, "*.bclass", SearchOption.AllDirectories);
		//	var classCompiler = new ClassCompiler();
		//	var definitions = new List<ClassDefinition>();
			
		//	foreach( var classFile in fileNames )
		//	{
		//		try
		//		{
		//			var def = classCompiler.LoadClassDefinition(classFile);
		//			var newClass = new Class(def);
		//			definitions.Add(def);
		//		}
		//		catch(Exception ex)
		//		{
		//			Debug.Print($"Error while loading boo class '{classFile}'.");
		//			Debug.Print($"{ex.Message}");
		//		}
		//	}
			
		//	return definitions;
		//}

        private void AddHp(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}addhp <player-name> <hp-amount>");
                return;
            }

            var inputPlayerName = parameters[0];
            var players = TShock.Utils.FindPlayer(inputPlayerName);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                return;
            }
            if (players.Count > 1)
            {
                player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var inputHpAmount = parameters[1];
            if (!int.TryParse(inputHpAmount, out var hpAmount) || hpAmount == 0)
            {
                player.SendErrorMessage($"Invalid HP amount '{inputHpAmount}'.");
                return;
            }

            var otherPlayer = players[0];
            var session = GetOrCreateSession(otherPlayer);
            otherPlayer.TPlayer.statLifeMax = Math.Max(100, otherPlayer.TPlayer.statLifeMax + hpAmount);
            TSPlayer.All.SendData(PacketTypes.PlayerHp, "", otherPlayer.Index);

            player.SendSuccessMessage($"Gave {otherPlayer.Name} {hpAmount} HP.");
            if (hpAmount > 0)
            {
                otherPlayer.SendInfoMessage($"You gained [c/{CombatText.HealLife.Hex3()}:{hpAmount} HP].");
                session.AddCombatText($"+{hpAmount} HP", CombatText.HealLife);
            }
            else
            {
                otherPlayer.SendInfoMessage($"You lost [c/{CombatText.DamagedFriendly.Hex3()}:{-hpAmount} HP].");
                session.AddCombatText($"{hpAmount} HP", CombatText.DamagedFriendly);
            }
        }

        private void AddMp(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}addmp <player-name> <mp-amount>");
                return;
            }

            var inputPlayerName = parameters[0];
            var players = TShock.Utils.FindPlayer(inputPlayerName);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                return;
            }
            if (players.Count > 1)
            {
                player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var inputMpAmount = parameters[1];
            if (!int.TryParse(inputMpAmount, out var mpAmount) || mpAmount == 0)
            {
                player.SendErrorMessage($"Invalid MP amount '{inputMpAmount}'.");
                return;
            }

            var otherPlayer = players[0];
            var session = GetOrCreateSession(otherPlayer);
            otherPlayer.TPlayer.statManaMax = Math.Max(20, otherPlayer.TPlayer.statManaMax + mpAmount);
            TSPlayer.All.SendData(PacketTypes.PlayerMana, "", otherPlayer.Index);

            player.SendSuccessMessage($"Gave {otherPlayer.Name} {mpAmount} MP.");
            if (mpAmount > 0)
            {
                otherPlayer.SendInfoMessage($"You gained [c/{CombatText.HealMana.Hex3()}:{mpAmount} MP].");
                session.AddCombatText($"+{mpAmount} MP", CombatText.HealMana);
            }
            else
            {
                otherPlayer.SendInfoMessage($"You lost [c/{CombatText.DamagedFriendly.Hex3()}:{-mpAmount} MP].");
                session.AddCombatText($"{mpAmount} MP", CombatText.DamagedFriendly);
            }
        }

        private void ClassCmd(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count > 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}class [class-name]");
                return;
            }

            var session = GetOrCreateSession(player);
            if (parameters.Count == 0)
            {
                var classes = session.UnlockedClasses.Except(session.MasteredClasses).ToList();
                var newClasses = _classes.Except(session.UnlockedClasses).Where(session.HasPrerequisites).ToList();
                if (classes.Count > 0)
                {
                    player.SendInfoMessage($"Classes: {string.Join(", ", classes)}");
                }
                if (session.MasteredClasses.Count > 0)
                {
                    player.SendInfoMessage(
                        $"Mastered classes: [c/{Color.LightSlateGray.Hex3()}:{string.Join(", ", session.MasteredClasses)}]");
                }
                if (newClasses.Count > 0)
                {
                    player.SendInfoMessage(
                        $"New classes: [c/{Color.LimeGreen.Hex3()}:{string.Join(", ", newClasses)}]");
                }
            }
            else
            {
                var inputClassName = parameters[0];
                var klass = _classes.FirstOrDefault(
                    c => string.Equals(c.DisplayName, inputClassName, StringComparison.OrdinalIgnoreCase));
                if (klass == null)
                {
                    player.SendErrorMessage($"Invalid class '{inputClassName}'.");
                    return;
                }

                if (!session.Class.AllowSwitching)
                {
                    player.SendErrorMessage("You can't switch classes.");
                    return;
                }
                if (!session.Class.AllowSwitchingBeforeMastery && !session.MasteredClasses.Contains(session.Class))
                {
                    player.SendErrorMessage("You can't switch classes until you've mastered your current one.");
                    return;
                }

                if (session.UnlockedClasses.Contains(klass))
                {
                    session.Class = klass;
                    player.SendSuccessMessage($"Changed to the {klass} class.");
                    return;
                }

                var missingLevels = klass.PrerequisiteLevels.Where(l => !session.HasLevel(l)).ToList();
                if (missingLevels.Count > 0)
                {
                    player.SendErrorMessage(
                        $"You can't unlock the {klass} class, as you haven't reached " +
                        $"{string.Join(", ", missingLevels.Select(l => $"{l} {l.Class}"))}");
                    return;
                }

                var missingPermissions = klass.PrerequisitePermissions.Where(p => !player.HasPermission(p)).ToList();
                if (missingPermissions.Count > 0)
                {
                    player.SendErrorMessage(
                        $"You can't unlock the {klass} class, as you don't have the " +
                        $"{string.Join(", ", missingPermissions)} permission(s).");
                    return;
                }
								
				if( klass.CostCurrency != null && klass.Cost > 0)
                {
					player.SendInfoMessage($"It costs [c/{Color.OrangeRed.Hex3()}:{klass.CostString}] to unlock the {klass} class.");
                    player.SendInfoMessage("Do you wish to proceed? Type /yes or /no.");
                    player.AddResponse("yes", args2 =>
                    {
                        player.AwaitingResponse.Remove("no");
												
						var bankAccount = BankingPlugin.Instance.GetBankAccount(player, klass.CostCurrency.InternalName);
						if (bankAccount == null || bankAccount.Balance < klass.Cost)
                        {
                            player.SendErrorMessage( $"Insufficient funds to unlock the {klass} class.");
                            return;
                        }

						if(bankAccount.TryTransferTo(BankingPlugin.Instance.GetBankAccount("Server", klass.CostCurrency.InternalName), klass.Cost))
						{
							session.UnlockClass(klass);
							session.Class = klass;
							player.SendSuccessMessage($"Changed to the {klass} class.");
						}
						else
						{
							player.SendErrorMessage($"Currency transfer failed.");
						}
	                });
                    player.AddResponse("no", args2 =>
                    {
                        player.AwaitingResponse.Remove("yes");
                        player.SendInfoMessage($"Canceled unlocking the {klass} class.");
                    });
                    return;
                }

                session.UnlockClass(klass);
                session.Class = klass;
                player.SendSuccessMessage($"Changed to the {klass} class.");
            }
        }

        private void Exp(CommandArgs args)
        {
            var player = args.Player;
            var session = GetOrCreateSession(player);
            var level = session.Level;
            player.SendInfoMessage($"You are currently a {level} {session.Class}.");
            if (level.ExpRequired > 0)
            {
                player.SendInfoMessage($"EXP: [c/{Color.LimeGreen.Hex3()}:{session.Exp}/{level.ExpRequired}]");
            }
        }

        internal Session GetOrCreateSession(TSPlayer player)
        {
            var session = player.GetData<Session>(SessionKey);
			if (session == null)
            {
				var username = player.User?.Name ?? player.Name;
				
				//first try the database
				SessionDefinition definition = SessionRepository.Load(username);

				//otherwise we need to create
				if(definition==null)
				{
                    definition = new SessionDefinition();
					definition.initialize();
                }

                session = new Session(player, definition);
                session.Resolve(_classes);
                player.SetData(SessionKey, session);
            }
            return session;
        }
		
		private void GiveExp(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}giveexp <player-name> <exp-amount>");
                return;
            }

            var inputPlayerName = parameters[0];
            var players = TShock.Utils.FindPlayer(inputPlayerName);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                return;
            }
            if (players.Count > 1)
            {
                player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var inputExpAmount = parameters[1];
            if (!long.TryParse(inputExpAmount, out var expAmount) || expAmount == 0)
            {
                player.SendErrorMessage($"Invalid EXP amount '{inputExpAmount}'.");
                return;
            }

            var otherPlayer = players[0];
            var session = GetOrCreateSession(otherPlayer);

            player.SendSuccessMessage($"Gave {otherPlayer.Name} {expAmount} EXP.");
            otherPlayer.SendInfoMessage(expAmount > 0
                                            ? $"You gained [c/{Color.LimeGreen.Hex3()}:{expAmount} EXP]."
                                            : $"You lost [c/{Color.OrangeRed.Hex3()}:{-expAmount} EXP].");
            session.AddExpToReport(expAmount);
            session.GiveExp(expAmount);
        }

        private void GiveOnce(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count < 2 || parameters.Count > 4)
            {
                player.SendErrorMessage(
                    $"Syntax: {Commands.Specifier}giveonce <player-name> <item-name> [stack] [prefix]");
                return;
            }

            var inputPlayerName = parameters[0];
            var players = TShock.Utils.FindPlayer(inputPlayerName);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                return;
            }
            if (players.Count > 1)
            {
                player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var inputItemName = parameters[1];
            var items = TShock.Utils.GetItemByIdOrName(inputItemName);
            if (items.Count == 0)
            {
                player.SendErrorMessage($"Invalid item '{inputItemName}'.");
                return;
            }
            if (items.Count > 1)
            {
                player.SendErrorMessage($"Multiple items matched '{inputItemName}':");
                TShock.Utils.SendMultipleMatchError(player, items);
                return;
            }

            var item = items[0];
            var inputStack = parameters.Count > 2 ? parameters[2] : item.maxStack.ToString();
            if (!int.TryParse(inputStack, out var stack) || stack <= 0 || stack > item.maxStack)
            {
                player.SendErrorMessage($"Invalid stack '{inputStack}'.");
                return;
            }

            var prefix = 0;
            if (parameters.Count > 3)
            {
                var inputPrefix = parameters[3];
                var prefixes = TShock.Utils.GetPrefixByIdOrName(inputPrefix);
                if (prefixes.Count == 0)
                {
                    player.SendErrorMessage($"Invalid prefix '{inputPrefix}'.");
                    return;
                }
                if (prefixes.Count > 1)
                {
                    player.SendErrorMessage($"Multiple prefixes matched '{inputItemName}':");
                    TShock.Utils.SendMultipleMatchError(player, prefixes.Cast<object>());
                    return;
                }
                prefix = prefixes[0];
            }

            var otherPlayer = players[0];
            var session = GetOrCreateSession(otherPlayer);
            if (session.ItemIdsGiven.Contains(item.type))
            {
                player.SendErrorMessage(
                    $"{otherPlayer.Name} was already given [i/s{stack},p{prefix}:{item.type}].");
                return;
            }

            session.AddItemId(item.type);
            player.SendSuccessMessage($"Gave [i/s{stack},p{prefix}:{item.type}] to {otherPlayer.Name}.");
            otherPlayer.GiveItem(item.type, "", Player.defaultWidth, Player.defaultHeight, stack, prefix);
            otherPlayer.SendInfoMessage($"Received [i/s{stack},p{prefix}:{item.type}].");
        }

        private void KillNpc(NPC npc)
        {
            if (!_npcDamages.TryGetValue(npc, out var damages))
            {
                return;
            }
            _npcDamages.Remove(npc);

            if (npc.value <= 0.0 || npc.SpawnedFromStatue)
            {
                return;
            }

            Debug.Assert(damages.Count > 0, "Damages must not be empty.");

            var total = damages.Values.Sum();
            var config = Config.Instance;
            foreach (var kvp in damages)
            {
                Debug.Assert(kvp.Value > 0, "Damage must be positive.");

                var player = kvp.Key;
                var session = GetOrCreateSession(player);
                var expAmount = (long)Math.Round((double)kvp.Value / total *
                                                 config.NpcNameToExpReward.Get(npc.GivenOrTypeName, npc.lifeMax) *
                                                 (session.Class.ExpMultiplierOverride ?? 1.0) * config.ExpMultiplier);

				//DISABLED DURING CONVERSION!!
                //session.AddExpToReport(expAmount);
                //session.GiveExp(expAmount);
            }
        }

        private void LevelDown(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}leveldown <player-name>");
                return;
            }

            var inputPlayerName = parameters[0];
            var players = TShock.Utils.FindPlayer(inputPlayerName);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                return;
            }
            if (players.Count > 1)
            {
                player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var otherPlayer = players[0];
            var session = GetOrCreateSession(otherPlayer);
            if (session.LevelDown())
            {
                player.SendSuccessMessage($"Leveled down {otherPlayer.Name}.");
                otherPlayer.SendInfoMessage("You have been leveled down.");
            }
            else
            {
                player.SendErrorMessage($"{otherPlayer.Name} could not be leveled down.");
            }
        }

        private void LevelUp(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}levelup <player-name>");
                return;
            }

            var inputPlayerName = parameters[0];
            var players = TShock.Utils.FindPlayer(inputPlayerName);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                return;
            }
            if (players.Count > 1)
            {
                player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var otherPlayer = players[0];
            var session = GetOrCreateSession(otherPlayer);
            if (session.LevelUp())
            {
                player.SendSuccessMessage($"Leveled up {otherPlayer.Name}.");
                otherPlayer.SendInfoMessage("You have been leveled up.");
            }
            else
            {
                player.SendErrorMessage($"{otherPlayer.Name} could not be leveled up.");
            }
        }

		private void LevelReset(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 1 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}levelreset <player-name>");
				return;
			}

			var inputPlayerName = parameters[0];
			var players = TShock.Utils.FindPlayer(inputPlayerName);
			if( players.Count == 0 )
			{
				player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
				return;
			}
			if( players.Count > 1 )
			{
				player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
				TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var otherPlayer = players[0];
			var session = GetOrCreateSession(otherPlayer);
			
			session.LevelReset();
			player.SendSuccessMessage($"Reset level for {otherPlayer.Name}.");
			otherPlayer.SendInfoMessage("Your level has been reset.");
		}
		
        private void OnGameUpdate(EventArgs args)
        {
            foreach (var player in TShock.Players.Where(p => p?.Active == true))
            {
                var session = GetOrCreateSession(player);
                session.Update();
            }
        }

        private void OnNetGetData(GetDataEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

      //      if (args.MsgID == PacketTypes.NpcItemStrike || args.MsgID == PacketTypes.NpcStrike)
      //      {
      //          var player = TShock.Players[args.Msg.whoAmI];
      //          using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
      //          {
      //              var npcIndex = reader.ReadInt16();

      //              void DoStrike(double damage, bool isCritical)
      //              {
      //                  if (damage < 1.0)
      //                  {
      //                      return;
      //                  }

      //                  var npc = Main.npc[npcIndex];
      //                  var defense = npc.defense;
      //                  defense -= npc.ichor ? 20 : 0;
      //                  defense -= npc.betsysCurse ? 40 : 0;
      //                  defense = Math.Max(0, defense);

      //                  damage = Main.CalculateDamage((int)damage, defense);
      //                  damage *= isCritical ? 2.0 : 1.0;
      //                  damage *= Math.Max(1.0, npc.takenDamageMultiplier);

      //                  var damages = _npcDamages.GetOrCreateValue(npc);
      //                  damages[player] = damages.Get(player) + (int)damage;

						////Debug.Print($"Leveling - DoStrike! Damage: {damage}, Critical: {isCritical}");

						//if (npc.life <= damage)
      //                  {
      //                      KillNpc(npc);
      //                  }
      //              }

      //              if (args.MsgID == PacketTypes.NpcItemStrike)
      //              {
      //                  DoStrike(player.SelectedItem.damage, false);
      //              }
      //              else
      //              {
      //                  var damage = reader.ReadInt16();
      //                  reader.ReadSingle();
      //                  reader.ReadByte();
      //                  var isCritical = reader.ReadByte() == 1;
      //                  DoStrike(damage, isCritical);
      //              }
      //          }
      //      }
            //else if (args.MsgID == PacketTypes.PlayerDeathV2)
			if( args.MsgID == PacketTypes.PlayerDeathV2 )
			{
                var player = TShock.Players[args.Msg.whoAmI];
                var session = GetOrCreateSession(player);
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    reader.ReadByte();
                    var deathReason = PlayerDeathReason.FromReader(reader);
                    reader.ReadInt16();
                    reader.ReadByte();
                    var wasPvP = ((BitsByte)reader.ReadByte())[0];
                    if (wasPvP)
                    {
                        var otherPlayer = deathReason.SourcePlayerIndex >= 0
                            ? TShock.Players[deathReason.SourcePlayerIndex]
                            : null;
                        if (otherPlayer == player)
                        {
                            return;
                        }

                        var expLoss = (long)Math.Round(Math.Max(
                                                           Config.Instance.DeathPenaltyPvPMultiplier * session.Exp,
                                                           Config.Instance.DeathPenaltyMinimum));
                        session.GiveExp(-expLoss);
                        session.AddExpToReport(-expLoss);

                        if (otherPlayer != null)
                        {
                            var otherSession = GetOrCreateSession(otherPlayer);
                            otherSession.GiveExp(expLoss);
                            otherSession.AddExpToReport(expLoss);
                        }
                    }
                    else
                    {
                        var expLoss = (long)Math.Round(Math.Max(
                                                           Config.Instance.DeathPenaltyMultiplier *
                                                           (session.Class.DeathPenaltyMultiplierOverride ?? 1.0) *
                                                           session.Exp,
                                                           Config.Instance.DeathPenaltyMinimum));
                        session.GiveExp(-expLoss);
                        session.AddExpToReport(-expLoss);
                    }
                }
            }
        }

        //private void OnNpcKilled(NpcKilledEventArgs args)
        //{
        //    KillNpc(args.npc);
        //}

        private void OnPlayerChat(PlayerChatEventArgs args)
        {
            var player = args.Player;
            if (player.HasPermission("leveling.noprefix"))
            {
                return;
            }

            var session = GetOrCreateSession(player);
            args.TShockFormattedText = string.Format(TShock.Config.ChatFormat, player.Group.Name,
                                                     player.Group.Prefix + session.Level.Prefix, player.Name,
                                                     player.Group.Suffix,
                                                     args.RawText);
        }

        private void OnPlayerPermission(PlayerPermissionEventArgs args)
        {
            var player = args.Player;
            if (!player.RealPlayer)
            {
                return;
            }

            var session = GetOrCreateSession(args.Player);
            args.Handled |= session.PermissionsGranted.Contains(args.Permission);
        }
				
		private void OnServerLeave(LeaveEventArgs args)
        {
            if (args.Who < 0 || args.Who >= Main.maxPlayers)
            {
                return;
            }

            var player = TShock.Players[args.Who];
            if (player != null)
            {
                var session = GetOrCreateSession(player);
                session.Save();
            }
        }

        private void SendTo(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count < 3)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}sendto <player-name> <rrr,ggg,bbb> <text>");
                return;
            }

            var inputPlayerName = parameters[0];
            var players = TShock.Utils.FindPlayer(inputPlayerName);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                return;
            }
            if (players.Count > 1)
            {
                player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var inputRgb = parameters[1];
            var inputRgbComponents = inputRgb.Split(',');
            if (inputRgbComponents.Length != 3 || !byte.TryParse(inputRgbComponents[0], out var r) ||
                !byte.TryParse(inputRgbComponents[1], out var g) || !byte.TryParse(inputRgbComponents[2], out var b))
            {
                player.SendErrorMessage($"Invalid RGB components '{inputRgb}'.");
                return;
            }

            var inputText = string.Join(" ", parameters.Skip(2));
            players[0].SendMessage(inputText, r, g, b);
        }

        private void SetClass(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}setclass <player-name> <class-name>");
                return;
            }

            var inputPlayerName = parameters[0];
            var players = TShock.Utils.FindPlayer(inputPlayerName);
            if (players.Count == 0)
            {
                player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                return;
            }
            if (players.Count > 1)
            {
                player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                TShock.Utils.SendMultipleMatchError(player, players);
                return;
            }

            var inputClassName = parameters[1];
            var @class = _classes.FirstOrDefault(
                c => c.DisplayName.Equals(inputClassName, StringComparison.OrdinalIgnoreCase));
            if (@class == null)
            {
                player.SendErrorMessage($"Invalid class '{inputClassName}'.");
                return;
            }

            var otherPlayer = players[0];
            var session = GetOrCreateSession(otherPlayer);
            if (!session.UnlockedClasses.Contains(@class))
            {
                session.UnlockClass(@class);
            }
            session.Class = @class;
            player.SendSuccessMessage($"Set {otherPlayer.Name}'s class to {@class}.");
            otherPlayer.SendInfoMessage($"You have been set to the {@class} class.");
        }

		private void LevelDump(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;

			var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
			var fileName = $"{player.Name}-{timeStamp}.json";

			fileName = Path.Combine("leveling", fileName);

			var session = GetOrCreateSession(player);
			var json = JsonConvert.SerializeObject(session._definition, Formatting.Indented);

			File.WriteAllText(fileName, json);
			
			player.SendErrorMessage($"Dumped leveling info to file '{fileName}'.");
		}
	}
}
