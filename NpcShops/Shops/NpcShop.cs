using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace NpcShops.Shops
{
    /// <summary>
    ///     Represents an NPC shop.
    /// </summary>
    public class NpcShop
    {
        private static readonly Dictionary<string, double> TimeToNumber =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["day"] = 4.5,
                ["noon"] = 12.0,
                ["night"] = 19.5,
                ["midnight"] = 0.0
            };

        private readonly NpcShopDefinition _definition;

        private DateTime _lastRestock = DateTime.UtcNow;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NpcShop" /> class with the specified definition.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        public NpcShop(NpcShopDefinition definition)
        {
			_definition = definition ?? throw new ArgumentNullException("NpcShopDefinition cannot be null.");

			var region = TShock.Regions.GetRegionByName(definition.RegionName);
				
			if( region == null )
				throw new Exception($"Could not find region named {definition.RegionName}.");

			Rectangle = region.Area;

			ShopCommands = definition.ShopCommands.Select(sc => new ShopCommand(sc)).ToList();
			ShopItems = definition.ShopItems.Select(si => new ShopItem(si)).ToList();
        }

        /// <summary>
        ///     Gets the closing time.
        /// </summary>
        public string ClosingTime => _definition.ClosingTime;

        /// <summary>
        ///     Gets a value indicating whether the shop is open.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (OpeningTime == null || ClosingTime == null)
                {
                    return true;
                }

                var time = Main.time / 3600.0;
                time += 4.5;
                if (!Main.dayTime)
                {
                    time += 15.0;
                }
                time %= 24.0;

                var openingTime = TimeToNumber[OpeningTime];
                var closingTime = TimeToNumber[ClosingTime];
                return openingTime < closingTime
                    ? openingTime < time && time < closingTime
                    : time > openingTime || time < closingTime;
            }
        }

        /// <summary>
        ///     Gets the message.
        /// </summary>
        public string Message => _definition.Message;

        /// <summary>
        ///     Gets the opening time.
        /// </summary>
        public string OpeningTime => _definition.OpeningTime;

        /// <summary>
        ///     Gets the rectangle.
        /// </summary>
        public Rectangle Rectangle { get; }

        /// <summary>
        ///     Gets the restock time.
        /// </summary>
        public TimeSpan RestockTime => _definition.RestockTime;

        /// <summary>
        ///     Gets the sales tax rate.
        /// </summary>
        public double SalesTaxRate => _definition.SalesTaxRate;

        /// <summary>
        ///     Gets the list of shop commands.
        /// </summary>
        public IList<ShopCommand> ShopCommands { get; }

        /// <summary>
        ///     Gets the list of shop items.
        /// </summary>
        public IList<ShopItem> ShopItems { get; }

        /// <summary>
        ///     Shows the shop to the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        public void ShowTo(TSPlayer player)
        {
            Debug.Assert(player != null, "Player must not be null.");

            player.SendInfoMessage("Current stock:");
            var sb = new StringBuilder();
            for (var i = 0; i < ShopItems.Count; ++i)
            {
                var shopItem = ShopItems[i];
				if (shopItem.StackSize != 0 &&
					(shopItem.PermissionRequired == null || player.HasPermission(shopItem.PermissionRequired)))
                {
					//doesn't look like we can make item image subscripts to render 0 or 1, so for now we ditch the subscripts altogether.

					//sb.Append(shopItem.StackSize < 0
					//              ? $"[{i + 1}:[i/p{shopItem.PrefixId}:{shopItem.ItemId}]] "
					//              : $"[{i + 1}:[i/s{shopItem.StackSize},p{shopItem.PrefixId}:{shopItem.ItemId}]] ");
										
					string stock; // = shopItem.StackSize;

					if( shopItem.StackSize == -1 || shopItem.StackSize > 99 )
						stock = "99+";
					else
						stock = shopItem.StackSize.ToString();

					sb.Append($"[{i + 1}:[i/p{shopItem.PrefixId}:{shopItem.ItemId}]x{stock}] ");
				}
                if (((i + 1) % 10 == 0 || i == ShopItems.Count - 1) && sb.Length > 0)
                {
                    player.SendInfoMessage(sb.ToString());
                    sb.Clear();
                }
            }

            sb.Clear();
            for (var i = 0; i < ShopCommands.Count; ++i)
            {
                var shopCommand = ShopCommands[i];
                if (shopCommand.StackSize != 0 )// &&
                    //(shopCommand.PermissionRequired == null || player.HasPermission(shopCommand.PermissionRequired)))
                {
					string stock;

					if( shopCommand.StackSize == -1 || shopCommand.StackSize > 99 )
						stock = "99+";
					else
						stock = shopCommand.StackSize.ToString();

                    sb.Append($"[{i + 1 + ShopItems.Count}: {shopCommand.Name} x{stock}] ");
                }
                if (((i + 1) % 5 == 0 || i == ShopCommands.Count - 1) && sb.Length > 0)
                {
                    player.SendInfoMessage(sb.ToString());
                    sb.Clear();
                }
            }
            player.SendInfoMessage(
                $"Use {Commands.Specifier}npcbuy <index> [amount] to buy items or commands.");
        }

        /// <summary>
        ///     Tries restocking the shop.
        /// </summary>
        public void TryRestock()
        {
            if (DateTime.UtcNow - _lastRestock < RestockTime)
            {
                return;
            }

            Debug.WriteLine("DEBUG: Restocking shop");
            _lastRestock = DateTime.UtcNow;
            foreach (var shopItem in ShopItems)
            {
                shopItem.Restock();
            }
            foreach (var shopCommand in ShopCommands)
            {
                shopCommand.Restock();
            }
        }
    }
}
