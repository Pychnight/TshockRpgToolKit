using System.Collections.Generic;
using System.Diagnostics;
using Leveling.Classes;

namespace Leveling.Levels
{
    /// <summary>
    ///     Represents a level.
    /// </summary>
    public sealed class Level
    {
        private readonly LevelDefinition _definition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Level" /> class with the specified class and definition.
        /// </summary>
        /// <param name="class">The class, which must not be <c>null</c>.</param>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        public Level(Class @class, LevelDefinition definition)
        {
            Debug.Assert(definition != null, "Definition must not be null.");

            Class = @class;
            _definition = definition;
        }

        /// <summary>
        ///     Gets the class that contains this level.
        /// </summary>
        public Class Class { get; }

        /// <summary>
        ///     Gets the list of commands to execute on leveling down.
        /// </summary>
        public IList<string> CommandsOnLevelDown => _definition.CommandsOnLevelDown;

        /// <summary>
        ///     Gets the list of commands to execute on leveling up.
        /// </summary>
        public IList<string> CommandsOnLevelUp => _definition.CommandsOnLevelUp;

        /// <summary>
        ///     Gets the list of commands to execute on leveling up, but only once.
        /// </summary>
        public IList<string> CommandsOnLevelUpOnce => _definition.CommandsOnLevelUpOnce;

        /// <summary>
        ///     Gets the display name.
        /// </summary>
        public string DisplayName => _definition.DisplayName;

        /// <summary>
        ///     Gets the EXP required to level up.
        /// </summary>
        public long ExpRequired => _definition.ExpRequired;

        /// <summary>
        ///     Gets the set of item names allowed.
        /// </summary>
        public ISet<string> ItemNamesAllowed => _definition.ItemNamesAllowed;

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name => _definition.Name;

        /// <summary>
        ///     Gets the set of permissions granted.
        /// </summary>
        public ISet<string> PermissionsGranted => _definition.PermissionsGranted;

        /// <summary>
        ///     Gets the prefix.
        /// </summary>
        public string Prefix => _definition.Prefix;

        public override string ToString() => DisplayName;
    }
}
