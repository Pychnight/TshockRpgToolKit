using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public static ConcurrentDictionary<int, NpcShop> NpcToShopMap { get; private set; } = new ConcurrentDictionary<int, NpcShop>();

        private readonly NpcShopDefinition _definition;

        private DateTime _lastRestock = DateTime.UtcNow;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NpcShop" /> class with the specified definition.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        public NpcShop(NpcShopDefinition definition)
        {
			_definition = definition ?? throw new ArgumentNullException("NpcShopDefinition cannot be null.");

			if(!string.IsNullOrWhiteSpace(definition.RegionName))
			{
				var region = TShock.Regions.GetRegionByName(definition.RegionName);

				if( region == null )
					throw new Exception($"Could not find region named {definition.RegionName}.");

				Rectangle = region.Area;
			}
			else
			{
				//ensure against nre's... create a dummy rectangle that will never be hit
				Rectangle = new Rectangle(-1, -1, 0, 0);
			}

			if( definition.OverrideNpcTypes!=null)
			{
				foreach( var npcType in definition.OverrideNpcTypes )
					NpcToShopMap[npcType] = this;
			}

			//we have to create our products, and make sure they are in a valid state before we add them to the shop.
			ShopCommands = definition.ShopCommands.Select(sc => new ShopCommand(sc))
													.Where( sc=> sc.IsValid )
													.ToList();

			ShopItems = definition.ShopItems.Select(si => new ShopItem(si))
											.Where( si => si.IsValid)
											.ToList();
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
		///     Gets the closed message.
		/// </summary>
		public string ClosedMessage => _definition.ClosedMessage;

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
					sb.Append($"[{i + 1}:{GetItemRenderString(shopItem.ItemId,shopItem.PrefixId, shopItem.StackSize)}] ");
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
					sb.Append($"[{i + 1 + ShopItems.Count}: {shopCommand.Name} x{GetQuantityRenderString(shopCommand.StackSize)}] ");
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
		/// Shows the shop to the specified player, after a specified amount of time.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		/// <param name="delay">Delay in milliseconds.</param>
		public void ShowTo(TSPlayer player, int delay)
		{
			if( delay == -1 )
				delay = 1;//never wait indefinitely

			Task.Delay(delay).ContinueWith(t =>
			{
				ShowTo(player);
			});
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

		public string GetQuantityRenderString(int quantity)
		{
			string stock;

			if( quantity == -1 || quantity > 99 )
				stock = "99+";
			else
				stock = quantity.ToString();

			return $"[c/{Color.OrangeRed.Hex3()}:{stock}]";
		}

		public string GetItemRenderString(int itemId, int itemPrefixId, int quantity)
		{
			return $"[i/p{itemPrefixId}:{itemId}]x{GetQuantityRenderString(quantity)}";
		}

		public string GetMaterialsCostRenderString(ShopProduct product, int quantity)
		{
			var result = "";

			foreach(var reqItem in product.RequiredItems)
			{
				if(reqItem.StackSize>0)
				{
					result += GetItemRenderString(reqItem.ItemId, reqItem.PrefixId, reqItem.StackSize * quantity) + " ";
				}
			}

			return result;
		}
    }
}
