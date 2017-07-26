using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leveling
{
    /// <summary>
    ///     Represents the configuration. This class is a singleton.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config
    {
        /// <summary>
        ///     Gets the mapping of NPC names to EXP rewards.
        /// </summary>
        [JsonProperty("NpcToExpReward", Order = 1)]
        public IDictionary<string, long> NpcNameToExpReward = new Dictionary<string, long>();

        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        public static Config Instance { get; internal set; } = new Config();

        /// <summary>
        ///     Gets the default class name.
        /// </summary>
        [JsonProperty("DefaultClass", Order = 0)]
        public string DefaultClassName { get; private set; } = "ranger";

        /// <summary>
        ///     Gets the global EXP multiplier.
        /// </summary>
        [JsonProperty("ExpMultiplier", Order = 2)]
        public double ExpMultiplier { get; private set; } = 1.0;
    }
}
