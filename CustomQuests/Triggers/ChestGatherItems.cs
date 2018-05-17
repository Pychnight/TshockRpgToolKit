using Corruption;
using CustomQuests.Quests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace CustomQuests.Triggers
{
	public class ChestGatherItems : Trigger
	{
		ConcurrentQueue<ChestItemChangedEventArgs> chestItemChanges;//when we first get the event, the chest count wont be updated yet, so we just add the eventargs to this queue,
																	//to be looked at during the triggers update.

		IEnumerable<PartyMember> partyMembers;
		int chestX, chestY;
		int chestId;
		string itemType;
		int itemId;
		byte itemPrefix;
		int itemsRequired;
		int itemsGathered;
		//bool perMember;//is amount absolute, or per each member?

		int chestItemCount;//amount carried by chest.
		
		bool success;

		public ChestGatherItems(IEnumerable<PartyMember> partyMembers, int chestX, int chestY, int amount, int itemId, byte itemPrefix) // bool perMember)
		{
			chestItemChanges = new ConcurrentQueue<ChestItemChangedEventArgs>();

			this.partyMembers = partyMembers;
			this.chestX = chestX;
			this.chestY = chestY;
			this.itemId = itemId;
			this.itemPrefix = itemPrefix;
			this.itemsRequired = amount;
			//this.perMember = perMember;
		}

		public ChestGatherItems(IEnumerable<PartyMember> partyMembers, int chestX, int chestY, int amount, int itemId)
			: this(partyMembers, chestX, chestY, amount, itemId, 0)
		{
		}
		
		public ChestGatherItems(IEnumerable<PartyMember> partyMembers, int chestX, int chestY, int amount, string itemType, byte itemPrefix) // bool perMember)
		{
			chestItemChanges = new ConcurrentQueue<ChestItemChangedEventArgs>();

			this.partyMembers = partyMembers;
			this.chestX = chestX;
			this.chestY = chestY;
			this.itemType = itemType;
			this.itemPrefix = itemPrefix;
			this.itemsRequired = amount;
			//this.perMember = perMember;
		}

		public ChestGatherItems(IEnumerable<PartyMember> partyMembers, int chestX, int chestY, int amount, string itemType)
			: this(partyMembers, chestX, chestY, amount, itemType, 0)
		{
		}

		public ChestGatherItems(PartyMember partyMember, int chestX, int chestY, int amount, string itemType, byte itemPrefix)
			: this(partyMember.ToEnumerable(),chestX,chestY,amount,itemType,itemPrefix)
		{
		}

		public ChestGatherItems(PartyMember partyMember, int chestX, int chestY, int amount, string itemType)
			: this(partyMember,chestX,chestY,amount,itemType,0)
		{
		}

		public ChestGatherItems(PartyMember partyMember, int chestX, int chestY, int amount, int itemId, byte itemPrefix)
			: this(partyMember.ToEnumerable(), chestX, chestY, amount, itemId, itemPrefix)
		{
		}

		public ChestGatherItems(PartyMember partyMember, int chestX, int chestY, int amount, int itemId)
			: this(partyMember, chestX, chestY, amount, itemId, 0)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if( disposing )
				CustomQuestsPlugin.Instance.ChestItemChanged -= chestItemChanged;

			base.Dispose(disposing);
		}

		/// <inheritdoc />
		protected override void Initialize()
		{
			//ensure chest exists...
			chestId = Chest.FindChest(chestX, chestY - 1);

			if( chestId == -1 )
			{
				success = true; // just pass for now on invalid chests.
				//return;
			}

			//try to ensure valid item if we were passed an id string, and not an integer id.
			if( !string.IsNullOrWhiteSpace(itemType) )
			{
				var id = ItemFunctions.GetItemIdFromName(itemType);

				if( id != null )
					itemId = (int)id;
				else
					success = true;
			}
			
			//ensure the item actually exists in the chest, with enough quantity.
			chestItemCount = ItemFunctions.CountChestItem(chestX, chestY, itemId, itemPrefix);

			if( chestItemCount < itemsRequired )
			{
				//Debug.Print("There are not enough items in the chest. Skipping.");
				success = true;
			}
			
			CustomQuestsPlugin.Instance.ChestItemChanged += chestItemChanged;
		}

		private void chestItemChanged(object sender, ChestItemChangedEventArgs args)
		{
			if(args.ChestId == chestId)
			{
				Debug.Print("ChestGatherItems Trigger detected chest modifications.");
				if( partyMembers.Any( m => m.Index == args.PlayerIndex))
				{
					chestItemChanges.Enqueue(args);
				}
			}
		}

		protected internal override TriggerStatus UpdateImpl()
		{
			while(!success && chestItemChanges.Count>0)
			{
				if( chestItemChanges.TryDequeue(out var args) )
				{
					var newCount = ItemFunctions.CountChestItem(chestX, chestY, itemId, itemPrefix);

					if( newCount < chestItemCount )
					{
						var diff = chestItemCount - newCount;
						itemsGathered += diff;

						chestItemCount = newCount;
					}

					if( itemsGathered >= itemsRequired )
					{
						success = true;
					}
				}
			}
			
			return success.ToTriggerStatus();
		}
	}
}
