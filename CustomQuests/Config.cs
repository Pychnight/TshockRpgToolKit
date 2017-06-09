using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace CustomQuests
{
    /// <summary>
    ///     Represents the configuration.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config
    {
        [JsonProperty("DefaultQuests")]
        private readonly List<string> _defaultQuestNames = new List<string>();

        /// <summary>
        ///     Gets a read-only view of the default quest names.
        /// </summary>
        public ReadOnlyCollection<string> DefaultQuestNames => _defaultQuestNames.AsReadOnly();
    }
}
