using System.Collections.Generic;
using Leveling.Levels;
using Newtonsoft.Json;

namespace Leveling.Classes
{
    /// <summary>
    ///     Represents a class definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ClassDefinition
    {
        /// <summary>
        ///     Gets the display name.
        /// </summary>
        [JsonProperty("DisplayName", Order = 1)]
        public string DisplayName { get; private set; }

        /// <summary>
        ///     Gets the list of level definitions.
        /// </summary>
        [JsonProperty("Levels", Order = 2)]
        public IList<LevelDefinition> LevelDefinitions { get; private set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        [JsonProperty("Name", Order = 0)]
        public string Name { get; private set; }

        /// <summary>
        ///     Gets the list of prerequisite class names.
        /// </summary>
        [JsonProperty("PrerequisiteClasses", Order = 3)]
        public IList<string> PrerequisiteClassNames { get; private set; }

        /// <summary>
        ///     Gets the SEconomy cost to enter this class.
        /// </summary>
        [JsonProperty(Order = 4)]
        public long SEconomyCost { get; private set; }
    }
}
