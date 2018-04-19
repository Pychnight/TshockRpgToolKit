using System;
using System.Collections.Generic;
using System.Linq;
using CustomQuests.Quests;
using JetBrains.Annotations;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a gather items trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class GatherItems : Trigger
    {
        private readonly HashSet<int> _blacklistedIndexes = new HashSet<int>();
        private readonly string _itemName;
		private readonly Party party;
		private int _amount;
		
		/// <summary>
		///     Initializes a new instance of the <see cref="GatherItems" /> class with the specified party, item name, and amount.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="party" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public GatherItems( Party party, string itemName, int amount)
		{
			this.party = party ?? throw new ArgumentNullException(nameof(party));
			_itemName = itemName;
			_amount = amount > 0
				? amount
				: throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="GatherItems" /> class with the specified party and item name.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="party" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public GatherItems( Party party, string itemName) : this(party,itemName,1)
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
        protected override bool UpdateImpl() => _amount <= 0;

        private void OnItemDrop(object sender, GetDataHandlers.ItemDropEventArgs args)
        {
            var player = args.Player;
            if (args.Handled)
            {
                return;
            }

            if (args.ID == Main.maxItems)
            {
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
            else if (party.Any(p => p.Player.Index == player.Index))
            {
                var index = args.ID;
                var item = Main.item[index];
                if (_itemName?.Equals(item.Name, StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    if (_blacklistedIndexes.Contains(index))
                    {
                        _blacklistedIndexes.Remove(index);
                    }
                    else
                    {
                        var difference = item.stack - args.Stacks;
                        _amount = Math.Max(0, _amount - difference);
                    }
                }
            }
        }
    }
}
