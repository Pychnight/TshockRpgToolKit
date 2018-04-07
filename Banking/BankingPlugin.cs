using Banking.Configuration;
using Banking.Rewards;
using Corruption;
using Corruption.PluginSupport;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Banking
{
	[ApiVersion(2, 1)]
	public sealed class BankingPlugin : TerrariaPlugin
	{
		public override string Author => "Timothy Barela";
		public override string Description => "A simple, banking and currency system for TShock.";
		public override string Name => "Banking";
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
		
		private static string DataDirectory { get; set; } = "banking";
		private static string ConfigPath => Path.Combine(DataDirectory, "config.json");

		public static BankingPlugin Instance { get; private set; }
				
		//public event EventHandler RewardDepositing;

		internal CombatTextDistributor CombatTextDistributor;
		public Bank Bank { get; internal set; }
		internal NpcStrikeTracker NpcStrikeTracker;
		public RewardDistributor RewardDistributor { get; private set; }
		internal VoteChecker VoteChecker { get; set; }
				
		//public BankAccount WorldAccount { get { return BankAccountManager.WorldAccount; } }
		
		public BankingPlugin(Main game) : base(game)
		{
#if DEBUG
			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif
			Instance = this;
		}

		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			
			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			ServerApi.Hooks.NetGetData.Register(this, OnNetGetData);
			ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);
			ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
			ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
			//ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
			ServerApi.Hooks.WorldSave.Register(this, OnWorldSave);
									
			Commands.ChatCommands.Add(new Command("banking.bank", BankCommands.Bank, "bank")
			{
				HelpText = $"Syntax: {Commands.Specifier}bank bal <currency>\n" +
									$"{Commands.Specifier}bank pay <currency> <player> <amount>\n" +
									$"{Commands.Specifier}bank list\n"
			});
			Commands.ChatCommands.Add(new Command("banking.admin", BankCommands.BankAdmin, "bankadmin")
			{
				HelpText = $"Syntax: {Commands.Specifier}bank bal <currency> <player>\n" +
									$"{Commands.Specifier}bank set <currency> <player> <amount>\n" +
									$"{Commands.Specifier}bank give <currency> <player> <amount>\n" +
									$"{Commands.Specifier}bank take <currency> <player> <amount>\n" +
									$"{Commands.Specifier}bank reset <currency> <player>\n"
			});
			Commands.ChatCommands.Add(new Command("banking.multiplier", BankCommands.Multiplier, "multiplier")
			{
				HelpText = $"Syntax: {Commands.Specifier}multiplier <currency> <gain|death|deathpvp> <value>\n" +
						   "Sets multipliers for gains and penalties, per Currency."
			});
			Commands.ChatCommands.Add(new Command("banking.reward", BankCommands.Reward, "reward")
			{
				HelpText = $"Syntax: {Commands.Specifier}reward\n" +
						   "Reward players if they vote for the server."
			});
			
		}
		
		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				//JsonConfig.Save(this, Config.Instance, ConfigPath);
				//Bank.Save();
				
				GeneralHooks.ReloadEvent -= OnReload;
				//	PlayerHooks.PlayerChat -= OnPlayerChat;
				//	PlayerHooks.PlayerPermission -= OnPlayerPermission;
				ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
				ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
				ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
				ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
				ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
				//ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
				//ServerApi.Hooks.WorldSave.Deregister(this, OnWorldSave);
			}

			base.Dispose(disposing);
		}
		
		private void onLoad()
		{
			Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);

			if(Bank==null)
			{
				CombatTextDistributor = new CombatTextDistributor();
				Bank = new Bank();
				NpcStrikeTracker = new NpcStrikeTracker();
				NpcStrikeTracker.StruckNpcKilled += OnStruckNpcKilled;
				RewardDistributor = new RewardDistributor();
				VoteChecker = new VoteChecker();
			}

			NpcStrikeTracker.Clear();
			Bank.Load();
		}

		private void OnPostInitialize(EventArgs args)
		{
			onLoad();
		}

		private void OnReload(ReloadEventArgs e)
		{
			onLoad();
		}
		
		private void OnWorldSave(WorldSaveEventArgs args)
		{
			BankAccount.UpdateAccounts();
		}

		private void OnServerJoin(JoinEventArgs args)
		{
			var player = new TSPlayer(args.Who);
			Bank.EnsureBankAccountsExist(player.Name);
		}

		private void OnNetGetData(GetDataEventArgs args)
		{
			if( args.Handled )
				return;

			switch(args.MsgID)
			{
				case PacketTypes.Tile:
					using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
					{
						var action = reader.ReadByte();

						if(action==0 || action==1)//0 kill, 1 place
						{
							var tileX = reader.ReadInt16();
							var tileY = reader.ReadInt16();
							var var1 = reader.ReadInt16();//kill tile status
							var var2 = reader.ReadInt16();//place tile

							//Debug.Print($"action: {action}");
							//Debug.Print($"tileX: {tileX}");
							//Debug.Print($"tileY: {tileY}");
							//Debug.Print($"var1: {var1}");
							//Debug.Print($"var2: {var2}");

							if( action==0 && var1 == 0 )
							{
								//kill tile
								var player = TShock.Players[args.Msg.whoAmI];
								var tile = Main.tile[tileX, tileY];
								
								//ignore walls and grass
								if(tile.collisionType>0)
								{
									//player.TilesDestroyed.Add(key, tile);
									OnTileKilled(new TileChangedEventArgs(player, tileX, tileY, tile.type));
									return;
								}
							}
							else if(action==1)// && var1 > 0)
							{
								//place tile
								var player = TShock.Players[args.Msg.whoAmI];
								var tile = Main.tile[tileX, tileY];
								
								OnTilePlaced(new TileChangedEventArgs(player, tileX, tileY, tile.type));
								return;
							}
						}
					}

					break;

				case PacketTypes.PlayerDeathV2:
					//based off of MarioE's original code from the Leveling plugin...
					using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
					{
						var player = TShock.Players[args.Msg.whoAmI];

						reader.ReadByte();
						var deathReason = PlayerDeathReason.FromReader(reader);
						reader.ReadInt16();
						reader.ReadByte();
						var wasPvP = ( (BitsByte)reader.ReadByte() )[0];
						if( wasPvP )
						{
							var otherPlayer = deathReason.SourcePlayerIndex >= 0
								? TShock.Players[deathReason.SourcePlayerIndex]
								: null;
							if( otherPlayer == player )
							{
								return;
							}

							if( otherPlayer != null )
							{
								//RewardDistributor.TryAddReward(otherPlayer.Name, RewardReason.DeathPvP, otherPlayer.Name);
								RewardDistributor.TryAddDeathPenalty(player.Name, RewardReason.DeathPvP, otherPlayer.Name);
							}
							else
							{
								//RewardDistributor.TryAddReward(player.Name, RewardReason.DeathPvP, "");
								RewardDistributor.TryAddDeathPenalty(player.Name, RewardReason.DeathPvP, "");
							}
						}
						else
						{
							//RewardDistributor.TryAddReward(player.Name, RewardReason.Death, "");
							RewardDistributor.TryAddDeathPenalty(player.Name, RewardReason.Death, "");
						}
					}

					break;
			}
		}
		
		private void OnGameUpdate(EventArgs args)
		{
			NpcStrikeTracker.OnGameUpdate();
			CombatTextDistributor.Send(400);
		}

		private void OnNpcStrike(NpcStrikeEventArgs args)
		{
			Debug.Print($"Banking - OnNpcStrike! Damage: {args.Damage}, Critical: {args.Critical}");

			var item = args.Player.HeldItem;
			//Debug.Print($"Strike NPC with {item.Name}!");
			NpcStrikeTracker.OnNpcStrike(args.Player, args.Npc, args.Damage, args.Critical, item.Name);
		}

		private void OnNpcKilled(NpcKilledEventArgs args)
		{
			Debug.Print($"NpcKilled! #{args.npc.whoAmI} - {args.npc.GivenOrTypeName}");
			Debug.Print($"Value: {args.npc.value}");
			//NpcStrikeTracker.OnNpcKilled(args.npc);

			Task.Run(() =>
			{
				//Debug.Print("Task.Run() => OnNpcKilled!");
				NpcStrikeTracker.OnNpcKilled(args.npc);
			});
		}

		private void OnStruckNpcKilled(object sender, StruckNpcKilledEventArgs args)
		{
			Debug.Print("OnStruckNpcKilled!");
			RewardDistributor.TryAddKillingReward(args.PlayerStrikeInfo, args.NpcGivenOrTypeName, args.NpcValue, args.NpcSpawnedFromStatue);
		}

		private void OnTileKilled(TileChangedEventArgs args)
		{
			Debug.Print("OnTileKilled!");

			var player = args.Player;
			var key = new Vector2(args.TileX, args.TileY);

			if(!canBuild(args.TileX,args.TileY,player))
			{
				Debug.Print("Cannot build here.");
				return;
			}

			if( !player.TilesDestroyed.TryGetValue(key, out var dummy) &&
				!player.TilesCreated.TryGetValue(key,out var dummy2) ) //dont gain from mining own placed tiles
			{
				var tile = Main.tile[args.TileX, args.TileY];

				//ignore walls and grass
				if( tile.collisionType > 0 )
				{
					player.TilesDestroyed.Add(key, tile);

					if( args.Player != null )
						RewardDistributor.TryAddReward(args.Player.Name, RewardReason.Mining, args.Type.ToString(), 1);//ideally we wont create strings, but for now...
				}
			}
			else
				Debug.Print("Already destroyed.");
		}
		
		private void OnTilePlaced(TileChangedEventArgs args)
		{
			Debug.Print("OnTilePlaced!");

			var player = args.Player;
			var key = new Vector2(args.TileX, args.TileY);

			if( !canBuild(args.TileX, args.TileY, player) )
			{
				Debug.Print("Cannot build here.");
				return;
			}

			if( !player.TilesCreated.TryGetValue(key, out var dummy) )
			{
				var tile = Main.tile[args.TileX, args.TileY];
				player.TilesCreated.Add(key, tile);

				if( args.Player != null )
					RewardDistributor.TryAddReward(args.Player.Name, RewardReason.Placing, args.Type.ToString(), 1);//ideally we wont create strings, but for now...
			}
			else
				Debug.Print("Already placed.");
		}

		private bool canBuild(int tileX, int tileY, TSPlayer player)
		{
			if( AreaFunctions.InSpawn(tileX, tileY) )
				return false;
			
			return TShock.Regions.CanBuild(tileX, tileY, player);
		}

		public BankAccount GetBankAccount(TSPlayer player, string accountType)
		{
			return Bank.GetBankAccount(player.Name,accountType);
		}

		public BankAccount GetBankAccount(string playerName, string accountName)
		{
			return Bank.GetBankAccount(playerName,accountName);
		}

		public PlayerBankAccountMap GetAllBankAccountsForPlayer(string playerName)
		{
			return Bank[playerName];
		}

		public IEnumerable<CurrencyDefinition> EnumerateCurrencies()
		{
			return Bank.CurrencyManager.AsEnumerable();
		}

		public bool TryGetCurrency(string currencyType, out CurrencyDefinition result)
		{
			return Bank.CurrencyManager.Definitions.TryGetValue(currencyType, out result);
		}
	}
}
