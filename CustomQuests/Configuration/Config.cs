using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Corruption.PluginSupport;
using Newtonsoft.Json;

namespace CustomQuests.Configuration
{
    /// <summary>
    ///     Represents the configuration.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config : JsonConfig
    {
		[JsonProperty(Order = 0)]
		public DatabaseConfig Database { get; private set; } = new DatabaseConfig("sqlite", $"uri=file://quests/db.sqlite,Version=3");
		
		[JsonProperty("DefaultQuests",Order = 1)]
        private readonly List<string> _defaultQuestNames = new List<string>();

        /// <summary>
        ///     Gets a read-only view of the default quest names.
        /// </summary>
        public ReadOnlyCollection<string> DefaultQuestNames => _defaultQuestNames.AsReadOnly();

        /// <summary>
        ///     Gets the save period.
        /// </summary>
        [JsonProperty]
        public TimeSpan SavePeriod { get; private set; }

		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			if (Database == null)
				result.Errors.Add(new ValidationError($"Database config is null."));

			if(_defaultQuestNames.Count<1)
				result.Warnings.Add(new ValidationWarning("There are no DefaultQuestNames configured."));
						
			return result;
		}
	}
}
