using Housing.Models;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace Housing
{
	public static class ShopExtensions
	{
		/// <summary>
		/// Shows the Shop's stock menu, after a delay, and only if the shop is open and/or not being changed.
		/// </summary>
		/// <param name="shop"></param>
		/// <param name="player"></param>
		/// <param name="delay"></param>
		public static void TryShowStock(this Shop shop, TSPlayer player, int delay)
		{
			if(delay<0)
				delay = 0;

			Task.Delay(delay).ContinueWith(t =>
			{
				TryShowStock(shop, player);
			});
		}

		/// <summary>
		///	Shows the shop's stock menu, if the shop is open and/or not being changed.
		/// </summary>
		/// <param name="shop">Shop</param>
		/// <param name="player">Player</param>
		public static void TryShowStock(this Shop shop, TSPlayer player)
		{
			if( player == null )
				return;

			if( !shop.IsOpen )
			{
				Debug.WriteLine($"DEBUG: {player.Name} tried to view shop at {shop.ChestX}, {shop.ChestY}");
				player.SendErrorMessage("This shop is closed.");
				return;
			}

			if( shop.IsBeingChanged )
			{
				Debug.WriteLine($"DEBUG: {player.Name} tried to view shop at {shop.ChestX}, {shop.ChestY}");
				player.SendErrorMessage("This shop is being changed right now.");
				return;
			}

			shop.ShowStock(player);
		}

		/// <summary>
		/// Shows the Shop's stock menu.
		/// </summary>
		/// <param name="shop">Shop</param>
		/// <param name="player">Player</param>
		public static void ShowStock(this Shop shop, TSPlayer player)
		{
			if( player == null )
				return;

			player.SendInfoMessage("Current stock:");
			var sb = new StringBuilder();
			for( var i = 0; i < Chest.maxItems; ++i )
			{
				var shopItem = shop.Items.FirstOrDefault(si => si.Index == i);
				if( shopItem?.StackSize > 0 )
				{
					sb.Append(
						$"[{i + 1}:[i/s{shopItem.StackSize},p{shopItem.PrefixId}:{shopItem.ItemId}]] ");
				}
				if( ( i + 1 ) % 10 == 0 && sb.Length > 0 )
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
