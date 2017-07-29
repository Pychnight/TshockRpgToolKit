using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leveling.Sessions
{
    /// <summary>
    ///     Represents a session definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SessionDefinition
    {
        /// <summary>
        ///     Gets a mapping of unlocked class names to EXP amounts.
        /// </summary>
        [JsonProperty("ClassToExp")]
        public IDictionary<string, long> ClassNameToExp { get; } = new Dictionary<string, long>();

        /// <summary>
        ///     Gets a mapping of unlocked class names to level names.
        /// </summary>
        [JsonProperty("ClassToLevel")]
        public IDictionary<string, string> ClassNameToLevelName { get; } = new Dictionary<string, string>();

        /// <summary>
        ///     Gets or sets the current class name.
        /// </summary>
        [JsonProperty("CurrentClass")]
        public string CurrentClassName { get; set; }

        /// <summary>
        ///     Gets the item IDs given.
        /// </summary>
        [JsonProperty("ItemIdsGiven")]
        public ISet<int> ItemIdsGiven { get; internal set; } = new HashSet<int>();

        /// <summary>
        ///     Gets the level names obtained.
        /// </summary>
        [JsonProperty("LevelsObtained")]
        public ISet<string> LevelNamesObtained { get; internal set; } = new HashSet<string>();

        /// <summary>
        ///     Gets the set of completed class names.
        /// </summary>
        [JsonProperty("MasteredClasses")]
        public ISet<string> MasteredClassNames { get; private set; } = new HashSet<string>();

        /// <summary>
        ///     Gets the set of unlocked class names.
        /// </summary>
        [JsonProperty("UnlockedClasses")]
        public ISet<string> UnlockedClassNames { get; private set; } = new HashSet<string>();
    }
}
