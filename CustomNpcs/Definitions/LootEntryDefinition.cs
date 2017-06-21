using Newtonsoft.Json;

namespace CustomNpcs.Definitions
{
    /// <summary>
    ///     Represents a loot entry definition.
    /// </summary>
    [JsonObject]
    public sealed class LootEntryDefinition
    {
        /// <summary>
        ///     Gets or sets the chance.
        /// </summary>
        public double Chance { get; set; }

        /// <summary>
        ///     Gets or sets the maximum stack size.
        /// </summary>
        public int MaxStackSize { get; set; }

        /// <summary>
        ///     Gets or sets the minimum stack size.
        /// </summary>
        public int MinStackSize { get; set; }

        /// <summary>
        ///     Gets or sets the prefix.
        /// </summary>
        public byte Prefix { get; set; }

        /// <summary>
        ///     Gets or sets the type.
        /// </summary>
        public int Type { get; set; }
    }
}
