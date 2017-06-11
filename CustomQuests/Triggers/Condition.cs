using System;
using JetBrains.Annotations;
using NLua;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a condition trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class Condition : Trigger
    {
        private LuaFunction _condition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Condition" /> class with the specified condition.
        /// </summary>
        /// <param name="condition">The condition, which must not be <c>null</c>.</param>
        public Condition([NotNull] LuaFunction condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <inheritdoc />
        public override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _condition.Dispose();
                _condition = null;
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override bool UpdateImpl() => (bool)_condition.Call()[0];
    }
}
