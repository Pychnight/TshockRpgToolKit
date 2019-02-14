using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace Corruption
{
	public class PlayerInventoryManager
	{
		public Player Player { get; private set; }
		public int PlayerIndex { get; private set; }
		public InventoryGroup Inventory { get; private set; }
		public InventoryGroup Armor { get; private set; }
		public InventoryGroup Dye { get; private set; }
		public InventoryGroup MiscEquips { get; private set; }
		public InventoryGroup MiscDyes { get; private set; }
		
		public PlayerInventoryManager(int playerIndex, Player player)
		{
			Player =		player;
			PlayerIndex =	playerIndex;
			Inventory =		new InventoryGroup(playerIndex,player.inventory, 0);
			Armor =			new InventoryGroup(playerIndex,player.armor, NetItem.InventorySlots);
			Dye =			new InventoryGroup(playerIndex,player.dye, NetItem.InventorySlots + NetItem.ArmorSlots);
			MiscEquips =	new InventoryGroup(playerIndex,player.miscEquips, NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
			MiscDyes =		new InventoryGroup(playerIndex,player.miscDyes, NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
		}

		public PlayerInventoryManager(TSPlayer player)
			: this(player.Index, player.TPlayer)
		{
		}
	}
}
