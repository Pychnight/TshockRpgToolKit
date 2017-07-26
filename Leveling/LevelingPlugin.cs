using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Leveling.Classes;
using Leveling.Levels;
using Leveling.Sessions;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace Leveling
{
    [ApiVersion(2, 1)]
    public sealed class LevelingPlugin : TerrariaPlugin
    {
        private const string SessionKey = "Leveling_Session";

        public static readonly Dictionary<string, Level> ItemNameToLevelRequirements = new Dictionary<string, Level>();

        private static readonly string ConfigPath = Path.Combine("leveling", "config.json");

        private readonly ConditionalWeakTable<NPC, Dictionary<TSPlayer, int>> _npcDamages =
            new ConditionalWeakTable<NPC, Dictionary<TSPlayer, int>>();

        private List<ClassDefinition> _classDefinitions;
        private List<Class> _classes;

        public LevelingPlugin(Main game) : base(game)
        {
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif
        }

        public override string Author => "MarioE";
        public override string Description => "Provides RPG-styled leveling and classes.";
        public override string Name => "Leveling";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override void Initialize()
        {
            Directory.CreateDirectory("leveling");
            if (File.Exists(ConfigPath))
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            _classDefinitions = Directory.EnumerateFiles("leveling", "*.class", SearchOption.AllDirectories)
                .Select(p => JsonConvert.DeserializeObject<ClassDefinition>(File.ReadAllText(p))).ToList();
            _classes = _classDefinitions.Select(cd => new Class(cd)).ToList();
            foreach (var @class in _classes)
            {
                @class.Resolve(_classes);
            }

            foreach (var level in _classes.SelectMany(c => c.Levels))
            {
                foreach (var itemName in level.ItemNamesAllowed)
                {
                    ItemNameToLevelRequirements[itemName] = level;
                }
            }

            GeneralHooks.ReloadEvent += OnReload;
            PlayerHooks.PlayerChat += OnPlayerChat;
            PlayerHooks.PlayerPermission += OnPlayerPermission;
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            ServerApi.Hooks.NetGetData.Register(this, OnNetGetData, int.MinValue);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
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
            Commands.ChatCommands.Add(new Command("leveling.sendto", SendTo, "sendto")
            {
                HelpText = $"Syntax: {Commands.Specifier}sendto <player-name> <rrr,ggg,bbb> <text>\n" +
                           "Sends text in a certain color to the specified player."
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));
                foreach (var session in TShock.Players.Where(p => p?.Active == true).Select(GetOrCreateSession))
                {
                    session.Save();
                }

                GeneralHooks.ReloadEvent -= OnReload;
                PlayerHooks.PlayerChat -= OnPlayerChat;
                PlayerHooks.PlayerPermission -= OnPlayerPermission;
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            }

            base.Dispose(disposing);
        }

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
            TSPlayer.All.SendData(PacketTypes.PlayerHp, "", otherPlayer.Index);

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
                var newClasses = _classes.Except(session.UnlockedClasses).Where(
                    c => !c.PrerequisiteClasses.Except(session.MasteredClasses).Any()).ToList();
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
                var @class = _classes.First(
                    c => string.Equals(c.DisplayName, inputClassName, StringComparison.OrdinalIgnoreCase));
                if (@class == null)
                {
                    player.SendErrorMessage($"Invalid class '{inputClassName}'.");
                    return;
                }

                if (!Config.Instance.AllowSwitchingClassesMidClass && !session.MasteredClasses.Contains(session.Class))
                {
                    player.SendInfoMessage("You can't switch classes until you've mastered your current one.");
                    return;
                }

                if (session.UnlockedClasses.Contains(@class))
                {
                    session.Class = @class;
                    player.SendSuccessMessage($"Changed to the {@class} class.");
                    return;
                }

                var missingClasses = @class.PrerequisiteClasses.Except(session.MasteredClasses).ToList();
                if (missingClasses.Count > 0)
                {
                    player.SendErrorMessage(
                        $"You can't unlock {@class}, as you haven't mastered {string.Join(", ", missingClasses)}.");
                    return;
                }

                if (@class.SEconomyCost > 0)
                {
                    var cost = new Money(@class.SEconomyCost);
                    player.SendInfoMessage(
                        $"It costs [c/{Color.OrangeRed.Hex3()}:{cost}] to unlock the {@class} class.");
                    player.SendInfoMessage("Do you wish to proceed? Type /yes or /no.");
                    player.AddResponse("yes", args2 =>
                    {
                        player.AwaitingResponse.Remove("no");
                        var bankAccount = SEconomyPlugin.Instance?.GetBankAccount(player);
                        if (bankAccount == null || bankAccount.Balance < cost)
                        {
                            player.SendErrorMessage(
                                $"You do not have enough of a balance to unlock the {@class} class.");
                            return;
                        }

                        bankAccount.TransferTo(SEconomyPlugin.Instance.WorldAccount, cost,
                                               BankAccountTransferOptions.IsPayment, $"Unlocking the {@class} class",
                                               $"Unlocking the {@class} class.");
                        session.UnlockClass(@class);
                        session.Class = @class;
                        player.SendSuccessMessage($"Changed to the {@class} class.");
                    });
                    player.AddResponse("no", args2 =>
                    {
                        player.AwaitingResponse.Remove("yes");
                        player.SendInfoMessage($"Canceled unlocking the {@class} class.");
                    });
                    return;
                }

                session.UnlockClass(@class);
                session.Class = @class;
                player.SendSuccessMessage($"Changed to the {@class} class.");
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

        private Session GetOrCreateSession(TSPlayer player)
        {
            var session = player.GetData<Session>(SessionKey);
            if (session == null)
            {
                var username = player.User?.Name ?? player.Name;
                var sessionPath = Path.Combine("leveling", $"{username}.session");
                SessionDefinition definition;
                if (File.Exists(sessionPath))
                {
                    definition = JsonConvert.DeserializeObject<SessionDefinition>(File.ReadAllText(sessionPath));
                }
                else
                {
                    definition = new SessionDefinition();
                    var defaultClassName = Config.Instance.DefaultClassName;
                    definition.ClassNameToExp[defaultClassName] = 0;
                    definition.ClassNameToLevelName[defaultClassName] =
                        _classes.First(c => c.Name == defaultClassName).Levels[0].Name;
                    definition.CurrentClassName = defaultClassName;
                    definition.UnlockedClassNames.Add(defaultClassName);
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
            if (!int.TryParse(inputExpAmount, out var expAmount) || expAmount == 0)
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

            Debug.Assert(damages.Count > 0, "Damages must not be empty.");

            var total = damages.Values.Sum();
            var config = Config.Instance;
            foreach (var kvp in damages)
            {
                Debug.Assert(kvp.Value > 0, "Damage must be positive.");

                var expAmount = (double)kvp.Value / total * config.ExpMultiplier *
                                config.NpcNameToExpReward.Get(npc.GivenOrTypeName, npc.lifeMax);
                var player = kvp.Key;
                var session = GetOrCreateSession(player);
                session.AddExpToReport((int)expAmount);
                session.GiveExp((int)expAmount);
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
            }
            else
            {
                player.SendErrorMessage($"{otherPlayer.Name} could not be leveled up.");
            }
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
            if (args.Handled || args.MsgID != PacketTypes.NpcItemStrike && args.MsgID != PacketTypes.NpcStrike)
            {
                return;
            }

            var player = TShock.Players[args.Msg.whoAmI];
            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                var npcIndex = reader.ReadInt16();

                void DoStrike(double damage, bool isCritical)
                {
                    if (damage < 1.0)
                    {
                        return;
                    }

                    var npc = Main.npc[npcIndex];
                    var defense = npc.defense;
                    defense -= npc.ichor ? 20 : 0;
                    defense -= npc.betsysCurse ? 40 : 0;
                    defense = Math.Max(0, defense);

                    damage = Main.CalculateDamage((int)damage, defense);
                    damage *= isCritical ? 2.0 : 1.0;
                    damage *= Math.Max(1.0, npc.takenDamageMultiplier);

                    var damages = _npcDamages.GetOrCreateValue(npc);
                    damages[player] = damages.Get(player) + (int)damage;

                    if (npc.life <= damage)
                    {
                        KillNpc(npc);
                    }
                }

                if (args.MsgID == PacketTypes.NpcItemStrike)
                {
                    DoStrike(player.SelectedItem.damage, false);
                }
                else
                {
                    var damage = reader.ReadInt16();
                    reader.ReadSingle();
                    reader.ReadByte();
                    var isCritical = reader.ReadByte() == 1;
                    DoStrike(damage, isCritical);
                }
            }
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            KillNpc(args.npc);
        }

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

        private void OnReload(ReloadEventArgs args)
        {
            if (File.Exists(ConfigPath))
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            _classDefinitions = Directory.EnumerateFiles("leveling", "*.class", SearchOption.AllDirectories)
                .Select(p => JsonConvert.DeserializeObject<ClassDefinition>(File.ReadAllText(p))).ToList();
            _classes = _classDefinitions.Select(cd => new Class(cd)).ToList();
            foreach (var @class in _classes)
            {
                @class.Resolve(_classes);
            }

            ItemNameToLevelRequirements.Clear();
            foreach (var level in _classes.SelectMany(c => c.Levels))
            {
                foreach (var itemName in level.ItemNamesAllowed)
                {
                    ItemNameToLevelRequirements[itemName] = level;
                }
            }

            // We have to resolve sessions again.
            foreach (var session in TShock.Players.Where(p => p?.Active == true).Select(GetOrCreateSession))
            {
                session.Resolve(_classes);
            }

            args.Player.SendSuccessMessage("[Leveling] Reloaded config!");
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
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
    }
}
