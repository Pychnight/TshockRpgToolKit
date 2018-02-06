using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using NpcShops.Shops;
using Terraria;
using TShockAPI;

namespace NpcShops
{
    /// <summary>
    ///     Holds session data.
    /// </summary>
    public sealed class Session
    {
		public const int InvalidNpcIndex = -1;
		public const int MaxShopkeeperTileRange = 32;

        private readonly TSPlayer player;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Session" /> class with the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        public Session(TSPlayer player)
        {
            Debug.Assert(player != null, "Player must not be null.");

            this.player = player;
        }

        /// <summary>
        ///     Gets or sets the current shop.
        /// </summary>
        public NpcShop CurrentShop { get; set; }

		/// <summary>
		///		Gets or sets the index of the npc running the shop, if any.
		/// </summary>
		public int CurrentShopkeeperNpcIndex { get; set; } = InvalidNpcIndex;

		public bool HasShopkeeper => CurrentShopkeeperNpcIndex != InvalidNpcIndex;

		/// <summary>
		///		Attemps to get the shopkeeper NPC.
		/// </summary>
		/// <returns>The shopkeeper NPC, if one exists.</returns>
		public NPC GetShopkeeper()
		{
			if( CurrentShopkeeperNpcIndex != InvalidNpcIndex )
			{
				var npc = Main.npc[CurrentShopkeeperNpcIndex];

				return npc?.active == true ? npc : null;
			}

			return null;
		}

		/// <summary>
		///		Returns if the distance between the player and the shopkeeper npc are within the allowable range.
		/// </summary>
		/// <returns>True if the shopkeeper is in range.</returns>
		public bool IsShopkeeperInRange()
		{
			var npc = GetShopkeeper();

			if( npc!=null)
			{
				var dist = Vector2.DistanceSquared(player.TPlayer.Center, npc.Center);
				return dist <= ( MaxShopkeeperTileRange * 16 ) * ( MaxShopkeeperTileRange * 16 );
			}

			return false;
		}
				
		//public bool InShop()
		//{
		//	var result = false;

		//	if(CurrentShop!=null)
		//	{
		//		if(HasShopkeeper)
		//			result = IsShopkeeperInRange();
		//		else
		//			result = CurrentShop?.Rectangle.Contains(player.TileX, player.TileY) == true;
		//	}

		//	return result;
		//}

		public void Update()
		{
			if(CurrentShop==null)
			{
				NpcShop shop = null;

				//shopkeeper will be assigned by OnNetGetaData when talking to an npc thats mapped to a shop.
				if(HasShopkeeper)
				{
					var shopKeeper = GetShopkeeper();
					NpcShop.NpcToShopMap.TryGetValue(shopKeeper.type, out shop);
				}
				else
					shop = NpcShopsPlugin.Instance.npcShops.FirstOrDefault(ns => ns.Rectangle.Contains(player.TileX, player.TileY));

				if(shop!=null)
					EnterShop(shop);
			}
			else
			{
				if(!CurrentShop.IsOpen)
				{
					LeaveShop();
				}
				else 
				{
					if( HasShopkeeper )
					{
						if( !IsShopkeeperInRange() )
							LeaveShop();
					}
					else if( !CurrentShop.Rectangle.Contains(player.TileX, player.TileY) )
					{
						LeaveShop();
					}
				}
			}
		}

		public void EnterShop(NpcShop shop) //, int shopkeeperNpcIndex = InvalidNpcIndex)
		{
			if(shop!=null)
			{
				Debug.Print("Player entered shop.");
				if( shop.IsOpen )
				{
					if( shop.Message != null )
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

			CurrentShop = shop;
			//CurrentShopkeeperNpcIndex = shopkeeperNpcIndex;
		}
		
		private void LeaveShop()
		{
			Debug.Print("Player left shop.");
			CurrentShop = null;
			CurrentShopkeeperNpcIndex = InvalidNpcIndex;
		}
    }
}
