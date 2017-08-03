using System.Diagnostics;
using Wolfje.Plugins.SEconomy;

namespace NpcShops.Shops
{
    /// <summary>
    ///     Represents a shop command.
    /// </summary>
    public sealed class ShopCommand
    {
        private readonly ShopCommandDefinition _definition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShopCommand" /> class with the specified definition.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        public ShopCommand(ShopCommandDefinition definition)
        {
            Debug.Assert(definition != null, "Definition must not be null.");

            _definition = definition;
            StackSize = definition.StackSize;
        }

        /// <summary>
        ///     Gets the command.
        /// </summary>
        public string Command => _definition.Command;

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name => _definition.Name;

        /// <summary>
        ///     Gets the permission required.
        /// </summary>
        public string PermissionRequired => _definition.PermissionRequired;

        /// <summary>
        ///     Gets or sets the stack size. A value of -1 indicates unlimited.
        /// </summary>
        public int StackSize { get; set; }

        /// <summary>
        ///     Gets the unit price.
        /// </summary>
        public Money UnitPrice => _definition.UnitPrice;

        /// <summary>
        ///     Restocks the shop command.
        /// </summary>
        public void Restock()
        {
            StackSize = _definition.StackSize;
        }
    }
}
