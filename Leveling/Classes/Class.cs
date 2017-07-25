using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Leveling.Levels;

namespace Leveling.Classes
{
    /// <summary>
    ///     Represents a class.
    /// </summary>
    public sealed class Class
    {
        private readonly ClassDefinition _definition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Class" /> class with the specified definition.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        public Class(ClassDefinition definition)
        {
            Debug.Assert(definition != null, "Definition must not be null.");

            _definition = definition;
            Levels = _definition.LevelDefinitions.Select(ld => new Level(this, ld)).ToList();
        }

        /// <summary>
        ///     Gets the display name.
        /// </summary>
        public string DisplayName => _definition.DisplayName;

        /// <summary>
        ///     Gets the set of item names allowed.
        /// </summary>
        public ISet<string> ItemNamesAllowed { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets the levels.
        /// </summary>
        public IList<Level> Levels { get; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name => _definition.Name;

        /// <summary>
        ///     Gets the next classes.
        /// </summary>
        public IReadOnlyList<Class> NextClasses { get; private set; } = new List<Class>();

        /// <summary>
        ///     Gets the set of permissions granted.
        /// </summary>
        public ISet<string> PermissionsGranted { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets the prerequisite classes.
        /// </summary>
        public IReadOnlyList<Class> PrerequisiteClasses { get; private set; } = new List<Class>();

        /// <summary>
        ///     Gets the SEconomy cost to enter this class.
        /// </summary>
        public long SEconomyCost => _definition.SEconomyCost;

        /// <summary>
        ///     Resolves the class using the specified classes.
        /// </summary>
        /// <param name="classes">The classes, which must not be <c>null</c> or contain <c>null</c>.</param>
        public void Resolve(IList<Class> classes)
        {
            Debug.Assert(classes != null, "Classes must not be null.");
            Debug.Assert(!classes.Contains(null), "Classes must not contain null.");

            NextClasses = classes.Where(c => c._definition.PrerequisiteClassNames.Contains(Name)).ToList();
            PrerequisiteClasses = _definition.PrerequisiteClassNames.Select(
                pcn => classes.First(c => c.Name == pcn)).ToList();

            var queue = new Queue<Class>();
            foreach (var @class in PrerequisiteClasses)
            {
                queue.Enqueue(@class);
            }
            while (queue.Count > 0)
            {
                var @class = queue.Dequeue();
                foreach (var level in @class.Levels)
                {
                    ItemNamesAllowed.UnionWith(level.ItemNamesAllowed);
                    PermissionsGranted.UnionWith(level.PermissionsGranted);
                }

                foreach (var class2 in @class.PrerequisiteClasses)
                {
                    queue.Enqueue(class2);
                }
            }
        }

        public override string ToString() => DisplayName;
    }
}
