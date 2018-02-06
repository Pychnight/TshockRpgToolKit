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
		
		bool? lastShopState = null;
		internal bool shopKeeperClickedHack = false;
				
		private void sendOpenMessage(NpcShop shop)
		{
			if( shop.Message != null )
				player.SendInfoMessage(shop.Message);
		}

		internal void SendClosedMessage(NpcShop shop)
		{
			var msg = !string.IsNullOrWhiteSpace(shop.ClosedMessage) ? shop.ClosedMessage :
																		$"This shop is closed. Come back at {shop.OpeningTime}.";
			player.SendErrorMessage(msg);
		}

		private NpcShop getCurrentShop()
		{
			NpcShop newShop = null;

			if( HasShopkeeper && IsShopkeeperInRange() )
			{
				//npc shop
				var shopKeeper = GetShopkeeper();
				NpcShop.NpcToShopMap.TryGetValue(shopKeeper.type, out newShop);
			}

			if(newShop==null)
			{ 
				//region shop
				newShop = NpcShopsPlugin.Instance.npcShops.FirstOrDefault(ns => ns.Rectangle.Contains(player.TileX, player.TileY));
			}

			return newShop;
		}

		public void Update()
		{
			NpcShop newShop = getCurrentShop();
			
			if( newShop != CurrentShop )
			{
				//we've changed shops somehow, someway
				CurrentShop = newShop;
				
				lastShopState = null;// CurrentShop?.IsOpen;

				Debug.Print("Changed shop.");
			}
			else
			{
				//HACK - to ensure shopkeeper tells you hes closed on each click/talk/interaction
				if( HasShopkeeper && shopKeeperClickedHack )
				{
					if( CurrentShop?.IsOpen == false )
					{
						SendClosedMessage(CurrentShop);
					}
					else
					{
						CurrentShop?.ShowTo(player);
					}
					
					shopKeeperClickedHack = false;
					lastShopState =  CurrentShop?.IsOpen;

					return;
				}
				
				if( CurrentShop == null )
				{
					//reset the npc shop if we're out of range
					if( HasShopkeeper )// && CurrentShop == null )
					{
						CurrentShopkeeperNpcIndex = InvalidNpcIndex;
					}

					lastShopState = null;
					return;
				}

				//still in same old shop
				if( CurrentShop.IsOpen != lastShopState )
				{
					if( CurrentShop.IsOpen )
					{
						sendOpenMessage(CurrentShop);
						CurrentShop.ShowTo(player);
					}
					else
					{
						SendClosedMessage(CurrentShop);
					}
					
					lastShopState = CurrentShop.IsOpen;
				}
			}
		}
    }
}
