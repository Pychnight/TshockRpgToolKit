using Newtonsoft.Json;

namespace CustomNpcs
{
    /// <summary>
    ///     Represents the configuration instance. This class is a singleton.
    /// </summary>
    [JsonObject]
    public sealed class Config
    {
        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        public static Config Instance { get; internal set; } = new Config();

        /// <summary>
        ///     Gets or sets the max spawns.
        /// </summary>
        public int MaxSpawns { get; set; } = 10;

        /// <summary>
        ///     Gets or sets the spawn rate.
        /// </summary>
        public int SpawnRate { get; set; } = 600;
    }
}
