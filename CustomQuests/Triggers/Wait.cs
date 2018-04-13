using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a wait.
    /// </summary>
    [UsedImplicitly]
    public sealed class Wait : Trigger
    {
        private readonly TimeSpan _delay;

        private DateTime _startTime;

		/// <summary>
		///     Initializes a new instance of the <see cref="Wait" /> class with the specified delay.
		/// </summary>
		/// <param name="milliseconds">The delay, in milliseconds.</param>
		public Wait(int milliseconds) : this(TimeSpan.FromMilliseconds(milliseconds))
		{
		}

        /// <summary>
        ///     Initializes a new instance of the <see cref="Wait" /> class with the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        public Wait(TimeSpan delay)
        {
            _delay = delay;
		}

        /// <inheritdoc />
        protected override void Initialize()
        {
            _startTime = DateTime.UtcNow;
        }

        /// <inheritdoc />
        protected override bool UpdateImpl() => DateTime.UtcNow - _startTime > _delay;
    }
}
