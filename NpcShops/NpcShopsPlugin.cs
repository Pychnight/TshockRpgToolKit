using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Banking;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NpcShops.Shops;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
//using Wolfje.Plugins.SEconomy;
//using Wolfje.Plugins.SEconomy.Journal;

namespace NpcShops
{
	[ApiVersion(2, 1)]
	public sealed class NpcShopsPlugin : TerrariaPlugin
	{
		private const string SessionKey = "NpcShops_Session";

		private static readonly string ConfigPath = Path.Combine("npcshops", "config.json");

		internal static NpcShopsPlugin Instance { get; private set; }

        internal List<NpcShop> NpcShops = new List<NpcShop>();
		internal NpcPauser NpcPauser = new NpcPauser();

		internal CurrencyDefinition Currency { get; private set; }
		
        public NpcShopsPlugin(Main game) : base(game)
        {
			Instance = this;
        }

        public override string Author => "MarioE, Timothy Barela";
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
			ServerApi.Hooks.NetGetData.Register(this, OnNetGetData);
			
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

				ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
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

		private void tryLoad()
		{
			if( BankingPlugin.Instance == null )
			{
				throw new Exception("BankingPlugin not detected. BankingPlugin is required for NpcShopsPlugin.");
			}
			else
			{
				var currencyType = Config.Instance?.CurrencyType;

				if( string.IsNullOrWhiteSpace(currencyType) )
				{
					throw new Exception($"Invalid CurrencyType configured. NpcShopsPlugin requires a configured Currency type to operate.");
				}

				if( !BankingPlugin.Instance.TryGetCurrency(currencyType, out var currency ) )
				{
					throw new Exception($"Unable to find CurrencyType '{Config.Instance.CurrencyType}'. NpcShopsPlugin requires a configured Currency type to operate.");
				}

				Currency = currency;
			}

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

			NpcShops = shops;
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

			if( !shop.IsOpen )
			{
				session.SendClosedMessage(shop);
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

                var purchaseCost = (decimal)(amount * shopItem.UnitPrice);
                var salesTax = (decimal)Math.Round(purchaseCost * (decimal)shop.SalesTaxRate);
                var itemText = $"[i/s{amount},p{shopItem.PrefixId}:{shopItem.ItemId}]";

				if(purchaseCost > 0 )
				{
					player.SendInfoMessage(	$"Purchasing {itemText} will cost [c/{Color.OrangeRed.Hex3()}:{purchaseCost.ToMoneyString()}], " +
											$"with a sales tax of [c/{Color.OrangeRed.Hex3()}:{salesTax.ToMoneyString()}].");
				}

				if(shopItem.RequiredItems.Count>0)
				{
					player.SendInfoMessage( purchaseCost > 0 ? $"{itemText} will also require materials: " : $"{itemText} requires materials: ");
					player.SendInfoMessage(shop.GetMaterialsCostRenderString(shopItem, amount));
				}
				
                player.SendInfoMessage("Do you wish to proceed? Use /yes or /no.");
				player.AddResponse("yes", args2 =>
				{
					player.AwaitingResponse.Remove("no");
					//var account = SEconomyPlugin.Instance?.GetBankAccount(player);
					var account = BankingPlugin.Instance.GetBankAccount(player,Currency.InternalName);
					var totalCost = purchaseCost + salesTax;

					if( account == null || account.Balance < totalCost )
					{
						player.SendErrorMessage($"You do not have enough of a balance to purchase {itemText}.");
						return;
					}
					if( amount > shopItem.StackSize && shopItem.StackSize > 0 )
					{
						player.SendErrorMessage("While waiting, the stock changed.");
						return;
					}
					if( !player.HasSufficientMaterials(shopItem, amount) )
					{
						player.SendErrorMessage($"You do not have sufficient materials to purchase {itemText}.");
						return;
					}

					var item = new Item();
					item.SetDefaults(shopItem.ItemId);
										
					var worldAccount = BankingPlugin.Instance.GetBankAccount("Server", Currency.InternalName);
					if( account.TryTransferTo(worldAccount,totalCost))
					{
						//deduct materials from player
						player.TransferMaterials(shopItem, amount);

						if( shopItem.StackSize > 0 )
							shopItem.StackSize -= amount;

						player.GiveItem(shopItem.ItemId, "", Player.defaultWidth, Player.defaultHeight, amount, shopItem.PrefixId);
						player.SendSuccessMessage($"Purchased {itemText} for { getPostPurchaseRenderString(shop, shopItem, totalCost, amount) }.");
					}
					else
					{
						player.SendErrorMessage($"Transfer of funds failed for {itemText}.");
					}
										
					//refresh the shop dispay for player, after some time so they can the transaction messages.
					shop.ShowTo(player, 2000);

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
                if (shopCommand.StackSize == 0 ) //||
                    //shopCommand.PermissionRequired != null && !player.HasPermission(shopCommand.PermissionRequired))
                {
                    player.SendErrorMessage($"Invalid index '{inputIndex}'.");
                    return;
                }

                if (amount > shopCommand.StackSize && shopCommand.StackSize > 0)
                {
                    player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
                    return;
                }

                var purchaseCost = (decimal)(amount * shopCommand.UnitPrice);
                var salesTax = (decimal)Math.Round(purchaseCost * (decimal)shop.SalesTaxRate);
                var commandText = $"{shopCommand.Name} x[c/{Color.OrangeRed.Hex3()}:{amount}]";
                
				if( purchaseCost > 0 )
				{
					player.SendInfoMessage($"Purchasing {commandText} will cost [c/{Color.OrangeRed.Hex3()}:{purchaseCost.ToMoneyString()}], " +
											$"with a sales tax of [c/{Color.OrangeRed.Hex3()}:{salesTax.ToMoneyString()}].");
				}

				if( shopCommand.RequiredItems.Count > 0 )
				{
					player.SendInfoMessage(purchaseCost > 0 ? $"{commandText} will also require materials: " : $"{commandText} requires materials: ");
					player.SendInfoMessage(shop.GetMaterialsCostRenderString(shopCommand, amount));
				}
				
				player.SendInfoMessage("Do you wish to proceed? Use /yes or /no.");
                player.AddResponse("yes", args2 =>
                {
                    player.AwaitingResponse.Remove("no");
                   // var account = SEconomyPlugin.Instance?.GetBankAccount(player);
					var account = BankingPlugin.Instance.GetBankAccount(player, Currency.InternalName);
					var totalCost = purchaseCost + salesTax;

					if (account == null || account.Balance < totalCost)
                    {
                        player.SendErrorMessage($"You do not have enough of a balance to purchase {commandText}.");
                        return;
                    }
                    if (amount > shopCommand.StackSize && shopCommand.StackSize > 0)
                    {
                        player.SendErrorMessage("While waiting, the stock changed.");
                        return;
                    }
					if( !player.HasSufficientMaterials(shopCommand, amount) )
					{
						player.SendErrorMessage($"You do not have sufficient materials to purchase {commandText}.");
						return;
					}
					
					var worldAccount = BankingPlugin.Instance.GetBankAccount("Server", Currency.InternalName);
					if( account.TryTransferTo(worldAccount, totalCost) )
					{
						//deduct materials from player
						player.TransferMaterials(shopCommand, amount);

						if( shopCommand.StackSize > 0 )
							shopCommand.StackSize -= amount;

						//run purchased commands
						for( var i = 0; i < amount; ++i )
						{
							Console.WriteLine(shopCommand.Command.Replace("$name", player.GetEscapedName()));
							shopCommand.ForceHandleCommand(player);
						}

						player.SendSuccessMessage($"Purchased {commandText} for { getPostPurchaseRenderString(shop, shopCommand, totalCost, amount) }.");

					}
					else
					{
						player.SendErrorMessage($"Transfer of funds failed for {commandText}.");
					}

					//refresh the shop dispay for player, after some time so they can the transaction messages.
					shop.ShowTo(player, 2000);

				});
                player.AddResponse("no", args2 =>
                {
                    player.AwaitingResponse.Remove("yes");
                    player.SendInfoMessage("Canceled purchase.");
                });
            }
        }

		private string getPostPurchaseRenderString( NpcShop shop, ShopProduct product, decimal totalCost, int quantity )
		{
			var sb = new StringBuilder();
			
			if( totalCost > 0 )
				sb.Append($"[c/{ Color.OrangeRed.Hex3()}:{totalCost.ToMoneyString()}]");
			
			if( product.RequiredItems.Count > 0 )
			{
				var fragment = shop.GetMaterialsCostRenderString(product, quantity); ;
				sb.Append(totalCost > 0 ? $" and {fragment}" : fragment);
			}

			return sb.ToString();
		}
		
        private void OnGamePostInitialize(EventArgs args)
        {
			tryLoad();
	    }
		
		private void OnNetGetData(GetDataEventArgs args)
		{
			var msgId = args.MsgID;

			if(msgId == PacketTypes.NpcTalk)
			{
				//Debug.Print("NpcShopTalk!!!"); // This happens when we right click the merchant

				//following is based off of https://github.com/MarioE/NoCheat/blob/master/NoCheat/ItemSpawning/Module.cs
				var player = TShock.Players[args.Msg.whoAmI];

				// Ignore packets sent when the client is syncing.
				if( player.State < 10 )
					return;

				using( var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)) )
				{
					reader.ReadByte();
					var npcIndex = reader.ReadInt16();
					var npcType = npcIndex < 0 ? 0 : Main.npc[npcIndex].type;
					var npc = npcIndex < 0 ? null : Main.npc[npcIndex];
					
					if(NpcShop.NpcToShopMap.ContainsKey(npcType))
					{
						//player.SendData(PacketTypes.NpcTalk, "", player.Index, npcIndex);
						player.SendData(PacketTypes.NpcTalk, "", player.Index, -1);
						//player.SendData(PacketTypes.NpcUpdate, "", npcIndex);

						if(npc!=null && npc.active)
						{
							NpcPauser.Pause(npc, Config.Instance.ShopNpcPauseDuration);
						}

						var session = GetOrCreateSession(player);
						session.CurrentShopkeeperNpcIndex = npcIndex;
						//session.CurrentShop = npcShop;
						session.shopKeeperClickedHack = true;
												
						args.Handled = true;
						//Debug.Print($"Shop mapped to npc index {npcIndex}.");
					}
				}
			}
		}

		private void OnGameUpdate(EventArgs args)
        {
            foreach (var player in TShock.Players.Where(p => p?.Active == true))
            {
                var session = GetOrCreateSession(player);
				session.Update();
			}

            foreach (var shop in NpcShops)
            {
                shop.TryRestock();
            }

			NpcPauser.OnGameUpdate();
		}

        private void OnReload(ReloadEventArgs args)
        {
			NpcPauser.UnpauseAll();

			//clear sessions
			TShock.Players.Where(tp => tp?.Active == true)
							.ForEach(tp => tp.SetData<Session>(SessionKey, null));

			tryLoadConfig();
            tryLoad();

            args.Player.SendSuccessMessage("[NpcShops] Reloaded config!");
        }
    }
}
