using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Corruption
{
	public class InventoryGroup
	{
		InventorySlot[] slots;

		public int Count => slots.Length;
		public InventorySlot this[int index] => slots[index];

		public int BaseIndex { get; private set; }

		internal InventoryGroup(int playerIndex, Item[] items, int baseIndex)
		{
			BaseIndex = baseIndex;

			slots = new InventorySlot[items.Length];

			for( var i = 0; i < items.Length; i++ )
				slots[i] = new InventorySlot(playerIndex, baseIndex, items, i);
		}

		public void Clear()
		{
			foreach( var slot in slots )
				slot.Clear();
		}

		public bool Contains(int itemId, int prefix)
		{
			var result = slots.FirstOrDefault(s => s.Stack > 0 &&
													s.Id == itemId &&
													s.Prefix == prefix);

			return result != null;
		}

		public bool Contains(string itemType, int prefix)
		{
			var id = ItemFunctions.GetItemIdFromName(itemType);

			return id != null ? Contains((int)id, prefix) : false;
		}

		public int IndexOf(int itemId, int prefix)
		{
			for( var i = 0; i < slots.Length; i++ )
			{
				var s = slots[i];

				if( s.Stack > 0 &&
					s.Id == itemId &&
					s.Prefix == prefix )
				{
					return i;
				}
			}

			return -1;
		}

		public int IndexOf(string itemType, int prefix)
		{
			var id = ItemFunctions.GetItemIdFromName(itemType);

			if( id == null )
				return -1;

			return IndexOf((int)id, prefix);
		}

	}
}
