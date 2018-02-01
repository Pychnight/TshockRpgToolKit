using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NpcShops.Shops;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace NpcShops
{
	[ApiVersion(2, 1)]
	public sealed class NpcShopsPlugin : TerrariaPlugin
	{
		private const string SessionKey = "NpcShops_Session";

		private static readonly string ConfigPath = Path.Combine("npcshops", "config.json");

		internal static NpcShopsPlugin Instance { get; private set; }

        private List<NpcShop> _npcShops = new List<NpcShop>();

        public NpcShopsPlugin(Main game) : base(game)
        {
			Instance = this;
        }

        public override string Author => "MarioE";
        public override string Description => "Adds an NPC shop system.";
        public override string Name => "NpcShops";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override void Initialize()
        {
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif

			tryLoadConfig();

            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize, int.MinValue);
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);

            Commands.ChatCommands.Add(new Command("npcshops.npcbuy", NpcBuy, "npcbuy"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));

                GeneralHooks.ReloadEvent -= OnReload;
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
            }
            base.Dispose(disposing);
        }

		public void LogPrint(string message, TraceLevel level)
		{
			ServerApi.LogWriter.PluginWriteLine(this, message, level);
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

		private void tryLoadConfig()
		{
			try
			{
				Directory.CreateDirectory("npcshops");
				if( File.Exists(ConfigPath) )
				{
					Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
				}
			}
			catch(Exception ex)
			{
				LogPrint("An error occured while trying to load shop config.", TraceLevel.Error);
				LogPrint(ex.Message, TraceLevel.Error);
			}
		}

		private void tryLoadShops()
		{
			var shops = new List<NpcShop>();
			var files = Directory.EnumerateFiles("npcshops", "*.shop", SearchOption.AllDirectories);
			
			foreach( var file in files )
			{
				var definition = NpcShopDefinition.TryLoadFromFile(file);
				
				if( definition != null )
				{
					try
					{
						var shop = new NpcShop(definition);
						shops.Add(shop);
					}
					catch( Exception ex )
					{
						LogPrint("An error occured while trying to create NpcShop.", TraceLevel.Error);
						LogPrint(ex.Message, TraceLevel.Error);
					}
				}
			}

			_npcShops = shops;
		}

        private void NpcBuy(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1 && parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}npcbuy <index> [amount]");
                return;
            }

            var session = GetOrCreateSession(player);
            var shop = session.CurrentShop;
            if (shop == null)
            {
                player.SendErrorMessage("You aren't currently in a shop.");
                return;
            }

            var inputIndex = parameters[0];
            if (!int.TryParse(inputIndex, out var index) || index < 1 ||
                index > shop.ShopItems.Count + shop.ShopCommands.Count)
            {
                player.SendErrorMessage($"Invalid index '{inputIndex}'.");
                return;
            }
            index -= 1;

            var inputAmount = parameters.Count == 2 ? parameters[1] : "1";
            if (!int.TryParse(inputAmount, out var amount) || amount < 1)
            {
                player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
                return;
            }

            if (index < shop.ShopItems.Count)
            {
                var shopItem = shop.ShopItems[index];
                if (shopItem.StackSize == 0 ||
                    shopItem.PermissionRequired != null && !player.HasPermission(shopItem.PermissionRequired))
                {
                    player.SendErrorMessage($"Invalid index '{inputIndex}'.");
                    return;
                }

                if (amount > shopItem.StackSize && shopItem.StackSize > 0 || amount > shopItem.MaxStackSize)
                {
                    player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
                    return;
                }

                var purchaseCost = (Money)(amount * shopItem.UnitPrice);
                var salesTax = (Money)Math.Round(purchaseCost * shop.SalesTaxRate);
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
                    if (amount > shopItem.StackSize && shopItem.StackSize > 0)
                    {
                        player.SendErrorMessage("While waiting, the stock changed.");
                        return;
                    }

                    var item = new Item();
                    item.SetDefaults(shopItem.ItemId);
                    account.TransferTo(
                        SEconomyPlugin.Instance.WorldAccount, purchaseCost, BankAccountTransferOptions.IsPayment,
                        "", $"Purchased {item.Name} x{amount}");
                    account.TransferTo(
                        SEconomyPlugin.Instance.WorldAccount, salesTax, BankAccountTransferOptions.IsPayment,
                        "", $"Sales tax for {item.Name} x{amount}");

                    if (shopItem.StackSize > 0)
                    {
                        shopItem.StackSize -= amount;
                    }

                    player.GiveItem(
                        shopItem.ItemId, "", Player.defaultWidth, Player.defaultHeight, amount, shopItem.PrefixId);
                    player.SendSuccessMessage(
                        $"Purchased {itemText} for [c/{Color.OrangeRed.Hex3()}:{(Money)(purchaseCost + salesTax)}].");
                });
                player.AddResponse("no", args2 =>
                {
                    player.AwaitingResponse.Remove("yes");
                    player.SendInfoMessage("Canceled purchase.");
                });
            }
            else
            {
                index -= shop.ShopItems.Count;
                var shopCommand = shop.ShopCommands[index];
                if (shopCommand.StackSize == 0 ||
                    shopCommand.PermissionRequired != null && !player.HasPermission(shopCommand.PermissionRequired))
                {
                    player.SendErrorMessage($"Invalid index '{inputIndex}'.");
                    return;
                }

                if (amount > shopCommand.StackSize && shopCommand.StackSize > 0)
                {
                    player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
                    return;
                }

                var purchaseCost = (Money)(amount * shopCommand.UnitPrice);
                var salesTax = (Money)Math.Round(purchaseCost * shop.SalesTaxRate);
                var commandText = $"{shopCommand.Name} x{amount}";
                player.SendInfoMessage(
                    $"Purchasing {commandText} will cost [c/{Color.OrangeRed.Hex3()}:{purchaseCost}], " +
                    $"with a sales tax of [c/{Color.OrangeRed.Hex3()}:{salesTax}].");
                player.SendInfoMessage("Do you wish to proceed? Use /yes or /no.");
                player.AddResponse("yes", args2 =>
                {
                    player.AwaitingResponse.Remove("no");
                    var account = SEconomyPlugin.Instance?.GetBankAccount(player);
                    if (account == null || account.Balance < purchaseCost + salesTax)
                    {
                        player.SendErrorMessage($"You do not have enough of a balance to purchase {commandText}.");
                        return;
                    }
                    if (amount > shopCommand.StackSize && shopCommand.StackSize > 0)
                    {
                        player.SendErrorMessage("While waiting, the stock changed.");
                        return;
                    }

                    account.TransferTo(
                        SEconomyPlugin.Instance.WorldAccount, purchaseCost, BankAccountTransferOptions.IsPayment,
                        "", $"Purchased {commandText}");
                    account.TransferTo(
                        SEconomyPlugin.Instance.WorldAccount, salesTax, BankAccountTransferOptions.IsPayment,
                        "", $"Sales tax for {commandText}");
                    for (var i = 0; i < amount; ++i)
                    {
                        Console.WriteLine(shopCommand.Command.Replace("$name", player.GetEscapedName()));
						shopCommand.ForceHandleCommand(player);
                    }
                    if (shopCommand.StackSize > 0)
                    {
                        shopCommand.StackSize -= amount;
                    }
                    player.SendSuccessMessage(
                        $"Purchased {commandText} for [c/{Color.OrangeRed.Hex3()}:{(Money)(purchaseCost + salesTax)}].");
                });
                player.AddResponse("no", args2 =>
                {
                    player.AwaitingResponse.Remove("yes");
                    player.SendInfoMessage("Canceled purchase.");
                });
            }
        }

        private void OnGamePostInitialize(EventArgs args)
        {
			tryLoadShops();
        }

        private void OnGameUpdate(EventArgs args)
        {
            foreach (var player in TShock.Players.Where(p => p?.Active == true))
            {
                var session = GetOrCreateSession(player);
                var shop = _npcShops.FirstOrDefault(ns => ns.Rectangle.Contains(player.TileX, player.TileY));
                if (shop != null && session.CurrentShop != shop)
                {
                    Debug.WriteLine($"DEBUG: {player.Name} entered shop");
                    if (shop.IsOpen)
                    {
                        if (shop.Message != null)
                        {
                            player.SendInfoMessage(shop.Message);
                        }
                        shop.ShowTo(player);
                    }
                    else
                    {
                        player.SendErrorMessage($"This shop is closed. Come back at {shop.OpeningTime}.");
                    }
                }
                session.CurrentShop = shop;
            }

            foreach (var shop in _npcShops)
            {
                shop.TryRestock();
            }
        }

        private void OnReload(ReloadEventArgs args)
        {
			tryLoadConfig();
            tryLoadShops();

            args.Player.SendSuccessMessage("[NpcShops] Reloaded config!");
        }
    }
}
