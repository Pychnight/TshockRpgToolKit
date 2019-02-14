using Banking;
using Microsoft.Xna.Framework;
using NpcShops.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace NpcShops
{
	public partial class NpcShopsPlugin
	{
		public static void NpcBuy(CommandArgs args)
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

			if (!shop.IsOpen)
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
				var currencyConverter = shopItem.Currency.GetCurrencyConverter();

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

				var purchaseCost = amount * shopItem.UnitPrice;
				var salesTax = Math.Round(purchaseCost * (decimal)shop.SalesTaxRate);
				var itemText = $"[i/s{amount},p{shopItem.PrefixId}:{shopItem.ItemId}]";

				if (purchaseCost > 0)
				{
					var purchaseCostString = currencyConverter.ToString(purchaseCost);
					var salesTaxString = currencyConverter.ToString(salesTax);

					player.SendInfoMessage($"Purchasing {itemText} will cost [c/{Color.OrangeRed.Hex3()}:{purchaseCostString}], " +
											$"with a sales tax of [c/{Color.OrangeRed.Hex3()}:{salesTaxString}].");
				}

				if (shopItem.RequiredItems.Count > 0)
				{
					player.SendInfoMessage(purchaseCost > 0 ? $"{itemText} will also require materials: " : $"{itemText} requires materials: ");
					player.SendInfoMessage(NpcShop.GetMaterialsCostRenderString(shopItem, amount));
				}

				player.SendInfoMessage("Do you wish to proceed? Use /yes or /no.");
				player.AddResponse("yes", args2 =>
				{
					player.AwaitingResponse.Remove("no");
					//var account = SEconomyPlugin.Instance?.GetBankAccount(player);
					var account = BankingPlugin.Instance.GetBankAccount(player, shopItem.Currency.InternalName);
					var totalCost = purchaseCost + salesTax;

					if (account == null || account.Balance < totalCost)
					{
						player.SendErrorMessage($"You do not have enough of a balance to purchase {itemText}.");
						return;
					}
					if (amount > shopItem.StackSize && shopItem.StackSize > 0)
					{
						player.SendErrorMessage("While waiting, the stock changed.");
						return;
					}
					if (!player.HasSufficientMaterials(shopItem, amount))
					{
						player.SendErrorMessage($"You do not have sufficient materials to purchase {itemText}.");
						return;
					}

					var item = new Item();
					item.SetDefaults(shopItem.ItemId);

					var worldAccount = BankingPlugin.Instance.GetBankAccount("Server", shopItem.Currency.InternalName);
					if (account.TryTransferTo(worldAccount, totalCost))
					{
						//deduct materials from player
						player.TransferMaterials(shopItem, amount);

						if (shopItem.StackSize > 0)
							shopItem.StackSize -= amount;

						player.GiveItem(shopItem.ItemId, "", Player.defaultWidth, Player.defaultHeight, amount, shopItem.PrefixId);
						player.SendSuccessMessage($"Purchased {itemText} for { GetPostPurchaseRenderString(shop, shopItem, totalCost, amount) }.");
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
				var currencyConverter = shopCommand.Currency.GetCurrencyConverter();

				if (shopCommand.StackSize == 0) //||
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

				var purchaseCost = amount * shopCommand.UnitPrice;
				var salesTax = Math.Round(purchaseCost * (decimal)shop.SalesTaxRate);
				var commandText = $"{shopCommand.Name} x[c/{Color.OrangeRed.Hex3()}:{amount}]";

				if (purchaseCost > 0)
				{
					var purchaseCostString = currencyConverter.ToString(purchaseCost);
					var salesTaxString = currencyConverter.ToString(salesTax);

					player.SendInfoMessage($"Purchasing {commandText} will cost [c/{Color.OrangeRed.Hex3()}:{purchaseCostString}], " +
											$"with a sales tax of [c/{Color.OrangeRed.Hex3()}:{salesTaxString}].");
				}

				if (shopCommand.RequiredItems.Count > 0)
				{
					player.SendInfoMessage(purchaseCost > 0 ? $"{commandText} will also require materials: " : $"{commandText} requires materials: ");
					player.SendInfoMessage(NpcShop.GetMaterialsCostRenderString(shopCommand, amount));
				}

				player.SendInfoMessage("Do you wish to proceed? Use /yes or /no.");
				player.AddResponse("yes", args2 =>
				{
					player.AwaitingResponse.Remove("no");
					// var account = SEconomyPlugin.Instance?.GetBankAccount(player);
					var account = BankingPlugin.Instance.GetBankAccount(player, shopCommand.Currency.InternalName);
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
					if (!player.HasSufficientMaterials(shopCommand, amount))
					{
						player.SendErrorMessage($"You do not have sufficient materials to purchase {commandText}.");
						return;
					}

					var worldAccount = BankingPlugin.Instance.GetBankAccount("Server", shopCommand.Currency.InternalName);
					if (account.TryTransferTo(worldAccount, totalCost))
					{
						//deduct materials from player
						player.TransferMaterials(shopCommand, amount);

						if (shopCommand.StackSize > 0)
							shopCommand.StackSize -= amount;

						//run purchased commands
						for (var i = 0; i < amount; ++i)
						{
							Console.WriteLine(shopCommand.Command.Replace("$name", player.GetEscapedName()));
							shopCommand.ForceHandleCommand(player);
						}

						player.SendSuccessMessage($"Purchased {commandText} for { GetPostPurchaseRenderString(shop, shopCommand, totalCost, amount) }.");

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
	}
}
