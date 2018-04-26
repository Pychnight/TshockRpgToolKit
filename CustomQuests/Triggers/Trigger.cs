using System;
using System.Threading;
using System.Threading.Tasks;
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
		/// Signals that the Trigger's UpdateImpl() has been set to true, allowing linked Tasks to stop blocking.  
		/// </summary>
		internal ManualResetEventSlim Signal { get; private set; } = new ManualResetEventSlim();
		
		public bool IsDisposed { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the trigger is completed.
        /// </summary>
        public bool IsCompleted { get; private set; }
		
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
				Signal.Set();
                IsCompleted = true;
            }
        }

		public void Dispose()
		{
			Dispose(true);
		}

        /// <summary>
        ///     Disposes the trigger.
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
			if( IsDisposed )
				return;

            if(disposing)
            {
			}

			//if( !Signal.IsSet )
			//	Signal.Set();

			Signal.Dispose();

			IsDisposed = true;
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
