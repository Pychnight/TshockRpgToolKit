using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Housing.Database;
using Microsoft.Xna.Framework;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace Housing
{
    [ApiVersion(2, 1)]
    public sealed class HousingPlugin : TerrariaPlugin
    {
        private const string SessionKey = "Housing_Session";

        private static readonly string ConfigPath = Path.Combine("housing", "config.json");
        private static readonly string SqlitePath = Path.Combine("housing", "db.sqlite");

        private DbConnection _connection;
        internal DatabaseManager _database;
		private TaxService taxService;

        public HousingPlugin(Main game) : base(game)
        {
        }

        public override string Author => "MarioE";
        public override string Description => "Adds a housing and shop system.";
        public override string Name => "Housing";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override void Initialize()
        {
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif

            Directory.CreateDirectory("housing");
            if (File.Exists(ConfigPath))
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
			
			taxService = new TaxService(this);
			if (Config.Instance != null)
			{
				taxService.IsEnabled = Config.Instance.EnableTaxService;
			}

			_connection = new SqliteConnection($"uri=file://{SqlitePath},Version=3");
            _database = new DatabaseManager(_connection);
			_database.TaxService = taxService;//for db integration, we do this.
			
            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.NetGetData.Register(this, OnNetGetData, 10);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);

            Commands.ChatCommands.Add(new Command("housing.house", HouseCmd, "house"));
			Commands.ChatCommands.Add(new Command("housing.house", GoHomeCommand, "gohome"));
			Commands.ChatCommands.Add(new Command("housing.itemshop", ItemShop, "itemshop"));
			Commands.ChatCommands.Add(new Command("housing.tax", taxService.TaxCmd, "tax"));
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));

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

        private void HouseCmd(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
			var playerGroupConfig = Config.Instance.GetGroupConfig(player.Group.Name);

			var subcommand = parameters.Count > 0 ? parameters[0] : "";
            if (subcommand.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                player.AwaitingTempPoint = 1;
                player.SendInfoMessage("Hit a block to set the first point.");
            }
            else if (subcommand.Equals("2", StringComparison.OrdinalIgnoreCase))
            {
                player.AwaitingTempPoint = 2;
                player.SendInfoMessage("Hit a block to set the second point.");
            }
            else if (subcommand.Equals("allow", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 2)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}house allow <player-name>");
                    return;
                }

                var session = GetOrCreateSession(player);
                if (session.CurrentHouse == null)
                {
                    player.SendErrorMessage("You aren't currently in a house.");
                    return;
                }

                var house = session.CurrentHouse;
                if (player.User?.Name != house.OwnerName && !player.HasPermission("housing.house.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't allow users for {house.OwnerName}'s [c/{Color.MediumPurple.Hex3()}:{house}] house.");
                    return;
                }

                var inputPlayerName = parameters[1];
                var players = TShock.Utils.FindPlayer(inputPlayerName);
                if (players.Count > 1)
                {
                    player.SendErrorMessage($"Multiple players matched '{inputPlayerName}':");
                    TShock.Utils.SendMultipleMatchError(player, players);
                    return;
                }
                if (players.Count == 0)
                {
                    player.SendErrorMessage($"Invalid player '{inputPlayerName}'.");
                    return;
                }

                var otherPlayer = players[0];
                if (otherPlayer.User == null)
                {
                    player.SendErrorMessage($"{otherPlayer.Name} is not logged in.");
                    return;
                }

                house.AllowedUsernames.Add(otherPlayer.User.Name);
                _database.Update(house);
                player.SendSuccessMessage(
                    $"Allowed {otherPlayer.Name} to modify " +
                    $"{(house.OwnerName == player.User?.Name ? "your" : house.OwnerName + "'s")} " +
                    $"[c/{Color.MediumPurple.Hex3()}:{house}] house.");
            }
            else if (subcommand.Equals("buy", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 2)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}house buy <house-name>");
                    return;
                }

                var inputHouseName = parameters[1];
                var session = GetOrCreateSession(player);
                if (session.CurrentHouse == null)
                {
                    var plot = TShock.Regions.InAreaRegion(player.TileX, player.TileY)
                        .FirstOrDefault(r => r.Name.StartsWith("__Plot"));
                    if (plot == null)
                    {
                        player.SendErrorMessage("You aren't currently in a house or plot.");
                        return;
                    }

                    player.TempPoints[0] = new Point(plot.Area.X, plot.Area.Y);
                    player.TempPoints[1] = new Point(plot.Area.Right - 1, plot.Area.Bottom - 1);
                    HouseCmd(new CommandArgs("", player, new List<string> {"set", inputHouseName}));
                    return;
                }

                var house = session.CurrentHouse;
                if (!house.ForSale || house.OwnerName == player.User?.Name)
                {
                    player.SendErrorMessage("You cannot purchase this house.");
                    return;
                }
								
				var purchaseCost = house.Price;
                var salesTax = (Money)Math.Round(playerGroupConfig.TaxRate * purchaseCost);
                player.SendInfoMessage(
                    $"Purchasing {house.OwnerName}'s house [c/{Color.MediumPurple.Hex3()}:{house}] will cost " +
                    $"[c/{Color.OrangeRed.Hex3()}:{purchaseCost}], with a sales tax of [c/{Color.OrangeRed.Hex3()}:{salesTax}].");
                player.SendInfoMessage("Do you wish to proceed? Use /yes or /no.");
                player.AddResponse("yes", args2 =>
                {
                    player.AwaitingResponse.Remove("no");
                    var account = SEconomyPlugin.Instance?.GetBankAccount(player);
                    if (account == null || account.Balance < purchaseCost + salesTax)
                    {
                        player.SendErrorMessage(
                            $"You do not have enough of a balance to purchase {house.OwnerName}'s " +
                            $"[c/{Color.MediumPurple.Hex3()}:{house}] house.");
                        return;
                    }
                    if (!house.ForSale)
                    {
                        player.SendErrorMessage("Unfortunately, the house was purchased while waiting.");
                        return;
                    }
                    house.ForSale = false;

                    if (purchaseCost > 0)
                    {
                        var account2 = SEconomyPlugin.Instance.RunningJournal.GetBankAccountByName(house.OwnerName);
                        account.TransferTo(
                            account2, purchaseCost, BankAccountTransferOptions.IsPayment,
                            "", $"Purchased {house.OwnerName}'s house {house.Name}");
                    }
                    if (salesTax > 0)
                    {
						//account.TransferTo(
						//    SEconomyPlugin.Instance.WorldAccount, salesTax, BankAccountTransferOptions.IsPayment,
						//    "", $"Sales tax for {house.OwnerName}'s {house.Name} house");

						taxService.PayTax(account, salesTax, BankAccountTransferOptions.IsPayment, "", $"Sales tax for {house.OwnerName}'s house, {house.Name}");
                    }

                    _database.Remove(house);
                    _database.AddHouse(player, inputHouseName, house.Rectangle.X, house.Rectangle.Y,
                                       house.Rectangle.Right - 1, house.Rectangle.Bottom - 1);
                    player.SendInfoMessage(
                        $"Purchased {house.OwnerName}'s house [c/{Color.MediumPurple.Hex3()}:{house}] for " +
                        $"[c/{Color.OrangeRed.Hex3()}:{(Money)(purchaseCost + salesTax)}].");

                    var player2 = TShock.Players.Where(p => p?.Active == true)
                        .FirstOrDefault(p => p.User?.Name == house.OwnerName);
                    player2?.SendInfoMessage(
                        $"{player.Name} purchased your house [c/{Color.MediumPurple.Hex3()}:{house}] for " +
                        $"[c/{Color.OrangeRed.Hex3()}:{(Money)(purchaseCost + salesTax)}].");
                });
                player.AddResponse("no", args2 =>
                {
                    player.AwaitingResponse.Remove("yes");
                    player.SendInfoMessage("Canceled purchase.");
                });
            }
            else if (subcommand.Equals("disallow", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 2)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}house disallow <username>");
                    return;
                }

                var session = GetOrCreateSession(player);
                if (session.CurrentHouse == null)
                {
                    player.SendErrorMessage("You aren't currently in a house.");
                    return;
                }

                var house = session.CurrentHouse;
                if (player.User?.Name != house.OwnerName && !player.HasPermission("housing.house.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't disallow users for {house.OwnerName}'s [c/{Color.MediumPurple.Hex3()}:{house}] house.");
                    return;
                }

                var inputUsername = parameters[1];
                house.AllowedUsernames.Remove(inputUsername);
                _database.Update(house);
                player.SendSuccessMessage(
                    $"Disallowed {inputUsername} from modifying " +
                    $"{(house.OwnerName == player.User?.Name ? "your house" : house.OwnerName + "'s house")} " +
                    $"[c/{Color.MediumPurple.Hex3()}:{house}].");
            }
            else if (subcommand.Equals("info", StringComparison.OrdinalIgnoreCase))
            {
                var session = GetOrCreateSession(player);
                if (session.CurrentHouse == null)
                {
                    player.SendErrorMessage("You aren't currently in a house.");
                    return;
                }
				
				var house = session.CurrentHouse;
                player.SendInfoMessage($"Owner: {house.OwnerName}, Name: {house.Name}");
                if (player.User?.Name == house.OwnerName || player.HasPermission("housing.house.admin"))
                {
					var ownerConfig = house.GetGroupConfig();//because a player other than the owner maybe running this command.

                    player.SendInfoMessage($"Debt: [c/{Color.OrangeRed.Hex3()}:{house.Debt}]");
                    var isStore = _database.GetShops().Any(s => house.Rectangle.Contains(s.Rectangle));
                    var taxRate = isStore ? ownerConfig.StoreTaxRate : ownerConfig.TaxRate;
                    var taxCost = (Money)Math.Round(house.Area * taxRate);
                    player.SendInfoMessage(
                        $"Tax cost: [c/{Color.OrangeRed.Hex3()}:{taxCost}], Last taxed: {house.LastTaxed}");
                    player.SendInfoMessage($"Allowed users: {string.Join(", ", house.AllowedUsernames)}");
                }
            }
            else if (subcommand.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                var session = GetOrCreateSession(player);
                if (session.CurrentHouse == null)
                {
                    player.SendErrorMessage("You aren't currently in a house.");
                    return;
                }

                var house = session.CurrentHouse;
                if (player.User?.Name != house.OwnerName && !player.HasPermission("housing.house.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't remove {house.OwnerName}'s [c/{Color.MediumPurple.Hex3()}:{house}] house.");
                    return;
                }

                _database.Remove(house);
                player.SendSuccessMessage(
                    $"Removed {(house.OwnerName == player.User?.Name ? "your" : house.OwnerName + "'s")} " +
                    $"[c/{Color.MediumPurple.Hex3()}:{house}] house.");
            }
            else if (subcommand.Equals("sell", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 2)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}house sell <price>");
                    return;
                }

                var session = GetOrCreateSession(player);
                if (session.CurrentHouse == null)
                {
                    player.SendErrorMessage("You aren't currently in a house.");
                    return;
                }

                var house = session.CurrentHouse;
                if (player.User?.Name != house.OwnerName && !player.HasPermission("housing.house.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't sell {house.OwnerName}'s [c/{Color.MediumPurple.Hex3()}:{house}] house.");
                    return;
                }

                var inputPrice = parameters[1];
                if (!Money.TryParse(inputPrice, out var price) || price <= 0)
                {
                    player.SendErrorMessage($"Invalid price '{inputPrice}'.");
                    return;
                }

                house.ForSale = true;
                house.Price = price;
                _database.Update(house);
                player.SendSuccessMessage(
                    $"Selling {(house.OwnerName == player.User?.Name ? "your" : house.OwnerName + "'s")} " +
                    $"[c/{Color.MediumPurple.Hex3()}:{house}] house for [c/{Color.OrangeRed.Hex3()}:{price}].");
            }
            else if (subcommand.Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 2)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}house set <house-name>");
                    return;
                }

                if (player.TempPoints.Any(p => p == Point.Zero))
                {
                    player.SendErrorMessage("Not all points have been set.");
                    return;
                }
								
                var point1 = player.TempPoints[0];
                var point2 = player.TempPoints[1];
                var inputHouseName = parameters[1];
                var x = Math.Min(point1.X, point2.X);
                var y = Math.Min(point1.Y, point2.Y);
                var x2 = Math.Max(point1.X, point2.X);
                var y2 = Math.Max(point1.Y, point2.Y);
                if (_database.GetHouses().Count(h => h.OwnerName == player.User?.Name) >= playerGroupConfig.MaxHouses)
                {
                    player.SendErrorMessage($"You have too many houses. Maximum allowed is {playerGroupConfig.MaxHouses}.");
                    return;
                }

                var area = (x2 - x + 1) * (y2 - y + 1);
                if (area < playerGroupConfig.MinHouseSize)
				{
                    player.SendErrorMessage($"Your house is too small. Minimum area is {playerGroupConfig.MinHouseSize}.");
                    return;
                }
                if (area > playerGroupConfig.MaxHouseSize)
                {
                    player.SendErrorMessage($"Your house is too large. Maximum area is {playerGroupConfig.MaxHouseSize}.");
                    return;
                }

                var rectangle = new Rectangle(x, y, x2 - x + 1, y2 - y + 1);
                if (_database.GetHouses().Any(h => h.Rectangle.Intersects(rectangle)))
                {
                    player.SendErrorMessage("Your house must not intersect any other houses.");
                    return;
                }
                if (Config.Instance.RequireAdminRegions && TShock.Regions.ListAllRegions(Main.worldID.ToString()).All(
                        r => !r.Name.StartsWith("__Plot") || !r.Area.Contains(rectangle)))
                {
                    player.SendErrorMessage("Your house must lie entirely on a plot.");
                }

                player.TempPoints[0] = Point.Zero;
                player.TempPoints[1] = Point.Zero;
                var purchaseCost = (Money)Math.Round(rectangle.Width * rectangle.Height * playerGroupConfig.PurchaseRate);
                if (purchaseCost > 0)
                {
                    var taxCost = (Money)Math.Round(rectangle.Width * rectangle.Height * playerGroupConfig.TaxRate);
                    player.SendInfoMessage(
                        $"Purchasing this house will require [c/{Color.OrangeRed.Hex3()}:{purchaseCost}].");
                    player.SendInfoMessage(
                        $"The tax for this house will be [c/{Color.OrangeRed.Hex3()}:{taxCost}].");
                    player.SendInfoMessage("Do you wish to proceed? Use /yes or /no.");
                    player.AddResponse("yes", args2 =>
                    {
                        player.AwaitingResponse.Remove("no");
                        var account = SEconomyPlugin.Instance?.GetBankAccount(player);
                        if (account == null || account.Balance < purchaseCost)
                        {
                            player.SendErrorMessage("You do not have enough of a balance to purchase the house.");
                            return;
                        }

                        account.TransferTo(
                            SEconomyPlugin.Instance.WorldAccount, purchaseCost, BankAccountTransferOptions.IsPayment,
                            "", $"Purchased the {inputHouseName} house");
                        var house = _database.AddHouse(player, inputHouseName, x, y, x2, y2);
                        player.SendSuccessMessage($"Purchased house [c/{Color.MediumPurple.Hex3()}:{house}] for " +
                                                  $"[c/{Color.OrangeRed.Hex3()}:{purchaseCost}].");
                    });
                    player.AddResponse("no", args2 =>
                    {
                        player.AwaitingResponse.Remove("yes");
                        player.SendInfoMessage("Canceled purchase.");
                    });
                }
                else
                {
                    var house = _database.AddHouse(player, inputHouseName, x, y, x2, y2);
                    player.SendSuccessMessage($"Added house [c/{Color.MediumPurple.Hex3()}:{house}].");
                }
            }
			else
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house 1/2");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house allow <player-name>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house buy <house-name>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house disallow <username>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house info");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house remove");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house sell <price>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house set <house-name>");
            }
        }

		//teleports player home
		private void GoHomeCommand(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var houseName = parameters.Count == 1 ? parameters[0] : "";
			//var ownerName = player.User?.Name; // which name to use?!?!
			var ownerName = player.Name;
			House house = null;

			if( parameters.Count> 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}gohome <house-name>");
				return;
			}
			
			if(!string.IsNullOrWhiteSpace(houseName))
				house = _database.GetHouse(player.Name, houseName);
			else
				house = _database.GetHouses(player.Name).FirstOrDefault();
			
			if(house==null)
			{
				player.SendErrorMessage($"Sorry, I couldn't find your house.");
				return;
			}
			else
			{
				var rect = house.Rectangle;

				var cx = ( rect.X * 16 ) + ( rect.Width / 2 ) * 16;
				var cy = ( rect.Y * 16 ) + ( rect.Height / 2 ) * 16;
				
				player.Teleport(cx, cy);
				player.SendErrorMessage($"Teleporting you to your house.");
			}
		}

		private void ItemShop(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
			var playerGroupConfig = Config.Instance.GetGroupConfig(player.Group.Name);
			var subcommand = parameters.Count > 0 ? parameters[0] : "";
            if (subcommand.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                player.AwaitingTempPoint = 1;
                player.SendInfoMessage("Hit a block to set the first point.");
            }
            else if (subcommand.Equals("2", StringComparison.OrdinalIgnoreCase))
            {
                player.AwaitingTempPoint = 2;
                player.SendInfoMessage("Hit a block to set the second point.");
            }
            else if (subcommand.Equals("buy", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 2 && parameters.Count != 3)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop buy <item-index> [amount]");
                    return;
                }

                var session = GetOrCreateSession(player);
                if (session.CurrentlyViewedShop == null)
                {
                    player.SendErrorMessage("You aren't currently viewing a shop.");
                    return;
                }
                var shop = session.CurrentlyViewedShop;

                var inputItemIndex = parameters[1];
                if (!int.TryParse(inputItemIndex, out var itemIndex) || itemIndex < 1 || itemIndex > Chest.maxItems)
                {
                    player.SendErrorMessage($"Invalid item index '{inputItemIndex}'.");
                    return;
                }

                var shopItem = shop.Items.FirstOrDefault(i => i.Index == itemIndex - 1 && i.StackSize > 0);
                if (shopItem == null)
                {
                    player.SendErrorMessage($"Invalid item index '{inputItemIndex}'.");
                    return;
                }

                var inputAmount = parameters.Count == 3 ? parameters[2] : "1";
                if (!int.TryParse(inputAmount, out var amount) || amount < 1 || amount > shopItem.StackSize)
                {
                    player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
                    return;
                }

                var item = new Item();
                var itemId = shopItem.ItemId;
                item.SetDefaults(itemId);
                var purchaseCost = (Money)(amount * shop.UnitPrices.Get(itemId, item.value / 5));
                var salesTax = (Money)Math.Round(purchaseCost * playerGroupConfig.SalesTaxRate);
                var itemText = $"[i/s{amount},p{shopItem.PrefixId}:{shopItem.ItemId}]";
                player.SendInfoMessage(
                    $"Purchasing {itemText} will cost [c/{Color.OrangeRed.Hex3()}:{purchaseCost}], " +
                    $"with a sales tax of [c/{Color.OrangeRed.Hex3()}:{salesTax}].");
                player.SendInfoMessage("Do you wish to proceed? Use /yes or /no.");
                player.AddResponse("yes", args2 =>
                {
                    player.AwaitingResponse.Remove("no");
                    var account = SEconomyPlugin.Instance?.GetBankAccount(player);
                    if (account == null || account.Balance < purchaseCost + salesTax)
                    {
                        player.SendErrorMessage($"You do not have enough of a balance to purchase {itemText}.");
                        return;
                    }
                    if (shopItem.StackSize < amount || shopItem.ItemId != itemId || shop.IsBeingChanged)
                    {
                        player.SendErrorMessage("While waiting, the shop changed.");
                        return;
                    }

                    if (purchaseCost > 0)
                    {
                        var account2 = SEconomyPlugin.Instance.RunningJournal.GetBankAccountByName(shop.OwnerName);
                        account.TransferTo(
                            account2, purchaseCost, BankAccountTransferOptions.IsPayment,
                            "", $"Purchased {item.Name} x{amount}");
                    }
                    if (salesTax > 0)
                    {
                        account.TransferTo(
                            SEconomyPlugin.Instance.WorldAccount, salesTax, BankAccountTransferOptions.IsPayment,
                            "", $"Sales tax for {item.Name} x{amount}");
                    }

                    shopItem.StackSize -= amount;
                    _database.Update(shop);

                    player.GiveItem(
                        itemId, "", Player.defaultWidth, Player.defaultHeight, amount, shopItem.PrefixId);
                    player.SendSuccessMessage($"Purchased {itemText} for " +
                                              $"[c/{Color.OrangeRed.Hex3()}:{(Money)(purchaseCost + salesTax)}].");

                    var player2 = TShock.Players.Where(p => p?.Active == true)
                        .FirstOrDefault(p => p.User?.Name == shop.OwnerName);
                    player2?.SendInfoMessage($"{player.Name} purchased {itemText} for " +
                                             $"[c/{Color.OrangeRed.Hex3()}:{(Money)(purchaseCost + salesTax)}].");
                });
                player.AddResponse("no", args2 =>
                {
                    player.AwaitingResponse.Remove("yes");
                    player.SendInfoMessage("Canceled purchase.");
                });
            }
            else if (subcommand.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                var session = GetOrCreateSession(player);
                if (session.CurrentShop == null)
                {
                    player.SendErrorMessage("You aren't currently in a shop.");
                    return;
                }

                var shop = session.CurrentShop;
                if (shop.OwnerName != player.User?.Name && !player.HasPermission("housing.itemshop.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't close {shop.OwnerName}'s shop [c/{Color.LimeGreen.Hex3()}:{shop}].");
                    return;
                }

                shop.IsOpen = false;
                _database.Update(shop);
                player.SendSuccessMessage(
                    $"Closed {(shop.OwnerName == player.User?.Name ? "your shop" : shop.OwnerName + "'s shop")} " +
                    $"[c/{Color.LimeGreen.Hex3()}:{shop}].");
            }
            else if (subcommand.Equals("info", StringComparison.OrdinalIgnoreCase))
            {
                var session = GetOrCreateSession(player);
                if (session.CurrentShop == null)
                {
                    player.SendErrorMessage("You aren't currently in a shop.");
                    return;
                }

                var shop = session.CurrentShop;
                player.SendInfoMessage($"Owner: {shop.OwnerName}, Name: {shop.Name}");
                var prices = shop.UnitPrices.Where(kvp => kvp.Value > 0)
                    .Select(kvp => $"[i:{kvp.Key}]: [c/{Color.OrangeRed.Hex3()}:{kvp.Value}]");
                player.SendInfoMessage(
                    $"Prices: {string.Join(", ", prices)}. All other items are default sell prices.");
                if (shop.OwnerName == player.User?.Name)
                {
					//var ownerConfig = shop.GetGroupConfig();

                    var house = session.CurrentHouse;
                    var taxRate = playerGroupConfig.StoreTaxRate - playerGroupConfig.TaxRate;
                    var taxCost = (Money)Math.Round(house.Area * taxRate);
                    player.SendInfoMessage($"Extra tax on house: [c/{Color.OrangeRed.Hex3()}:{taxCost}]");
                }
            }
            else if (subcommand.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                var session = GetOrCreateSession(player);
                if (session.CurrentShop == null)
                {
                    player.SendErrorMessage("You aren't currently in a shop.");
                    return;
                }

                var shop = session.CurrentShop;
                if (shop.OwnerName != player.User?.Name && !player.HasPermission("housing.itemshop.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't open {shop.OwnerName}'s shop [c/{Color.LimeGreen.Hex3()}:{shop}].");
                    return;
                }

                shop.IsOpen = true;
                _database.Update(shop);
                player.SendSuccessMessage(
                    $"Opened {(shop.OwnerName == player.User?.Name ? "your shop" : shop.OwnerName + "'s shop")} " +
                    $"[c/{Color.LimeGreen.Hex3()}:{shop}].");
            }
            else if (subcommand.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                var session = GetOrCreateSession(player);
                if (session.CurrentShop == null)
                {
                    player.SendErrorMessage("You aren't currently in a shop.");
                    return;
                }

                var shop = session.CurrentShop;
                if (shop.OwnerName != player.User?.Name && !player.HasPermission("housing.itemshop.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't remove {shop.OwnerName}'s shop [c/{Color.LimeGreen.Hex3()}:{shop}].");
                    return;
                }

                // Revert the chest to a normal chest.
                var chestId = Chest.FindEmptyChest(shop.ChestX, shop.ChestY);
                if (chestId >= 0)
                {
                    var chest = new Chest();
                    for (var i = 0; i < Chest.maxItems; ++i)
                    {
                        var shopItem = shop.Items.FirstOrDefault(si => si.Index == i);
                        var item = new Item();
                        item.SetDefaults(shopItem?.ItemId ?? 0);
                        item.stack = shopItem?.StackSize ?? 0;
                        item.prefix = shopItem?.PrefixId ?? 0;
                        chest.item[i] = item;
                    }
                    Main.chest[chestId] = chest;
                }

                _database.Remove(shop);
                player.SendSuccessMessage(
                    $"Removed {(shop.OwnerName == player.User?.Name ? "your shop" : shop.OwnerName + "'s shop")} " +
                    $"[c/{Color.LimeGreen.Hex3()}:{shop}].");
            }
            else if (subcommand.Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 2)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop set <shop-name>");
                    return;
                }

                if (player.TempPoints.Any(p => p == Point.Zero))
                {
                    player.SendErrorMessage("Not all points have been set.");
                    return;
                }

                var session = GetOrCreateSession(player);
                if (session.CurrentHouse == null || session.CurrentHouse.OwnerName != player.User?.Name)
                {
                    player.SendErrorMessage("You aren't currently in a house that you own.");
                    return;
                }

				//var playerGroupConfig = Config.Instance.GetGroupConfig(player.Group.Name);
				var point1 = player.TempPoints[0];
                var point2 = player.TempPoints[1];
                var inputShopName = parameters[1];
                var x = Math.Min(point1.X, point2.X);
                var y = Math.Min(point1.Y, point2.Y);
                var x2 = Math.Max(point1.X, point2.X);
                var y2 = Math.Max(point1.Y, point2.Y);
                var area = (x2 - x + 1) * (y2 - y + 1);
                if (area < playerGroupConfig.MinShopSize)
                {
                    player.SendErrorMessage($"Your shop is too small. Minimum area is {playerGroupConfig.MinShopSize}.");
                    return;
                }
                if (area > playerGroupConfig.MaxShopSize)
                {
                    player.SendErrorMessage($"Your shop is too large.Maximum area is {playerGroupConfig.MaxShopSize}.");
                    return;
                }

                var rectangle = new Rectangle(x, y, x2 - x + 1, y2 - y + 1);
                if (!session.CurrentHouse.Rectangle.Contains(rectangle))
                {
                    player.SendErrorMessage("Your shop must lie entirely within your house.");
                    return;
                }

                session.NextShopHouse = session.CurrentHouse;
                session.NextShopName = inputShopName;
                session.NextShopX = x;
                session.NextShopY = y;
                session.NextShopX2 = x2;
                session.NextShopY2 = y2;
                player.SendInfoMessage("Place a chest to serve as the item shop chest.");
            }
            else if (subcommand.Equals("setmsg", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count < 2)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop setmsg <message>");
                    return;
                }

                var session = GetOrCreateSession(player);
                if (session.CurrentShop == null)
                {
                    player.SendErrorMessage("You aren't currently in a shop.");
                    return;
                }

                var shop = session.CurrentShop;
                if (shop.OwnerName != player.User?.Name && !player.HasPermission("housing.itemshop.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't set the message for {shop.OwnerName}'s [c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
                    return;
                }

                var message = string.Join(" ", parameters.Skip(1));
                shop.Message = message;
                _database.Update(shop);
                player.SendSuccessMessage(
                    $"Updated {(shop.OwnerName == player.User?.Name ? "your" : shop.OwnerName + "'s")} " +
                    $"[c/{Color.LimeGreen.Hex3()}:{shop}] shop message.");
            }
            else if (subcommand.Equals("setprice", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 3)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop setprice <item-name> <price>");
                    return;
                }

                var session = GetOrCreateSession(player);
                if (session.CurrentShop == null)
                {
                    player.SendErrorMessage("You aren't currently in a shop.");
                    return;
                }

                var shop = session.CurrentShop;
                if (shop.OwnerName != player.User?.Name && !player.HasPermission("housing.itemshop.admin"))
                {
                    player.SendErrorMessage(
                        $"You can't modify {shop.OwnerName}'s [c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
                    return;
                }

                var inputItemName = parameters[1];
                var items = TShock.Utils.GetItemByIdOrName(inputItemName);
                if (items.Count > 1)
                {
                    player.SendErrorMessage($"Multiple items matched '{inputItemName}':");
                    TShock.Utils.SendMultipleMatchError(player, items);
                    return;
                }
                if (items.Count == 0)
                {
                    player.SendErrorMessage($"Invalid item '{inputItemName}'.");
                    return;
                }

                var inputPrice = parameters[2];
                if (!Money.TryParse(inputPrice, out var price) || price <= 0)
                {
                    player.SendErrorMessage($"Invalid price '{inputPrice}'.");
                    return;
                }

                shop.UnitPrices[items[0].type] = price;
                _database.Update(shop);
                player.SendSuccessMessage(
                    $"Updated {(shop.OwnerName == player.User?.Name ? "your" : shop.OwnerName + "'s")} " +
                    $"[c/{Color.LimeGreen.Hex3()}:{shop}] shop prices.");
            }
            else
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop 1/2");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop buy <item-index> [amount]");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop close");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop info");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop open");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop remove");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop set <shop-name>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop setmsg <message>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}itemshop setprice <item-name> <price>");
            }
        }

        private void OnGamePostInitialize(EventArgs args)
        {
            _database.Load();
        }

        private void OnGameUpdate(EventArgs args)
        {
            foreach (var player in TShock.Players.Where(p => p?.Active == true))
            {
                var session = GetOrCreateSession(player);
                var house = _database.GetHouse(player.TileX, player.TileY);
                if (house != null && session.CurrentHouse != house)
                {
                    Debug.WriteLine($"DEBUG: {player.Name} entered {house.OwnerName}'s {house} house");
                    player.SendInfoMessage(
                        $"You entered {(house.OwnerName == player.User?.Name ? "your house" : house.OwnerName + "'s house")} " +
                        $"[c/{Color.MediumPurple.Hex3()}:{house}].");
                    if (house.ForSale && house.OwnerName != player.User?.Name)
                    {
                        player.SendInfoMessage(
                            $"This house is on sale for [c/{Color.OrangeRed.Hex3()}:{house.Price}].");
                    }
                }
                else if (session.CurrentHouse != null && house != session.CurrentHouse)
                {
                    Debug.WriteLine(
                        $"DEBUG: {player.Name} left {session.CurrentHouse.OwnerName}'s {session.CurrentHouse} house");
                    player.SendInfoMessage(
                        $"You left {(session.CurrentHouse.OwnerName == player.User?.Name ? "your house" : session.CurrentHouse.OwnerName + "'s house")} " +
                        $"[c/{Color.MediumPurple.Hex3()}:{session.CurrentHouse}].");
                }
                session.CurrentHouse = house;

                var shop = _database.GetShop(player.TileX, player.TileY);
                if (shop != null && session.CurrentShop != shop && shop.Message != null)
                {
                    Debug.WriteLine($"DEBUG: {player.Name} entered {shop.OwnerName}'s {shop} shop");
                    player.SendInfoMessage(shop.Message);
                }
                session.CurrentShop = shop;
            }

            var shops = _database.GetShops();
            foreach (var house in _database.GetHouses())
            {
				var houseConfig = house.GetGroupConfig();

				if (DateTime.UtcNow - house.LastTaxed > houseConfig.TaxPeriod)
                {
                    var account = SEconomyPlugin.Instance?.RunningJournal.GetBankAccountByName(house.OwnerName);
                    if (account == null)
                    {
                        continue;
                    }
					
                    //var isStore = shops.Any(s => house.Rectangle.Contains(s.Rectangle));
					var store = shops.FirstOrDefault(s => house.Rectangle.Contains(s.Rectangle));
					var storeConfig = store != null ? store.GetGroupConfig() : houseConfig;

					var taxRate = store!=null ? storeConfig.StoreTaxRate : houseConfig.TaxRate;
                    var taxCost = (long)Math.Round(house.Area * taxRate) + house.Debt;
                    var payment = (Money)Math.Min(account.Balance, taxCost);
					//account.TransferTo(
					//    SEconomyPlugin.Instance.WorldAccount, payment, BankAccountTransferOptions.IsPayment, "",
					//    $"Taxed for the {house} house");

					taxService.PayTax(account, payment, BankAccountTransferOptions.IsPayment, "", $"Taxed for house {house}");

                    var player = TShock.Players.Where(p => p?.Active == true)
                        .FirstOrDefault(p => p.User?.Name == house.OwnerName);
                    player?.SendInfoMessage($"You were taxed [c/{Color.OrangeRed.Hex3()}:{payment}] for your house " +
                                            $"[c/{Color.MediumPurple.Hex3()}:{house}].");

                    house.Debt = taxCost - payment;
                    if (payment < taxCost)
                    {
                        if (house.Debt > houseConfig.MaxDebtAllowed)
                        {
							_database.Remove(house);

							if (player != null)
							{
								player.SendInfoMessage($"Your house '{house.Name}' has been reclaimed due to excessive debt!");
							}

							continue;
                        }
                    }

                    house.LastTaxed = DateTime.UtcNow;
                    _database.Update(house);
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

                    var shop = _database.GetShops().FirstOrDefault(s => s.ChestX == x && s.ChestY == y);
                    session.CurrentlyViewedShop = shop;
                    if (shop == null)
                    {
                        return;
                    }
                    args.Handled = true;

                    if (shop.OwnerName == player.User?.Name)
                    {
                        Debug.WriteLine($"DEBUG: {player.Name} changing shop at {x}, {y}");
                        shop.IsBeingChanged = true;
                        var chest = new Chest {x = x, y = y};
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
                    else if (shop.OwnerName != player.User?.Name)
                    {
                        if (!shop.IsOpen)
                        {
                            Debug.WriteLine(
                                $"DEBUG: {player.Name} tried to view shop at {shop.ChestX}, {shop.ChestY}");
                            player.SendErrorMessage("This shop is closed.");
                            return;
                        }
                        if (shop.IsBeingChanged)
                        {
                            Debug.WriteLine(
                                $"DEBUG: {player.Name} tried to view shop at {shop.ChestX}, {shop.ChestY}");
                            player.SendErrorMessage("This shop is being changed right now.");
                            return;
                        }

                        Debug.WriteLine($"DEBUG: {player.Name} viewed shop at {shop.ChestX}, {shop.ChestY}");
                        player.SendInfoMessage("Current stock:");
                        var sb = new StringBuilder();
                        for (var i = 0; i < Chest.maxItems; ++i)
                        {
                            var shopItem = shop.Items.FirstOrDefault(si => si.Index == i);
                            if (shopItem?.StackSize > 0)
                            {
                                sb.Append(
                                    $"[{i + 1}:[i/s{shopItem.StackSize},p{shopItem.PrefixId}:{shopItem.ItemId}]] ");
                            }
                            if ((i + 1) % 10 == 0 && sb.Length > 0)
                            {
                                player.SendInfoMessage(sb.ToString());
                                sb.Clear();
                            }
                        }
                        player.SendInfoMessage(
                            $"Use {Commands.Specifier}itemshop buy <item-index> [amount] to buy items.");
                    }
                }
            }
            else if (args.MsgID == PacketTypes.ChestItem)
            {
                var player = TShock.Players[args.Msg.whoAmI];
                var session = GetOrCreateSession(player);
                var shop = session.CurrentlyViewedShop;
                if (shop == null || shop.OwnerName != player.User?.Name)
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
                    _database.Update(shop);
                    args.Handled = true;
                }
            }
            else if (args.MsgID == PacketTypes.ChestOpen)
            {
                var player = TShock.Players[args.Msg.whoAmI];
                var session = GetOrCreateSession(player);
                var shop = session.CurrentlyViewedShop;
                if (shop != null && shop.OwnerName == player.User?.Name)
                {
                    Debug.WriteLine($"DEBUG: {player.Name} finished changing shop at {shop.ChestX}, {shop.ChestY}");
                    shop.IsBeingChanged = false;
                }
            }
            else if (args.MsgID == PacketTypes.TileKill)
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
                            
                            var shop = _database.AddShop(player, session.NextShopName, session.NextShopX,
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

                    if (_database.GetShops().Any(s => s.ChestX == x && s.ChestY == y))
                    {
                        player.SendErrorMessage("You can't remove shop chests.");
                        args.Handled = true;
                        player.SendTileSquare(x, y, 3);
                    }
                }
            }
        }

        private void OnReload(ReloadEventArgs args)
        {
            if (File.Exists(ConfigPath))
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }

			if(Config.Instance != null)
			{
				taxService.IsEnabled = Config.Instance.EnableTaxService;
			}

			_database.Load();
            args.Player.SendSuccessMessage("[Housing] Reloaded config!");
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
            if (args.Who < 0 || args.Who >= Main.maxPlayers)
            {
                return;
            }

            var player = TShock.Players[args.Who];
			var config = Config.Instance.GetGroupConfig(player.Group.Name);
			if (player != null && config.AllowOfflineShops)//!Config.Instance.AllowOfflineShops)
            {
                foreach (var shop in _database.GetShops().Where(s => s.OwnerName == player.User?.Name))
                {
                    shop.IsOpen = false;
                }
            }
        }
    }
}
