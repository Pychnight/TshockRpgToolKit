using CustomQuests.Quests;
using System;
using System.Collections.Concurrent;

namespace CustomQuests.Triggers
{
	/// <summary>
	/// Abstract base class for Triggers that have a quota or tally to be met by PartyMembers.
	/// </summary>
	public abstract class TallyTrigger : Trigger
	{
		//optional action to take when a member gains items.
		private Action<PartyMember, int> tallyChangedAction;

		//we use a concurrent queue to avoid running on the thread that listens/handles the ItemDrop
		private ConcurrentQueue<TallyChangedEventArgs> tallyChangesQueue;

		/// <summary>
		/// Gets whether this TallyTrigger has an Action set to run on tally changes.
		/// </summary>
		protected bool HasTallyChangedAction => tallyChangedAction != null;

		/// <summary>
		/// Sets an Action to be run for each recorded tally change, in <see cref="TryProcessTallyChanges" />.
		/// </summary>
		/// <param name="tallyChangedAction"></param>
		protected void SetTallyChangedAction(Action<PartyMember, int> tallyChangedAction)
		{
			if (tallyChangedAction != null)
			{
				this.tallyChangedAction = tallyChangedAction;
				tallyChangesQueue = new ConcurrentQueue<TallyChangedEventArgs>();
			}
		}

		/// <summary>
		/// Attempts to add tally change event information to the internal queue. If HasTallyChangedAction is false, this method will not enqueue anything.
		/// </summary>
		/// <param name="partyMember"></param>
		/// <param name="tallyChange"></param>
		protected void TryEnqueueTallyChange(PartyMember partyMember, int tallyChange)
		{
			if (HasTallyChangedAction)
			{
				var tallyArgs = new TallyChangedEventArgs(partyMember, tallyChange);
				tallyChangesQueue.Enqueue(tallyArgs);
			}
		}

		/// <summary>
		/// Attempts to drain any queued tally event information, running the tally changed action for each item. If HasTallyChangedAction is false, this method
		/// will not process the queue.
		/// </summary>
		protected void TryProcessTallyChanges()
		{
			if (HasTallyChangedAction)
			{
				while (!tallyChangesQueue.IsEmpty)
				{
					if (tallyChangesQueue.TryDequeue(out var args))
						tallyChangedAction(args.PartyMember, args.TallyChange);
				}
			}
		}
	}
}
