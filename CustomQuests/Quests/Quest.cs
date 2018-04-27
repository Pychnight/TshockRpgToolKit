using System;
using System.Collections.Generic;
using System.Linq;
using CustomQuests.Triggers;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CustomQuests.Quests
{
    /// <summary>
    ///     Represents a quest instance.
    /// </summary>
    public class Quest : IDisposable
    {
		public Quest()
		{
		}

        /// <summary>
        ///     Initializes a new instance of the <see cref="Quest" /> class with the specified quest info.
        /// </summary>
        /// <param name="questInfo">The quest info, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="questInfo" /> is <c>null</c>.</exception>
        public Quest([NotNull] QuestInfo questInfo)
        {
            QuestInfo = questInfo ?? throw new ArgumentNullException(nameof(questInfo));

			QuestStatusColor = Color.White;
        }

        /// <summary>
        ///     Gets a value indicating whether the quest is ended.
        /// </summary>
        public bool IsEnded { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the quest is successful.
        /// </summary>
        public bool IsSuccessful { get; private set; }

        /// <summary>
        ///     Gets the quest info.
        /// </summary>
        [NotNull]
        public QuestInfo QuestInfo { get; internal set; }

		/// <summary>
		///  Gets or sets a friendly string informing players of their progress within a quest.
		/// </summary>
		public string QuestStatus { get; set; }

		public Color QuestStatusColor { get; set; } // = Color.White;

        /// <summary>
        ///     Disposes the quest.
        /// </summary>
        public virtual void Dispose()
        {
        }
		
        /// <summary>
        ///     Completes the quest.
        /// </summary>
        /// <param name="isSuccess"><c>true</c> to complete successfully; otherwise, <c>false</c>.</param>
        [UsedImplicitly]
        public virtual void Complete(bool isSuccess)
        {
			if( IsEnded )
				return;

            IsEnded = true;
            IsSuccessful = isSuccess;
        }
		
        /// <summary>
        ///     Updates the quest.
        /// </summary>
        internal virtual void Update()
        {
            if (IsEnded)
            {
                return;
            }
        }
    }
}
