using Newtonsoft.Json;

namespace CustomNpcs
{
    /// <summary>
    ///     Represents the configuration.
    /// </summary>
    [JsonObject]
    public sealed class Config
    {
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
