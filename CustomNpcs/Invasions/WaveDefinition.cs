using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CustomNpcs.Invasions
{
    /// <summary>
    ///     Represents a wave definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class WaveDefinition
    {
        /// <summary>
        ///     Gets the maximum spawns.
        /// </summary>
        [JsonProperty(Order = 2)]
        public int MaxSpawns { get; private set; } = 10;

        /// <summary>
        ///     Gets the miniboss.
        /// </summary>
        [CanBeNull]
        [JsonProperty(Order = 5)]
        public object Miniboss { get; private set; }

        /// <summary>
        ///     Gets the NPC weights.
        /// </summary>
        [JsonProperty(Order = 0)]
        [NotNull]
        public Dictionary<string, int> NpcWeights { get; private set; } = new Dictionary<string, int>();

        /// <summary>
        ///     Gets the points required to advance.
        /// </summary>
        [JsonProperty(Order = 1)]
        public int PointsRequired { get; private set; }

        /// <summary>
        ///     Gets the spawn rate.
        /// </summary>
        [JsonProperty(Order = 3)]
        public int SpawnRate { get; private set; } = 20;

        /// <summary>
        ///     Gets the start message.
        /// </summary>
        [JsonProperty(Order = 4)]
        [NotNull]
        public string StartMessage { get; private set; } = "The wave has started!";
    }
}
