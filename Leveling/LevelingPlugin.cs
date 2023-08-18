using Banking;
using Corruption.PluginSupport;
using Leveling.Classes;
using Leveling.Database;
using Leveling.Levels;
//using Leveling.LoaderDsl;
using Leveling.Sessions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Leveling
{
	[ApiVersion(2, 1)]
	public sealed partial class LevelingPlugin : TerrariaPlugin
	{
		public override string Name => "Leveling";
		public override string Description => "Provides RPG-styled leveling and classes.";
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
		public override string Author => "MarioE, Timothy Barela";

		private static readonly string ConfigPath = Path.Combine("leveling", "config.json");
		/// <summary>
		/// The prefix used for BankAccounts created by this plugin.
		/// </summary>
		public const string BankAccountNamePrefix = "Exp_";
		internal const string SessionKey = "Leveling_Session";

		public static LevelingPlugin Instance { get; private set; }
		public static readonly Dictionary<string, Level> ItemNameToLevelRequirements = new Dictionary<string, Level>();
		internal ISessionDatabase SessionRepository;
		private readonly ConditionalWeakTable<NPC, Dictionary<TSPlayer, int>> _npcDamages = new ConditionalWeakTable<NPC, Dictionary<TSPlayer, int>>();
		private List<ClassDefinition> _classDefinitions;
		internal List<Class> _classes;

		public LevelingPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			PlayerHooks.PlayerChat += OnPlayerChat;
			PlayerHooks.PlayerPermission += OnPlayerPermission;
			ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			//ServerApi.Hooks.NetGetData.Register(this, OnNetGetData, int.MinValue);
			//ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
			ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
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
				ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
			}

			base.Dispose(disposing);
		}

		private void OnGamePostInitialize(EventArgs args) => OnLoad();

		private void OnReload(ReloadEventArgs args)
		{
			OnLoad();

			// We have to resolve sessions again.
			foreach (var session in TShock.Players.Where(p => p?.Active == true).Select(GetOrCreateSession))
			{
				session.Resolve(_classes);
			}

			args.Player.SendSuccessMessage("[Leveling] Reloaded config!");
		}

		private void InitializeBanking()
		{
			if (BankingPlugin.Instance == null)
			{
				//ServerApi.LogWriter.PluginWriteLine(LevelingPlugin.Instance, $"Error: Unable to retrieve BankingPlugin.Instance", TraceLevel.Error);
				throw new Exception($"Unable to retrieve BankingPlugin.Instance.");
			}

			var bank = BankingPlugin.Instance.Bank;

			foreach (var currency in bank.CurrencyManager)
			{
				currency.PreReward += OnCurrencyPreReward;
			}

			bank.AccountDeposit += Session.OnBankAccountBalanceChanged;
			bank.AccountWithdraw += Session.OnBankAccountBalanceChanged;
		}

		private void OnCurrencyPreReward(object sender, RewardEventArgs e)
		{
			var session = TryGetOrCreateSession(e.PlayerName);

			if (session != null)
			{
				if (session.Class.LevelingCurrency == e.Currency)
				{
					//originally from KillNpc() 
					//var expAmount = (long)Math.Round((double)kvp.Value / total *
					//							config.NpcNameToExpReward.Get(npc.GivenOrTypeName, npc.lifeMax) *
					//							( session.Class.ExpMultiplierOverride ?? 1.0 ) * config.ExpMultiplier);

					//TODO Start adapting old npc exp overrides... but this wont really work as is -- we need an overhaul
					//if(e.RewardReason == RewardReason.Killing)
					//{
					//	var expValues = session.Class.Definition.ParsedNpcNameToExpValues;
					//	//var result = rewardValue;
					//	var killReward = (KillingReward)e.Reward;

					//	if( expValues.TryGetValue(killReward.NpcGivenOrTypeName, out var newValue) == true )
					//	{
					//		Debug.Print($"ClassExpReward adjusted to {newValue}(was {e.RewardValue}).");
					//		e.RewardValue = newValue;
					//	}
					//}

					e.RewardValue = e.RewardValue *
										(decimal)((session.Class.ExpMultiplierOverride ?? 1.0) * Config.Instance.ExpMultiplier);
				}
			}
		}

		private void OnLoad()
		{
			const string classDirectory = "leveling";

			Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);

			var dbConfig = Config.Instance.DatabaseConfig;
			SessionRepository = SessionDatabaseFactory.LoadOrCreateDatabase(dbConfig.DatabaseType, dbConfig.ConnectionString);

			InitializeBanking();

			Directory.CreateDirectory(classDirectory);

			var classDefs = ClassDefinition.Load(classDirectory);

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
			foreach (var @class in _classes)
			{
				@class.Resolve(levels, 0);
			}
			foreach (var @class in _classes)
			{
				@class.Resolve(levels, 1);
			}
			foreach (var level in levels)
			{
				foreach (var itemName in level.ItemNamesAllowed)
				{
					ItemNameToLevelRequirements[itemName] = level;
				}
			}
		}

		internal Session TryGetOrCreateSession(string playerName)
		{
			var tsPlayer = TShock.Utils.FindPlayer(playerName).FirstOrDefault();

			if (tsPlayer != null && tsPlayer.Active)
				return GetOrCreateSession(tsPlayer);
			else
				return null;
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
				if (definition == null)
				{
					definition = new SessionDefinition();
					definition.Initialize();
				}

				session = new Session(player, definition);
				session.Resolve(_classes);
				player.SetData(SessionKey, session);
			}
			return session;
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
			if (args.MsgID == PacketTypes.PlayerDeathV2)
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

		//mystery method. why is this here? I noticed this on the callstack when debugging permissions in CustomSkillsPlugin...-Tim
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

		//we handle the join event so that we can ensure were creating sessions at this point, and not during runtime.
		private void OnServerJoin(JoinEventArgs args)
		{
			if (args.Who < 0 || args.Who >= Main.maxPlayers)
			{
				return;
			}

			var player = TShock.Players[args.Who];
			if (player != null)
			{
				var session = GetOrCreateSession(player);
				//session.Save();
			}
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
	}
}
