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
    ///     Represents a drop items trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class DropItems : Trigger
    {
        private readonly string _itemName;
		private List<PartyMember> partyMembers;
        private int _amount;

		/// <summary>
		///     Initializes a new instance of the <see cref="DropItems" /> class with the specified party, item name, and amount.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMembers" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public DropItems(IEnumerable<PartyMember> partyMembers, string itemName, int amount)
		{
			this.partyMembers = new List<PartyMember>(partyMembers) ?? throw new ArgumentNullException(nameof(partyMembers));
			_itemName = itemName;
			_amount = amount > 0
				? amount
				: throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DropItems" /> class with the specified party and item name.
		/// </summary>
		/// <param name="partyMembers">The party members, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMembers" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public DropItems(IEnumerable<PartyMember> partyMembers, string itemName) : this(partyMembers,itemName,1)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DropItems" /> class with the specified party, item name, and amount.
		/// </summary>
		/// <param name="partyMember">The party member, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMember" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public DropItems(PartyMember partyMember, string itemName, int amount) : this(partyMember.ToEnumerable(),itemName,amount)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DropItems" /> class with the specified party and item name.
		/// </summary>
		/// <param name="partyMember">The party member, which must not be <c>null</c>.</param>
		/// <param name="itemName">The item name, or <c>null</c> for any item.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="partyMember" /> or <paramref name="itemName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public DropItems(PartyMember partyMember, string itemName) : this(partyMember, itemName, 1)
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
            if (args.Handled || args.ID != Main.maxItems || partyMembers.All(p => p.Player.Index != player.Index))
            {
                return;
            }

            var itemIdName = EnglishLanguage.GetItemNameById(args.Type);
            if (_itemName?.Equals(itemIdName, StringComparison.OrdinalIgnoreCase) ?? true)
            {
                _amount -= args.Stacks;
                if (_amount < 0)
                {
                    player.GiveItem(args.Type, "", 20, 42, -_amount, args.Prefix);
                    _amount = 0;
                }
                args.Handled = true;
            }
        }
    }
}
