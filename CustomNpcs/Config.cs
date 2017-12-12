using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CustomNpcs
{
    /// <summary>
    ///     Represents the configuration instance. This class is a singleton.
    /// </summary>
    [JsonObject]
    internal sealed class Config
    {
        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        [NotNull]
        public static Config Instance { get; internal set; } = new Config();

        /// <summary>
        ///     Gets or sets the max spawns.
        /// </summary>
        public int MaxSpawns { get; set; } = 5;

        /// <summary>
        ///     Gets or sets the spawn rate.
        /// </summary>
        public int SpawnRate { get; set; } = 600;
    }
}
