using Newtonsoft.Json;

namespace CustomNpcs.Npcs
{
    /// <summary>
    ///     Represents a loot entry definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class LootEntryDefinition
    {
        /// <summary>
        ///     Gets or sets the chance.
        /// </summary>
        [JsonProperty(Order = 4)]
        public double Chance { get; set; }

        /// <summary>
        ///     Gets or sets the maximum stack size.
        /// </summary>
        [JsonProperty(Order = 2)]
        public int MaxStackSize { get; set; }

        /// <summary>
        ///     Gets or sets the minimum stack size.
        /// </summary>
        [JsonProperty(Order = 1)]
        public int MinStackSize { get; set; }

        /// <summary>
        ///     Gets or sets the prefix.
        /// </summary>
        [JsonProperty(Order = 3)]
        public byte Prefix { get; set; }

        /// <summary>
        ///     Gets or sets the type.
        /// </summary>
        [JsonProperty(Order = 0)]
        public int Type { get; set; }
    }
}
