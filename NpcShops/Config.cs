using Newtonsoft.Json;

namespace NpcShops
{
    /// <summary>
    ///     Represents a configuration. This class is a singleton.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config
    {
        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        public static Config Instance { get; internal set; } = new Config();

		[JsonProperty(Order = 0)]
		public int MaxNpcTileRange { get; set; } = 32;
    }
}
