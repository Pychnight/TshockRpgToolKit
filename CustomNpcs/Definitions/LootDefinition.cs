using Newtonsoft.Json;

namespace CustomNpcs.Definitions
{
    /// <summary>
    ///     Represents a loot definition.
    /// </summary>
    [JsonObject]
    public sealed class LootDefinition
    {
        /// <summary>
        ///     Gets the chance.
        /// </summary>
        public double Chance { get; private set; } = 1.0;

        /// <summary>
        ///     Gets the maximum stack size.
        /// </summary>
        public int MaxStackSize { get; private set; } = 10;

        /// <summary>
        ///     Gets the minimum stack size.
        /// </summary>
        public int MinStackSize { get; private set; } = 1;

        /// <summary>
        ///     Gets the prefix.
        /// </summary>
        public byte Prefix { get; private set; }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        public int Type { get; private set; } = 2;
    }
}
