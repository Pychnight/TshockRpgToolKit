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
        ///     Gets the global death penalty minimum.
        /// </summary>
        [JsonProperty("DeathPenaltyMinimum", Order = 4)]
        public long DeathPenaltyMinimum { get; private set; }

        /// <summary>
        ///     Gets or sets the global death penalty multiplier.
        /// </summary>
        [JsonProperty("DeathPenaltyMultiplier", Order = 3)]
        public double DeathPenaltyMultiplier { get; set; } = 0.33;

        /// <summary>
        ///     Gets or sets the global death penalty multiplier from PvP.
        /// </summary>
        [JsonProperty("DeathPenaltyPvPMultiplier", Order = 5)]
        public double DeathPenaltyPvPMultiplier { get; set; } = 0.10;

        /// <summary>
        ///     Gets the default class name.
        /// </summary>
        [JsonProperty("DefaultClass", Order = 0)]
        public string DefaultClassName { get; private set; } = "ranger";

        /// <summary>
        ///     Gets or sets the global EXP multiplier.
        /// </summary>
        [JsonProperty("ExpMultiplier", Order = 2)]
        public double ExpMultiplier { get; set; } = 1.0;
    }
}
