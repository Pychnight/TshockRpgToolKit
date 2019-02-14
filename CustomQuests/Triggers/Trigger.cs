using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.Localization;

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
		/// Signals that the Trigger's UpdateImpl() has been set to Success or Fail, allowing linked Tasks to stop blocking.  
		/// </summary>
		internal ManualResetEventSlim Signal { get; private set; } = new ManualResetEventSlim();
		
		public bool IsDisposed { get; private set; }

		//Id to track triggers within a Quest.
		internal int Id { get; set; }

		/// <summary>
		///     Gets a value indicating the state of the trigger.
		/// </summary>
		public TriggerStatus Status { get; private set; }
				
		/// <summary>
		/// Temporary holding place for a linked Task.
		/// </summary>
		internal Task<TriggerStatus> Task { get; set; }
		
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

            if(Status != TriggerStatus.Running)
            {
                return;
            }

			Status = UpdateImpl();
			
			if(Status!=TriggerStatus.Running)
            {
				Signal.Set();
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
        protected internal abstract TriggerStatus UpdateImpl();

		//this should probably be moved into Corruption...
		internal static HashSet<string> BuildNpcNameHashSet(IEnumerable<object> typesOrNames)
		{
			//we now ignore invalid inputs, and just keep valid ones.
			var validInputs = typesOrNames.Where(o => o is string || o is int);
			var types = validInputs.Select(o => GetNPCName(o));
			var validTypes = types.Where(s => s != null);

			return new HashSet<string>(validTypes);
		}

		//this should probably be moved into Corruption...
		internal static string GetNPCName(object value)
		{
			if( value is string )
			{
				return value as string;
			}
			else if( value is int )
			{
				var id = (int)value;
				var name = EnglishLanguage.GetNpcNameById(id);//think id should be renamed 'type'
				return name;
			}

			return null;
		}
	}
}
