using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Corruption
{
	public class InventorySlot
	{
		int playerIndex;
		int baseIndex;
		Item[] items;
		int index;

		public int Id
		{
			get { return items[index].netID; }
			set
			{
				items[index].netID = value;
				NetUpdate();
			}
		}

		public int Stack
		{
			get { return items[index].stack; }
			set
			{
				items[index].stack = value;
				NetUpdate();
			}
		}

		public int Prefix
		{
			get { return items[index].prefix; }
			set
			{
				items[index].prefix = (byte)value;
				NetUpdate();
			}
		}

		internal InventorySlot(int playerIndex, int baseIndex, Item[] items, int index)
		{
			this.playerIndex = playerIndex;
			this.baseIndex = baseIndex;
			this.items = items;
			this.index = index;
		}

		public void Set(int itemId, int stack, int prefix)
		{
			items[index].netID = itemId;
			items[index].stack = stack;
			items[index].prefix = (byte)prefix;

			NetUpdate();
		}

		public void Set(string itemType, int stack, int prefix)
		{
			var id = ItemFunctions.GetItemIdFromName(itemType);

			if( id == null )
				return;

			Set((int)id, stack, prefix);
		}

		public void Clear()
		{
			items[index].stack = 0;

			NetUpdate();
		}

		private void NetUpdate()
		{
			var item = items[index];
			var slot = baseIndex + index;

			//?? tshock example shows plr.TPlayer.inventory[0].name .. name doesn't exist on Item 
			NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, playerIndex, slot, item.prefix);
			NetMessage.SendData((int)PacketTypes.PlayerSlot, playerIndex, -1, null, playerIndex, slot, item.prefix);
		}

		public override string ToString()
		{
			if( Stack > 0 )
			{
				var itemName = ItemFunctions.GetItemNameFromId(Id) ?? "N/A";

				if( Prefix > 0 )
					return $"{itemName} x {Stack} ({(ItemPrefix)Prefix})";
				else
					return $"{itemName} x {Stack}";
			}
			else
				return $"Empty Slot";
		}
	}
}
