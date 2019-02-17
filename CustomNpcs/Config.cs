using Corruption.PluginSupport;
using Newtonsoft.Json;

namespace CustomNpcs
{
    /// <summary>
    ///     Represents the configuration instance. This class is a singleton.
    /// </summary>
    [JsonObject]
    internal sealed class Config : JsonConfig
    {
        /// <summary>
        ///     Gets the configuration instance.
        /// </summary>
        public static Config Instance { get; internal set; } = new Config();

        /// <summary>
        ///     Gets or sets the max spawns.
        /// </summary>
        public int MaxSpawns { get; set; } = 5;

        /// <summary>
        ///     Gets or sets the spawn rate.
        /// </summary>
        public int SpawnRate { get; set; } = 600;

		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			if(MaxSpawns<1)
				result.Warnings.Add(new ValidationWarning($"{nameof(MaxSpawns)} is less than 1. Custom NPCs will not spawn naturally."));
						
			return result;
		}
	}
}
