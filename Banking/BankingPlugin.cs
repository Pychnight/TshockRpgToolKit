using Banking.Configuration;
using Banking.Rewards;
using Banking.TileTracking;
using Corruption;
using Corruption.PluginSupport;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Concurrent;
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
		
		internal static string DataDirectory { get; set; } = "banking";
		internal static string ConfigPath => Path.Combine(DataDirectory, "config.json");

		public static BankingPlugin Instance { get; private set; }
		internal PlayerRewardNotificationDistributor PlayerRewardNotificationDistributor;
		public Bank Bank { get; internal set; }

		/// <summary>
		/// Gets a Dictionary that records spawned NPC's starting hit points.
		/// </summary>
		/// <remarks>This is exposed publicly for BankingPlugin interaction.</remarks>
		public ConcurrentDictionary<int,int> NpcSpawnHP { get; private set; }
		internal NpcStrikeTracker NpcStrikeTracker;
		internal PlayerFishingTracker PlayerFishingTracker;
		internal PlayerTileTracker PlayerTileTracker;
		internal PlayingRewardTracker PlayerSessionTracker;
		public RewardDistributor RewardDistributor { get; private set; }
		internal VoteChecker VoteChecker { get; set; }
				
		public BankingPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		public override void Initialize()
		{
			GeneralHooks.ReloadEvent += OnReload;
			
			ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			ServerApi.Hooks.NetGetData.Register(this, OnNetGetData);
			ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
			ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);
			ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
			ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
			ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
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
				ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
				ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
				ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
				ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
				ServerApi.Hooks.WorldSave.Deregister(this, OnWorldSave);
			}

			base.Dispose(disposing);
		}
		
		private void onLoad()
		{
			Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);

			try
			{
				Debug.Print($"Loading Bank.");
				
				if( Bank == null )
				{
					PlayerRewardNotificationDistributor = new PlayerRewardNotificationDistributor();
					Bank = new Bank();
					NpcSpawnHP = new ConcurrentDictionary<int, int>();
					NpcStrikeTracker = new NpcStrikeTracker();
					NpcStrikeTracker.StruckNpcKilled += OnStruckNpcKilled;
					PlayerFishingTracker = new PlayerFishingTracker();
					PlayerTileTracker = new PlayerTileTracker(DataDirectory);
					PlayerSessionTracker = new PlayingRewardTracker();
					RewardDistributor = new RewardDistributor();
					VoteChecker = new VoteChecker();
				}

				NpcStrikeTracker.Clear();
				Bank.Load();
			}
			catch(Exception ex)
			{
				this.LogPrint(ex.ToString(), TraceLevel.Error);
			}
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
			BankAccount.PersistDirtyAccounts();
		}

		private void OnServerJoin(JoinEventArgs args)
		{
			var player = TShock.Players[args.Who];
			Bank.EnsureBankAccountsExist(player.Name);
			PlayerSessionTracker.AddPlayer(player);
		}

		private void OnServerLeave(LeaveEventArgs args)
		{
			var player = new TSPlayer(args.Who);
			PlayerTileTracker.OnPlayerLeave(player.Name);
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

						if(action>=0 && action<= 3)//0 kill, 1 place tile, 2 kill, 3 place wall
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

							var player = TShock.Players[args.Msg.whoAmI];
							TileSubTarget tileSubTarget;

							if( action < 2 )
								tileSubTarget = TileSubTarget.Tile;
							else
								tileSubTarget = TileSubTarget.Wall;	

							if( ( action==0 || action == 2) && var1 == 0 )
							{
								var tile = Main.tile[tileX, tileY];

								//kill tile
								if(action==2 || tile.collisionType>0 ) //ignore grass
								{
									OnTileKilled(new TileChangedEventArgs(player, tileX, tileY, tile.type, tile.wall, tileSubTarget));
									return;
								}
							}
							else if(action==1 || action==3)// && var1 > 0)
							{
								//var1 should hold the type of the tile or wall we placed.
								
								//place tile
								OnTilePlaced(new TileChangedEventArgs(player, tileX, tileY, (ushort)var1, (byte)var1, tileSubTarget));
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
								var reward = new DeathReward(player.Name,RewardReason.DeathPvP, otherPlayer.Name);
								RewardDistributor.EnqueueReward(reward);

							}
							else
							{
								var reward = new DeathReward(player.Name, RewardReason.DeathPvP, "");
								RewardDistributor.EnqueueReward(reward);
							}
						}
						else
						{
							var reward = new DeathReward(player.Name, RewardReason.Death, "");
							RewardDistributor.EnqueueReward(reward);

						}
					}

					break;
				
				case PacketTypes.ProjectileNew:
					//Debug.Print("ProjectileNew!");

					using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
					{
						var projectileId = reader.ReadInt16();
						reader.ReadSingle();
						reader.ReadSingle();
						reader.ReadSingle();
						reader.ReadSingle();
						reader.ReadSingle();
						reader.ReadInt16();
						var playerId = reader.ReadByte();
						var type = reader.ReadInt16();
						//var aiFlags = reader.ReadByte();
						//var ai0 = reader.ReadSingle();
						//var ai1 = reader.ReadSingle();
						//var projUUID = reader.ReadSingle();

						PlayerFishingTracker.TryBeginFishing(playerId, projectileId, type);
					}

					break;

				case PacketTypes.ProjectileDestroy:
					//Debug.Print("ProjectileDestroy!");
					using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
					{
						var projectileId = reader.ReadInt16();
						var ownerId = reader.ReadByte();

						PlayerFishingTracker.TryEndFishing(ownerId, projectileId);
					}

					break;

				case PacketTypes.PlayerSlot:
					//Debug.Print("PlayerSlot!");
					using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
					{
						var playerId = reader.ReadByte();
						var slotId = reader.ReadByte();
						var stack = reader.ReadInt16();
						var prefix = reader.ReadByte();
						var itemId = reader.ReadInt16();

						if(PlayerFishingTracker.IsItemFromFishing(playerId))//, stack, prefix, itemId))
						{
							var player = TShock.Players[args.Msg.whoAmI];
							var reward = new FishingReward(player.Name, stack, prefix, itemId);
							RewardDistributor.EnqueueReward(reward);
						}
					}
					
					break;

				default:
					break;
			}
		}
		
		private void OnGameUpdate(EventArgs args)
		{
			NpcStrikeTracker.OnGameUpdate();
			PlayerFishingTracker.OnGameUpdate();
			PlayerSessionTracker.OnGameUpdate();
			RewardDistributor.OnGameUpdate();
			PlayerRewardNotificationDistributor.Send(400);
		}
		
		private void OnNpcSpawn(NpcSpawnEventArgs args)
		{
			var npc = Main.npc[args.NpcId];
			var life = npc.life; //...should we use npc.lifeMax?

			//this wont work here.. NpcManager hasn't added the npc to the custom npcs conditional weak table at the point
			//this is raised. So we reverse things... CustomNpcs plugin can now access NpcSpawnHP, and set it manually within. 
			//var customNpc = NpcManager.Instance?.GetCustomNpc(npc);
			//if( customNpc != null )
			//	life = customNpc.MaxHp;

			//Debug.Print("** Regular set life.");
			NpcSpawnHP[args.NpcId] = life;
		}

		private void OnNpcStrike(NpcStrikeEventArgs args)
		{
			//Debug.Print($"Banking - OnNpcStrike! Damage: {args.Damage}, Critical: {args.Critical}");

			var item = args.Player.HeldItem;
			//Debug.Print($"Strike NPC with {item.Name}!");
			NpcStrikeTracker.OnNpcStrike(args.Player, args.Npc, args.Damage, args.Critical, item.Name);
		}

		private void OnNpcKilled(NpcKilledEventArgs args)
		{
			//Debug.Print($"NpcKilled! #{args.npc.whoAmI} - {args.npc.GivenOrTypeName}");
			//Debug.Print($"Value: {args.npc.value}");
			
			if(!NpcSpawnHP.TryGetValue(args.npc.whoAmI,out var spawnHp))
			{
				throw new Exception("Unable to retrieve NpcSpawnHP!");
			}

			//Debug.Print($"NpcHP: {spawnHp}");
			//NpcStrikeTracker.OnNpcKilled(args.npc);

			Task.Run(() =>
			{
				//Debug.Print("Task.Run() => OnNpcKilled!");
				NpcStrikeTracker.OnNpcKilled(args.npc, spawnHp);
			});
		}

		private void OnStruckNpcKilled(object sender, StruckNpcKilledEventArgs args)
		{
			//Debug.Print("OnStruckNpcKilled!");
		
			var reward = new KillingReward(args.PlayerStrikeInfo, args.NpcGivenOrTypeName, args.NpcHitPoints, args.NpcSpawnedFromStatue);
			RewardDistributor.EnqueueReward(reward);
		}

		private void OnTileKilled(TileChangedEventArgs args)
		{
			//Debug.Print("OnTileKilled!");

			var player = args.Player;
			
			if(!CanBuild(args.TileX,args.TileY,player))
			{
				Debug.Print("Cannot build here.");
				return;
			}

			if( !PlayerTileTracker.HasModifiedTile(player.Name,args.TileX,args.TileY))
			{
				//ignore walls and grass.. this should be filtered already...
				//if( tile.collisionType > 0 )
				{
					PlayerTileTracker.ModifyTile(player.Name, args.TileX, args.TileY);

					if( player != null )
					{
						var reward = new MiningReward(player, args.GetTypeOrWall() , args.TileSubTarget, RewardReason.Mining);
						RewardDistributor.EnqueueReward(reward);
					}
				}
			}
			else
				Debug.Print("Already destroyed.");
		}
		
		private void OnTilePlaced(TileChangedEventArgs args)
		{
			//Debug.Print("OnTilePlaced!");

			var player = args.Player;
			
			if( !CanBuild(args.TileX, args.TileY, player) )
			{
				Debug.Print("Cannot build here.");
				return;
			}
			
			if(!PlayerTileTracker.HasModifiedTile(player.Name, args.TileX, args.TileY))
			{
				PlayerTileTracker.ModifyTile(player.Name, args.TileX, args.TileY);

				if( args.Player != null )
				{
					var reward = new MiningReward(args.Player, args.GetTypeOrWall(), args.TileSubTarget, RewardReason.Placing);
					RewardDistributor.EnqueueReward(reward);
				}
			}
			else
				Debug.Print("Already placed.");
		}

		private bool CanBuild(int tileX, int tileY, TSPlayer player)
		{
			if( AreaFunctions.InSpawn(tileX, tileY) )
				return false;
			
			return TShock.Regions.CanBuild(tileX, tileY, player);
		}

		public BankAccount GetBankAccount(TSPlayer player, string accountType)
		{
			return Bank.GetBankAccount(player.Name,accountType);
		}

		public BankAccount GetBankAccount(string playerName, string accountType)
		{
			return Bank.GetBankAccount(playerName,accountType);
		}
		
		public IEnumerable<BankAccount> GetAllBankAccountsForPlayer(string playerName)
		{
			var playerBankAccountMap = Bank[playerName];
			return playerBankAccountMap?.ToList() ?? new List<BankAccount>();
		}

		public IEnumerable<CurrencyDefinition> EnumerateCurrencies()
		{
			return Bank.CurrencyManager.AsEnumerable();
		}

		public bool TryGetCurrency(string currencyType, out CurrencyDefinition result)
		{
			result = Bank.CurrencyManager.GetCurrencyByName(currencyType);
			return result != null;
		}
	}
}
