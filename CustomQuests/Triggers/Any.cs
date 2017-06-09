using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a trigger that completes if any of its subtriggers complete.
    /// </summary>
    [UsedImplicitly]
    public sealed class Any : Trigger
    {
        private readonly List<Trigger> _triggers;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="Any" /> class with the specified triggers.
        /// </summary>
        /// <param name="triggers">The triggers, which must not be <c>null</c> or contain <c>null</c>.</param>
        /// <exception cref="ArgumentException"><paramref name="triggers" /> contains <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="triggers" /> is <c>null</c>.</exception>
        public Any([NotNull] [ItemNotNull] IEnumerable<Trigger> triggers)
        {
            if (triggers == null)
            {
                throw new ArgumentNullException(nameof(triggers));
            }

            var triggerList = triggers.ToList();
            if (triggerList.Contains(null))
            {
                throw new ArgumentException("Triggers cannot contain null.", nameof(triggers));
            }

            _triggers = triggerList;
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            foreach (var trigger in _triggers)
            {
                trigger.Initialize();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var trigger in _triggers)
                {
                    trigger.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override bool UpdateImpl()
        {
            foreach (var trigger in _triggers)
            {
                trigger.Update();
            }
            return _triggers.Any(t => t.IsCompleted);
        }
    }
}
