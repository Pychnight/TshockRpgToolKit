using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TShockAPI;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Specifies a trigger that is required for a quest to progress.
    /// </summary>
    // TODO: show progress
    public abstract class Trigger : IDisposable
    {
        private bool _isInitialized;
		        
		/// <summary>
		///     Gets or sets the Action to run after the trigger is completed for the first time.
		/// </summary>
		public Action Action { get; set; }

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
		/// Temporary - id to track triggers within a BooQuest.
		/// </summary>
		internal int Id { get; set; }

		/// <summary>
		/// Temporary holding place for a linked Task.
		/// </summary>
		internal Task Task { get; set; }
		
        /// <summary>
        ///     Updates the trigger.
        /// </summary>
        public void Update()
        {
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            if (IsCompleted)
            {
                return;
            }

            if (UpdateImpl())
            {
                try
                {
					Action?.Invoke();
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError("An exception occurred on Callback:");
                    TShock.Log.ConsoleError(ex.ToString());
                }
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
				//Callback?.Dispose();
				//Callback = null;
				Action = null;
            }
        }

        /// <summary>
        ///     Initializes the trigger.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        ///     Updates the trigger.
        /// </summary>
        /// <returns><c>true</c> if the trigger is completed; otherwise, <c>false</c>.</returns>
        protected internal abstract bool UpdateImpl();
    }
}
