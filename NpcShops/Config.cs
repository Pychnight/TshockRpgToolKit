using Corruption.PluginSupport;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace NpcShops
{
    /// <summary>
    ///     Represents a configuration. This class is a singleton.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config : JsonConfig
    {
        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        public static Config Instance { get; internal set; }

		/// <summary>
		///		Gets the maximum distance in tiles, in which a player may talk to a Shopkeeper NPC.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int ShopNpcMaxTalkRange { get; internal set; } = 32;

		/// <summary>
		///		Gets the duration a Shopkeeper NPC will stand still, in millseconds. -1 = indefinite.
		/// </summary>
		[JsonProperty(Order = 2)]
		public int ShopNpcPauseDuration { get; internal set; } = 8000;

		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			if (ShopNpcMaxTalkRange<1)
			{
				result.Warnings.Add(new ValidationWarning($"{nameof(ShopNpcMaxTalkRange)} is less than 1. This may make it impossible to talk to the shop NPC."));
			}

			return result;
		}
	}
}
