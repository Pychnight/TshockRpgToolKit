using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Corruption;
using CustomQuests.Quests;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a gather items trigger.
    /// </summary>
    public sealed class GatherItems : Trigger
    {
        private readonly HashSet<int> _blacklistedIndexes = new HashSet<int>();
        private readonly string _itemName;
		private IEnumerable<PartyMember> partyMembers;
		private int itemsRequired;
		private Action<PartyMember,int> tallyChangedAction;						//optional action to take when a member gains items.
		private ConcurrentQueue<PartyTallyChangedEventArgs> tallyChangesQueue;	//we use a concurrent queue to avoid running on the thread that listens/handles the ItemDrop

		/// <summary>
		///     Initializes a new instance of the <see cref="GatherItems" /> class with the specified party, item name, and amount.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="tallyChangedAction">An action to run each time a PartyMember gathers items.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMembers" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public GatherItems( IEnumerable<PartyMember> partyMembers, Action<PartyMember, int> tallyChangedAction, string itemName, int amount )
		{
			this.partyMembers = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));

			_itemName = itemName;
			itemsRequired = amount > 0
				? amount
				: throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
			
			if(tallyChangedAction!=null)
			{
				this.tallyChangedAction = tallyChangedAction;
				tallyChangesQueue = new ConcurrentQueue<PartyTallyChangedEventArgs>();
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="GatherItems" /> class with the specified party, item name, and amount.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMembers" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public GatherItems( IEnumerable<PartyMember> partyMembers, string itemName, int amount ) : this(partyMembers, null, itemName, amount)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="GatherItems" /> class with the specified party and item name.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMembers" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		public GatherItems( IEnumerable<PartyMember> partyMembers, string itemName) : this(partyMembers,itemName,1)
		{
		}

		public GatherItems(IEnumerable<PartyMember> partyMembers, int itemType, int amount) : this(partyMembers, ItemFunctions.GetItemNameFromId(itemType), amount)
		{
		}

		public GatherItems(IEnumerable<PartyMember> partyMembers, int itemType) : this(partyMembers, itemType, 1)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="GatherItems" /> class with the specified party, item name, and amount.
		/// </summary>
		/// <param name="partyMember">The party member, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMember" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public GatherItems(PartyMember partyMember, string itemName, int amount)
			: this(partyMember.ToEnumerable(), itemName, amount)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="GatherItems" /> class with the specified party and item name.
		/// </summary>
		/// <param name="partyMember">The party member, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMember" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		public GatherItems(PartyMember partyMember, string itemName) : this(partyMember, itemName, 1)
		{
		}

		public GatherItems(PartyMember partyMember, int itemType, int amount) : this(partyMember, ItemFunctions.GetItemNameFromId(itemType), amount)
		{
		}

		public GatherItems(PartyMember partyMember, int itemType ) : this(partyMember, itemType, 1 )
		{
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GetDataHandlers.ItemDrop -= OnItemDrop;
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            GetDataHandlers.ItemDrop += OnItemDrop;
        }

		/// <inheritdoc />
		protected internal override TriggerStatus UpdateImpl()
		{
			//process any queued tally event info
			if(tallyChangedAction!=null)
			{
				while(!tallyChangesQueue.IsEmpty)
				{
					if(tallyChangesQueue.TryDequeue(out var args))
						tallyChangedAction(args.PartyMember, args.TallyChange);
				}
			}
			
			return (itemsRequired <= 0).ToTriggerStatus();
		}

		private void OnItemDrop(object sender, GetDataHandlers.ItemDropEventArgs args)
		{
			if(args.Handled)
				return;

			if(args.ID == Main.maxItems)
				ItemDropped(args);
			else if(partyMembers.Any(p => p.IsValidMember && p.Player.Index == args.Player.Index))
				ItemGathered(args);
		}

		private void ItemGathered(GetDataHandlers.ItemDropEventArgs args )
		{
			//Debug.Print("Gathered Item!");

			//only continue if an entire Item(stack) is being removed
			if (args.Stacks != 0)
				return;

			var index = args.ID;
			var item = Main.item[index];
			if(_itemName?.Equals(item.Name, StringComparison.OrdinalIgnoreCase) ?? true)
			{
				if (_blacklistedIndexes.Contains(index))
					_blacklistedIndexes.Remove(index);
				else
				{
					itemsRequired = Math.Max(0, itemsRequired - item.stack);

					//add player tally event info?
					if(tallyChangedAction!=null)
					{
						var member = partyMembers.FirstOrDefault(pm => pm.Player == args.Player);
						if( member!=null)
						{
							var tallyArgs = new PartyTallyChangedEventArgs(member, item.stack);
							tallyChangesQueue.Enqueue(tallyArgs); 
						}
					}
				}
			}
		}

		private void ItemDropped(GetDataHandlers.ItemDropEventArgs args)
		{
			//Debug.Print("Dropped Item!");
			
			var itemIdName = EnglishLanguage.GetItemNameById(args.Type);
			if (_itemName?.Equals(itemIdName, StringComparison.OrdinalIgnoreCase) ?? true)
			{
				var index = Main.maxItems;
				for (var i = 0; i < Main.maxItems; ++i)
				{
					if (!Main.item[i].active && Main.itemLockoutTime[i] == 0)
					{
						index = i;
						break;
					}
				}
				if (index == Main.maxItems)
				{
					var minTimeDiff = 0;
					for (var i = 0; i < Main.maxItems; ++i)
					{
						var timeDiff = Main.item[i].spawnTime - Main.itemLockoutTime[i];
						if (timeDiff > minTimeDiff)
						{
							minTimeDiff = timeDiff;
							index = i;
						}
					}
				}
				_blacklistedIndexes.Add(index);
			}
		}
    }
}
