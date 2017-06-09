using System;
using JetBrains.Annotations;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a drop item trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class DropItems : Trigger
    {
        private readonly string _itemName;
        private readonly TSPlayer _player;

        private int _amount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DropItems" /> class with the specified player, item name, and amount.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <param name="itemName">The item name, which must not be <c>null</c>.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="player" /> or <paramref name="itemName" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
        public DropItems([NotNull] TSPlayer player, [NotNull] string itemName, int amount = 1)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _itemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
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
            if (args.Handled || args.Player.Index != _player.Index || args.ID != Main.maxItems)
            {
                return;
            }

            var itemIdName = EnglishLanguage.GetItemNameById(args.Type);
            if (itemIdName.Equals(_itemName, StringComparison.OrdinalIgnoreCase))
            {
                _amount -= args.Stacks;
                if (_amount < 0)
                {
                    _player.GiveItem(args.Type, "", 20, 42, -_amount, args.Prefix);
                    _amount = 0;
                }
                args.Handled = true;
            }
        }
    }
}
