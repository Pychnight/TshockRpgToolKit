using NpcShops.Shops;
using System;
using System.Diagnostics;
using TShockAPI;
//using Wolfje.Plugins.SEconomy;

namespace NpcShops
{
    /// <summary>
    ///     Provides extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Gets the escaped name for the specified player, suitable for use in commands.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <returns>The escaped name.</returns>
        public static string GetEscapedName(this TSPlayer player)
        {
            Debug.Assert(player != null, "Player must not be null.");

            // First, we need to replace all instances of \\ with \\\\. This is because otherwise, the TShock command
            // system would treat the \\ as an escaped \. Then we need to replace \" with \\" and \(space) with
            // \\(space). Then we escape quotes.
            var name = player.Name.Replace(@"\\", @"\\\\");
            name = name.Replace(@"\""", @"\\""");
            name = name.Replace(@"\ ", @"\\ ");
            name = name.Replace(@"""", @"\""");
            return name;
        }
		
		const int MaxInventorySlot = 179;

		public static bool HasSufficientMaterials(this TSPlayer player, ShopProduct product, int quantity)
		{
			var tplayer = player.TPlayer;
			var inventory = tplayer.inventory;

			foreach( var requiredItem in product.RequiredItems )
			{
				var total = 0;

				foreach( var playerItem in inventory )
				{
					if( playerItem.active && playerItem.type == requiredItem.ItemId )
						total += playerItem.stack;
				}

				if( total < requiredItem.StackSize * quantity )
					return false;
			}

			return true;
		}

		public static void TransferMaterials(this TSPlayer player, ShopProduct product, int quantity)
		{
			var tplayer = player.TPlayer;
			//var inventory = player.PlayerData.inventory;//tplayer.inventory;
			var inventory = tplayer.inventory;

			foreach( var requiredItem in product.RequiredItems )
			{
				var needed = requiredItem.StackSize;
				var total = 0;

				for( var i = 0; i < MaxInventorySlot; i++ )//playerItem in inventory )
				{
					var playerItem = inventory[i];

					if( playerItem.active && playerItem.type == requiredItem.ItemId && playerItem.stack > 0 )
					//if( playerItem.NetId == requiredItem.ItemId && playerItem.Stack > 0 )
					{
						if( total + playerItem.stack > needed )
						{
							//take portion of stack
							var portion = needed - total;
							playerItem.stack -= portion;
							total += portion;
						}
						else
						{
							//take whole stack
							var portion = playerItem.stack;
							playerItem.stack = 0;
							playerItem.active = false;
							total += portion;
						}
					}

					TSPlayer.All.SendData(PacketTypes.PlayerSlot, "", player.Index, i, playerItem.stack, playerItem.prefix, 0);

					if( total == needed )
						break;
				}

				if( total != needed )
					throw new Exception("Total != needed. This should never happen.");

				//if( total < requiredItem.StackSize * quantity )
				//	return false;
			}
		}
	}
}
