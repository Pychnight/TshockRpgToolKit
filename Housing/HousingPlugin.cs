using System;
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
        private DatabaseManager _database;

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
            _connection = new SqliteConnection($"uri=file://{SqlitePath},Version=3");
            _database = new DatabaseManager(_connection);

            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.NetGetData.Register(this, OnNetGetData, 1);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);

            Commands.ChatCommands.Add(new Command("housing.house", HouseCmd, "house"));
            Commands.ChatCommands.Add(new Command("housing.itemshop", ItemShop, "itemshop"));
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
            var subcommand = parameters.Count > 0 ? parameters[0] : "";
            if (subcommand.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                player.AwaitingTempPoint = 1;
                player.SendInfoMessage("Hit a block to set the first point.");
            }
            else if (subcommand.Equals("2", StringComparison.OrdinalIgnoreCase))
            {
                player.AwaitingTempPoint = 2;
                player.SendInfoMessage("Hit a block to set the first point.");
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
            else if (subcommand.Equals("disallow", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count != 2)
                {
                    player.SendErrorMessage($"Syntax: {Commands.Specifier}house allow <username>");
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
                    $"{(house.OwnerName == player.User?.Name ? "your" : house.OwnerName + "'s")} " +
                    $"[c/{Color.MediumPurple.Hex3()}:{house}] house.");
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
                    player.SendInfoMessage($"Debt: {house.Debt}, Last taxed: {house.LastTaxed}");
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
                var purchaseCost = (Money)Math.Round(rectangle.Width * rectangle.Height * Config.Instance.PurchaseRate);
                if (purchaseCost > 0)
                {
                    var taxCost = (Money)Math.Round(rectangle.Width * rectangle.Height * Config.Instance.TaxRate);
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
                        player.SendSuccessMessage($"Purchased the [c/{Color.MediumPurple.Hex3()}:{house}] house.");
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
                    player.SendSuccessMessage($"Added the [c/{Color.MediumPurple.Hex3()}:{house}] house.");
                }
            }
            else
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house 1/2");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house allow <player-name>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house disallow <username>");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house info");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house remove");
                player.SendErrorMessage($"Syntax: {Commands.Specifier}house set <house-name>");
            }
        }

        private void ItemShop(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
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

                var inputAmount = parameters.Count == 3 ? parameters[2] : shopItem.StackSize.ToString();
                if (!int.TryParse(inputAmount, out var amount) || amount < 1 || amount > shopItem.StackSize)
                {
                    player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
                    return;
                }

                var purchaseCost = (Money)(amount * shopItem.UnitPrice);
                var salesTax = (Money)Math.Round(purchaseCost * Config.Instance.SalesTaxRate);
                var itemId = shopItem.ItemId;
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

                    var item = new Item();
                    item.SetDefaults(shopItem.ItemId);
                    var account2 = SEconomyPlugin.Instance.RunningJournal.GetBankAccountByName(shop.OwnerName);
                    account.TransferTo(
                        account2, purchaseCost, BankAccountTransferOptions.IsPayment,
                        "", $"Purchased {item.Name} x{amount}");
                    account.TransferTo(
                        SEconomyPlugin.Instance.WorldAccount, salesTax, BankAccountTransferOptions.IsPayment,
                        "", $"Sales tax for {item.Name} x{amount}");

                    shopItem.StackSize -= amount;
                    _database.Update(shop);

                    player.GiveItem(
                        shopItem.ItemId, "", Player.defaultWidth, Player.defaultHeight, amount, shopItem.PrefixId);
                    player.SendSuccessMessage($"Purchased {itemText}.");
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
                        $"You can't close {shop.OwnerName}'s [c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
                    return;
                }

                shop.IsOpen = false;
                _database.Update(shop);
                player.SendSuccessMessage(
                    $"Closed {(shop.OwnerName == player.User?.Name ? "your" : shop.OwnerName + "'s")} " +
                    $"[c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
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
                        $"You can't open {shop.OwnerName}'s [c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
                    return;
                }

                shop.IsOpen = true;
                _database.Update(shop);
                player.SendSuccessMessage(
                    $"Opened {(shop.OwnerName == player.User?.Name ? "your" : shop.OwnerName + "'s")} " +
                    $"[c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
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
                        $"You can't remove {shop.OwnerName}'s [c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
                    return;
                }

                _database.Remove(shop);
                player.SendSuccessMessage(
                    $"Removed {(shop.OwnerName == player.User?.Name ? "your" : shop.OwnerName + "'s")} " +
                    $"[c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
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

                var point1 = player.TempPoints[0];
                var point2 = player.TempPoints[1];
                var inputShopName = parameters[1];
                var x = Math.Min(point1.X, point2.X);
                var y = Math.Min(point1.Y, point2.Y);
                var x2 = Math.Max(point1.X, point2.X);
                var y2 = Math.Max(point1.Y, point2.Y);
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
                player.SendInfoMessage("Open a chest to serve as the item shop chest.");
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

                foreach (var shopItem in shop.Items.Where(i => i.ItemId == items[0].type))
                {
                    shopItem.UnitPrice = price;
                }
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
                        $"You entered {(house.OwnerName == player.User?.Name ? "your" : house.OwnerName + "'s")} " +
                        $"[c/{Color.MediumPurple.Hex3()}:{house}] house.");
                }
                else if (session.CurrentHouse != null && house != session.CurrentHouse)
                {
                    Debug.WriteLine(
                        $"DEBUG: {player.Name} left {session.CurrentHouse.OwnerName}'s {session.CurrentHouse} house");
                    player.SendInfoMessage(
                        $"You left {(session.CurrentHouse.OwnerName == player.User?.Name ? "your" : session.CurrentHouse.OwnerName + "'s")} " +
                        $"[c/{Color.MediumPurple.Hex3()}:{session.CurrentHouse}] house.");
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
                if (DateTime.UtcNow - house.LastTaxed > Config.Instance.TaxPeriod)
                {
                    var account = SEconomyPlugin.Instance?.RunningJournal.GetBankAccountByName(house.OwnerName);
                    if (account == null)
                    {
                        continue;
                    }

                    var isStore = shops.Any(s => house.Rectangle.Contains(s.Rectangle));
                    var taxRate = isStore ? Config.Instance.StoreTaxRate : Config.Instance.TaxRate;
                    var taxCost = (Money)Math.Round(house.Area * taxRate) + house.Debt;
                    var payment = Math.Min(account.Balance, taxCost);
                    account.TransferTo(
                        SEconomyPlugin.Instance.WorldAccount, payment, BankAccountTransferOptions.IsPayment, "",
                        $"Taxed for the {house} house");
                    house.Debt = taxCost - payment;
                    if (payment < taxCost)
                    {
                        if (house.Debt > Config.Instance.MaxDebtAllowed)
                        {
                            _database.Remove(house);
                            continue;
                        }
                    }

                    house.LastTaxed = DateTime.UtcNow;
                    _database.Update(house);
                }
            }
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player != null && !Config.Instance.AllowOfflineShops)
            {
                foreach (var shop in _database.GetShops().Where(s => s.OwnerName == player.User?.Name))
                {
                    shop.IsOpen = false;
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

                    if (session.NextShopHouse != null)
                    {
                        if (!session.NextShopHouse.Rectangle.Contains(x, y))
                        {
                            player.SendErrorMessage("Your house must contain your item shop chest.");
                            return;
                        }

                        var shop = _database.AddShop(player, session.NextShopName, session.NextShopX,
                                                     session.NextShopY, session.NextShopX2, session.NextShopY2, x, y);
                        player.SendSuccessMessage($"Added the [c/{Color.LimeGreen.Hex3()}:{shop}] shop.");
                        player.SendInfoMessage("Use /itemshop open and /itemshop close to open and close your shop.");
                        args.Handled = true;
                        session.NextShopHouse = null;
                    }
                    else
                    {
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

                    var item = new Item();
                    item.SetDefaults(itemId);
                    var unitPrice = (Money)(item.value / 5);

                    var shopItem = shop.Items.FirstOrDefault(i => i.Index == itemIndex);
                    if (shopItem == null)
                    {
                        shop.Items.Add(new ShopItem(itemIndex, itemId, stackSize, prefixId, unitPrice));
                    }
                    else
                    {
                        shopItem.ItemId = itemId;
                        shopItem.StackSize = stackSize;
                        shopItem.PrefixId = prefixId;
                        shopItem.UnitPrice = unitPrice;
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
        }

        private void OnReload(ReloadEventArgs args)
        {
            if (File.Exists(ConfigPath))
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            _database.Load();
            args.Player.SendSuccessMessage("[Housing] Reloaded config!");
        }
    }
}
