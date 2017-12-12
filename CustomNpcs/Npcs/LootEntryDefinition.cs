using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Terraria.ID;

namespace CustomNpcs.Npcs
{
    /// <summary>
    ///     Represents a loot entry definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class LootEntryDefinition
    {
        /// <summary>
        ///     Gets the chance.
        /// </summary>
        [JsonProperty(Order = 4)]
        public double Chance { get; private set; }

        /// <summary>
        ///     Gets the maximum stack size.
        /// </summary>
        [JsonProperty(Order = 2)]
        public int MaxStackSize { get; private set; }

        /// <summary>
        ///     Gets the minimum stack size.
        /// </summary>
        [JsonProperty(Order = 1)]
        public int MinStackSize { get; private set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        [JsonProperty(Order = 0)]
        [NotNull]
        public string Name { get; private set; } = "Dirt Block";

        /// <summary>
        ///     Gets the prefix.
        /// </summary>
        [JsonProperty(Order = 3)]
        public int Prefix { get; private set; }

        internal void ThrowIfInvalid()
        {
            if (Name == null)
            {
                throw new FormatException($"{nameof(Name)} is null.");
            }
            if (MinStackSize < 0)
            {
                throw new FormatException($"{nameof(MinStackSize)} is negative.");
            }
            if (MaxStackSize < MinStackSize)
            {
                throw new FormatException($"{nameof(MaxStackSize)} is less than {nameof(MinStackSize)}.");
            }
            if (Chance <= 0)
            {
                throw new FormatException($"{nameof(Chance)} is not positive.");
            }
            if (Chance > 1)
            {
                throw new FormatException($"{nameof(Chance)} is greater than 1.");
            }
            if (Prefix <= -2)
            {
                throw new FormatException($"{nameof(Prefix)} is too small.");
            }
            if (Prefix >= PrefixID.Count)
            {
                throw new FormatException($"{nameof(Prefix)} is too large.");
            }
        }
    }
}
