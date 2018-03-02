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

		/// <summary>
		///		Gets the maximum distance in tiles, in which a player may talk to a Shopkeeper NPC.
		/// </summary>
		[JsonProperty(Order = 0)]
		public int ShopNpcMaxTalkRange { get; internal set; } = 32;

		/// <summary>
		///		Gets the duration a Shopkeeper NPC will stand still, in millseconds. -1 = indefinite.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int ShopNpcPauseDuration { get; internal set; } = 8000;
    }
}
