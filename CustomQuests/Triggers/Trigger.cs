using System;
using JetBrains.Annotations;
using NLua;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Specifies a trigger that is required for a quest to progress.
    /// </summary>
    // TODO: show progress
    public abstract class Trigger : IDisposable
    {
        /// <summary>
        ///     Gets or sets the callback to run after the trigger is completed for the first time.
        /// </summary>
        [CanBeNull]
        [UsedImplicitly]
        public LuaFunction Callback { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the trigger is completed.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        ///     Disposes the trigger.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Initializes the trigger.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        ///     Updates the trigger.
        /// </summary>
        public void Update()
        {
            if (IsCompleted)
            {
                return;
            }

            if (UpdateImpl())
            {
                Callback?.Call();
                IsCompleted = true;
            }
        }

        /// <summary>
        ///     Disposes the trigger.
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Callback?.Dispose();
                Callback = null;
            }
        }

        /// <summary>
        ///     Updates the trigger.
        /// </summary>
        /// <returns><c>true</c> if the trigger is completed; otherwise, <c>false</c>.</returns>
        protected abstract bool UpdateImpl();
    }
}
