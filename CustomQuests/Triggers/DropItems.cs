using System;
using System.Linq;
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
        private readonly Party _party;

        private int _amount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DropItems" /> class with the specified party, item name, and amount.
        /// </summary>
        /// <param name="party">The party, which must not be <c>null</c>.</param>
        /// <param name="itemName">The item name, or <c>null</c> for any item.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="party" /> or <paramref name="itemName" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
        public DropItems([NotNull] Party party, [CanBeNull] string itemName = null, int amount = 1)
        {
            _party = party ?? throw new ArgumentNullException(nameof(party));
            _itemName = itemName;
            _amount = amount > 0
                ? amount
                : throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            GetDataHandlers.ItemDrop += OnItemDrop;
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
        protected override bool UpdateImpl() => _amount == 0;

        private void OnItemDrop(object sender, GetDataHandlers.ItemDropEventArgs args)
        {
            var player = args.Player;
            if (args.Handled || args.ID != Main.maxItems || _party.All(p => p.Index != player.Index))
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
