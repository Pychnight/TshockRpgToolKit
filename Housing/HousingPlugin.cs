using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Housing.Database;
using Housing.Models;
using Housing.Extensions;
using Microsoft.Xna.Framework;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Banking;
using Corruption.PluginSupport;
using Banking.Currency;

namespace Housing
{
    [ApiVersion(2, 1)]
    public sealed partial class HousingPlugin : TerrariaPlugin
    {
        private const string SessionKey = "Housing_Session";
		private const int MessageRefreshDelay = 2000;//2 seconds( in ms ), used by calls that refresh the players display 
		private static readonly string ConfigPath = Path.Combine("housing", "config.json");
        private static readonly string SqlitePath = Path.Combine("housing", "db.sqlite");
		
		public static HousingPlugin Instance { get; private set; }
        private DbConnection databaseConnection;
		internal IDatabase database;
		internal TaxService TaxService;
		
        public override string Author => "MarioE, Timothy Barela";
        public override string Description => "Adds a housing and shop system.";
        public override string Name => "Housing";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		public HousingPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		public override void Initialize()
        {
            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.NetGetData.Register(this, OnNetGetData, 10);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);

            Commands.ChatCommands.Add(new Command("housing.house", HouseCmd, "house"));
			Commands.ChatCommands.Add(new Command("housing.house", GoHomeCommand, "gohome"));
			Commands.ChatCommands.Add(new Command("housing.itemshop", ItemShop, "itemshop"));
			Commands.ChatCommands.Add(new Command("housing.tax", TaxService.TaxCommand, "tax"));
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));

                GeneralHooks.ReloadEvent -= OnReload;
                ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            }
            base.Dispose(disposing);
        }

        private Session GetOrCreateSession(TSPlayer player)
        {
            var session = player.GetData<Session>(SessionKey);
            if (session == null)
            {
                session = new Session(player);
                player.SetData(SessionKey, session);
            }
            return session;
        }
		
		private void onLoad()
		{
			Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);
			
			TaxService = TaxService ?? new TaxService(this);
			
			TaxService.IsEnabled = Config.Instance.EnableTaxService;
			database = DatabaseFactory.LoadOrCreateDatabase(Config.Instance);
			
			database.Load();
		}

        private void OnGamePostInitialize(EventArgs args)
        {
			onLoad();
        }

		private void OnReload(ReloadEventArgs args)
		{
			onLoad();
			args.Player.SendSuccessMessage("[Housing] Reloaded config!");
		}

		private void OnGameUpdate(EventArgs args)
        {
            foreach (var player in TShock.Players.Where(p => p?.Active == true))
            {
                var session = GetOrCreateSession(player);
                var house = database.GetHouse(player.TileX, player.TileY);
                if (house != null && session.CurrentHouse != house)
                {
                    Debug.WriteLine($"DEBUG: {player.Name} entered {house.OwnerName}'s {house} house");
                    player.SendInfoMessage(
                        $"You entered {(house.OwnerName == player.Name ? "your house" : house.OwnerName + "'s house")} " +
                        $"{Color.MediumPurple.ColorText(house)}.");
                    if (house.ForSale && house.OwnerName != player.Name)
                    {
                        player.SendInfoMessage(
                            $"This house is on sale for [c/{Color.OrangeRed.Hex3()}:{house.SalePrice}].");
                    }
                }
                else if (session.CurrentHouse != null && house != session.CurrentHouse)
                {
                    Debug.WriteLine(
                        $"DEBUG: {player.Name} left {session.CurrentHouse.OwnerName}'s {session.CurrentHouse} house");
                    player.SendInfoMessage(
                        $"You left {(session.CurrentHouse.OwnerName == player.Name ? "your house" : session.CurrentHouse.OwnerName + "'s house")} " +
                        $"{Color.MediumPurple.ColorText(session.CurrentHouse)}.");
                }
                session.CurrentHouse = house;

                var shop = database.GetShop(player.TileX, player.TileY);
                if (shop != null && session.CurrentShop != shop && shop.Message != null)
                {
                    Debug.WriteLine($"DEBUG: {player.Name} entered {shop.OwnerName}'s {shop} shop");
                    player.SendInfoMessage(shop.Message);
                }
                session.CurrentShop = shop;
            }

            var shops = database.GetShops();
            foreach (var house in database.GetHouses())
            {
				var houseConfig = house.GetGroupConfig();

				if (DateTime.UtcNow - house.LastTaxed > houseConfig.TaxPeriod)
                {
					var totalBalance = BankingPlugin.Instance.Bank.GetTotalBalance(house.OwnerName);
					
                    //var isStore = shops.Any(s => house.Rectangle.Contains(s.Rectangle));
					var store = shops.FirstOrDefault(s => house.Rectangle.Contains(s.Rectangle));
					var storeConfig = store != null ? store.GetGroupConfig() : houseConfig;

					var taxRate = store!=null ? storeConfig.StoreTaxRate : houseConfig.TaxRate;
                    var taxCost = (long)Math.Round(house.Area * taxRate) + house.Debt;
                    var payment = Math.Min(totalBalance, taxCost);

					var player = TShock.Players.Where(p => p?.Active == true)
						.FirstOrDefault(p => p.Name == house.OwnerName);

					if(player!=null)
					{
						foreach(var payInfo in TaxService.PayTaxIterator(house.OwnerName, payment))
						{
							var curr = BankingPlugin.Instance.Bank.CurrencyManager.GetCurrencyByName(payInfo.Item2.Name);
							var payText = curr.GetCurrencyConverter().ToString(payInfo.Item1);
							
							player?.SendInfoMessage($"You were taxed {Color.OrangeRed.ColorText(payText)} for your house " +
												$"{Color.MediumPurple.ColorText(house)}.");
						}
					}
					else
					{
						//since the player is not active, we can use the more more efficient version
						TaxService.PayTax(house.OwnerName, payment);
					}
					
					house.Debt = taxCost - payment;
                    if (payment < taxCost)
                    {
                        if (house.Debt > houseConfig.MaxDebtAllowed)
                        {
							database.Remove(house);

							if (player != null)
							{
								player.SendInfoMessage($"Your house '{house.Name}' has been reclaimed due to excessive debt!");
							}

							continue;
                        }
                    }

                    house.LastTaxed = DateTime.UtcNow;
                    database.Update(house);
                }
            }
        }

        private void OnNetGetData(GetDataEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            if (args.MsgID == PacketTypes.ChestGetContents)
            {
                var player = TShock.Players[args.Msg.whoAmI];
                var session = GetOrCreateSession(player);
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    var x = reader.ReadInt16();
                    var y = reader.ReadInt16();

                    var shop = database.GetShops().FirstOrDefault(s => s.ChestX == x && s.ChestY == y);
                    session.CurrentlyViewedShop = shop;
                    if (shop == null)
                    {
                        return;
                    }
                    args.Handled = true;

                    if (shop.OwnerName == player.Name)
                    {
                        Debug.WriteLine($"DEBUG: {player.Name} changing shop at {x}, {y}");
                        shop.IsBeingChanged = true;
                        var chest = new Chest { x = x, y = y };
                        Main.chest[998] = chest;
                        for (var i = 0; i < 40; ++i)
                        {
                            var shopItem = shop.Items.FirstOrDefault(si => si.Index == i);
                            chest.item[i] = new Item();
                            chest.item[i].SetDefaults(shopItem?.ItemId ?? 0);
                            chest.item[i].stack = shopItem?.StackSize ?? 0;
                            chest.item[i].prefix = shopItem?.PrefixId ?? 0;
                            player.SendData(PacketTypes.ChestItem, "", 998, i);
                        }
                        player.SendData(PacketTypes.ChestOpen, "", 998);
                        player.SendInfoMessage("Use /itemshop setprice <item-name> <price> to change prices.");
                    }
                    else if (shop.OwnerName != player.Name)
                    {
                        Debug.WriteLine($"DEBUG: {player.Name} tried to view shop at {shop.ChestX}, {shop.ChestY}");
                        shop.TryShowStock(player);
                    }
                }
            }
            else if (args.MsgID == PacketTypes.ChestItem)
            {
                var player = TShock.Players[args.Msg.whoAmI];
                var session = GetOrCreateSession(player);
                var shop = session.CurrentlyViewedShop;
                if (shop == null || shop.OwnerName != player.Name)
                {
                    return;
                }

                Debug.WriteLine($"DEBUG: {player.Name} changed shop at {shop.ChestX}, {shop.ChestY}");
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    reader.ReadInt16();
                    var itemIndex = reader.ReadByte();
                    var stackSize = reader.ReadInt16();
                    var prefixId = reader.ReadByte();
                    var itemId = reader.ReadInt16();

                    var shopItem = shop.Items.FirstOrDefault(i => i.Index == itemIndex);
                    if (shopItem == null)
                    {
                        shop.Items.Add(new ShopItem(itemIndex, itemId, stackSize, prefixId));
                    }
                    else
                    {
                        shopItem.ItemId = itemId;
                        shopItem.StackSize = stackSize;
                        shopItem.PrefixId = prefixId;
                    }
                    database.Update(shop);
                    args.Handled = true;
                }
            }
            else if (args.MsgID == PacketTypes.ChestOpen)
            {
                var player = TShock.Players[args.Msg.whoAmI];
                var session = GetOrCreateSession(player);
                var shop = session.CurrentlyViewedShop;
                if (shop != null && shop.OwnerName == player.Name)
                {
                    Debug.WriteLine($"DEBUG: {player.Name} finished changing shop at {shop.ChestX}, {shop.ChestY}");
                    shop.IsBeingChanged = false;
                }
            }
            else if (args.MsgID == PacketTypes.Tile) //Fix me Tilekill removed?
            {
                var player = TShock.Players[args.Msg.whoAmI];
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    var action = reader.ReadByte();
                    var x = reader.ReadInt16();
                    var y = reader.ReadInt16();
                    var style = reader.ReadInt16();
                    if (action != 1 && action != 3)
                    {
                        if (action == 2)
                        {
                            --x;
                        }
                        --y;
                        
                        var session = GetOrCreateSession(player);
                        if (session.NextShopHouse != null)
                        {
                            if (!session.NextShopHouse.Rectangle.Contains(x, y))
                            {
                                player.SendErrorMessage("Your house must contain your item shop chest.");
                                return;
                            }
                            
                            var shop = database.AddShop(player, session.NextShopName, session.NextShopX,
                                                         session.NextShopY, session.NextShopX2, session.NextShopY2, x,
                                                         y);
                            player.SendSuccessMessage($"Added the shop [c/{Color.LimeGreen.Hex3()}:{shop}].");
                            player.SendInfoMessage(
                                "Use /itemshop open and /itemshop close to open and close your shop.");
                            session.NextShopHouse = null;
                            
                            var tileId = action == 0 ? TileID.Containers : action == 2 ? TileID.Dressers : TileID.Containers2;
                            var chestId = WorldGen.PlaceChest(x, y, tileId, false, style);
                            if (chestId >= 0)
                            {
                                Main.chest[chestId] = null;
                            }
                            // We don't send a chest creation packet, as the players have to "discover" the chest themselves.
                            TSPlayer.All.SendTileSquare(x, y, 4);
                        }
                        return;
                    }

                    var tile = Main.tile[x, y];
                    if (tile.type != TileID.Containers && tile.type != TileID.Dressers &&
                        tile.type != TileID.Containers2)
                    {
                        return;
                    }

                    if (tile.frameY % 36 != 0)
                    {
                        --y;
                    }
                    if (tile.frameX % 36 != 0)
                    {
                        --x;
                    }

                    if (database.GetShops().Any(s => s.ChestX == x && s.ChestY == y))
                    {
                        player.SendErrorMessage("You can't remove shop chests.");
                        args.Handled = true;
                        player.SendTileSquare(x, y, 3);
                    }
                }
            }
        }
		
        private void OnServerLeave(LeaveEventArgs args)
        {
            if (args.Who < 0 || args.Who >= Main.maxPlayers)
            {
                return;
            }

            var player = TShock.Players[args.Who];
			var config = Config.Instance.GetGroupConfig(player!=null ? player.Group.Name : ">");//force default if no group.. we can never have a group named ">" ...right?
			if (player != null && config?.AllowOfflineShops==false)//!Config.Instance.AllowOfflineShops)
            {
                foreach (var shop in database.GetShops().Where(s => s.OwnerName == player.Name))
                {
                    shop.IsOpen = false;
                }
            }
        }
    }
}
