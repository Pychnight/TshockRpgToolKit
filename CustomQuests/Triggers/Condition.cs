using System;
using JetBrains.Annotations;
using NLua;
using TShockAPI;

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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _condition?.Dispose();
                _condition = null;
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override bool UpdateImpl()
        {
            try
            {
                return (bool)_condition.Call()[0];
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleInfo("An exception occurred in Condition: ");
                TShock.Log.ConsoleInfo(ex.ToString());
                return true;
            }
        }
    }
}
