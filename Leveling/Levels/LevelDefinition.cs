using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leveling.Levels
{
    /// <summary>
    ///     Represents a level definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class LevelDefinition
    {
        /// <summary>
        ///     Gets the list of commands to execute on leveling up.
        /// </summary>
        [JsonProperty("CommandsOnLevelUp", Order = 6)]
        public IList<string> CommandsOnLevelUp { get; internal set; } = new List<string>();

        /// <summary>
        ///     Gets the list of commands to execute on leveling up, but only once.
        /// </summary>
        [JsonProperty("CommandsOnLevelUpOnce", Order = 7)]
        public IList<string> CommandsOnLevelUpOnce { get; internal set; } = new List<string>();

		/// <summary>
		///     Gets the list of commands to execute on leveling down.
		/// </summary>
		[JsonProperty("CommandsOnLevelDown", Order = 8)]
		public IList<string> CommandsOnLevelDown { get; internal set; } = new List<string>();
			
        /// <summary>
        ///     Gets the display name.
        /// </summary>
        [JsonProperty("DisplayName", Order = 1)]
        public string DisplayName { get; internal set; }

        /// <summary>
        ///     Gets the EXP required to level up.
        /// </summary>
        [JsonProperty("ExpRequired", Order = 2)]
        public long ExpRequired { get; internal set; }

        /// <summary>
        ///     Gets the set of item names allowed.
        /// </summary>
        [JsonProperty("ItemsAllowed", Order = 4)]
        public ISet<string> ItemNamesAllowed { get; internal set; } = new HashSet<string>();

        /// <summary>
        ///     Gets the name.
        /// </summary>
        [JsonProperty("Name", Order = 0)]
        public string Name { get; internal set; }

        /// <summary>
        ///     Gets the set of permissions granted.
        /// </summary>
        [JsonProperty("PermissionsGranted", Order = 5)]
        public ISet<string> PermissionsGranted { get; internal set; } = new HashSet<string>();

        /// <summary>
        ///     Gets the prefix for the level.
        /// </summary>
        [JsonProperty("Prefix", Order = 3)]
        public string Prefix { get; internal set; } = "";
    }
}
