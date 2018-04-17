using System;
using JetBrains.Annotations;
using TShockAPI;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a condition trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class Condition : Trigger
    {
        private Func<bool> condition;
		
		/// <summary>
		///     Initializes a new instance of the <see cref="Condition" /> class with the specified condition.
		/// </summary>
		/// <param name="condition">The condition, which must not be <c>null</c>.</param>
		public Condition(Func<bool> condition)
		{
			this.condition = condition;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
				//_condition?.Dispose();
				//_condition = null;
				condition = null;
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
				return condition?.Invoke() == true;
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
