using CustomQuests.Triggers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomQuests.Quests
{
	/// <summary>
	///     Represents a quest instance.
	/// </summary>
	public partial class Quest : IDisposable
	{
		internal protected Task MainQuestTask { get; internal set; } // deliberately long winded, to not clash with likely script names
		private CancellationTokenSource CancellationTokenSource;
		protected CancellationToken QuestCancellationToken => CancellationTokenSource.Token;
		private ConcurrentDictionary<int, Trigger> triggers;
		int nextTriggerId = 1;

		//used to warn when containing task exited, and Complete() was not called( leads to hanging triggers ).
		internal bool CalledComplete { get; private set; }

		/// <summary>
		///     Gets the quest info.
		/// </summary>
		public QuestInfo QuestInfo { get; internal set; }

		/// <summary>
		///     Gets a value indicating whether the quest is ended.
		/// </summary>
		public bool IsEnded { get; private set; }

		/// <summary>
		///     Gets a value indicating whether the quest is successful.
		/// </summary>
		public bool IsSuccessful { get; private set; }

		/// <summary>
		///  Gets or sets a friendly string informing players of their progress within a quest.
		/// </summary>
		public string QuestStatus { get; set; }
		public Color QuestStatusColor { get; set; } // = Color.White;

		internal HashSet<string> RejoinablePlayers { get; private set; }

		public Quest()
		{
			CancellationTokenSource = new CancellationTokenSource();
			triggers = new ConcurrentDictionary<int, Trigger>();
			QuestStatusColor = Color.White;

			RejoinablePlayers = new HashSet<string>();
		}

		/// <summary>
		///     Disposes the quest.
		/// </summary>
		public void Dispose()
		{
			foreach (var ct in triggers.Values)
			{
				ct.Dispose();
			}
		}

		internal void TryAddRejoinablePlayer(TSPlayer player)
		{
			if (!QuestInfo.AllowRejoin)
				return;

			RejoinablePlayers.Add(player.Name);
		}

		internal bool CanRejoin(TSPlayer player)
		{
			if (!QuestInfo.AllowRejoin)
				return false;

			if (RejoinablePlayers.Contains(player.Name))
				return true;

			return false;
		}


		internal void Run() => MainQuestTask = Task.Run(() => OnRun());

		//this method gets overridden in boo, by transplanting the modules main method into it.
		protected virtual void OnRun()
		{
		}

		public void Abort() => OnAbort();

		/// <summary>
		/// Aborts a Quest, and optionally sends each party member an error message.
		/// </summary>
		/// <param name="partyAbortMessage">Optional message to send party members on abort, null ignores.</param>
		protected internal virtual void OnAbort(string partyAbortMessage = null)
		{
			Debug.Print($"OnAbort()! for {QuestInfo.Name}");
			Debug.Print("Cancelling...");
			CancellationTokenSource.Cancel();

			foreach (var member in party)
			{
				if (member != null)
				{
					if (partyAbortMessage != null)
						member.SendErrorMessage(partyAbortMessage);

					var session = CustomQuestsPlugin.Instance.GetSession(member);
					session.IsAborting = true;
					session.HasAborted = true;
				}
			}
		}

		/// <summary>
		///     Completes the quest.
		/// </summary>
		/// <param name="isSuccess"><c>true</c> to complete successfully; otherwise, <c>false</c>.</param>
		protected void Complete(bool isSuccess)
		{
			if (IsEnded)
				return;

			CalledComplete = true;
			IsEnded = true;
			IsSuccessful = isSuccess;

			CancellationTokenSource.Cancel();
		}

		/// <summary>
		///     Updates the quest's triggers and party state.
		/// </summary>
		internal void Update()
		{
			if (IsEnded)
				return;

			checkParty();
			updateTriggers();
		}

		private void checkParty()
		{
			if (party == null || party.Count < 1)
			{
				Debug.Print("Party is null or empty, aborting quest.");
				Abort();
			}
		}

		private void updateTriggers()
		{
			var completedTriggers = new List<Trigger>();

			foreach (var trigger in triggers.Values)
			{
				trigger.Update();

				if (trigger.Status != TriggerStatus.Running)
					completedTriggers.Add(trigger);
			}

			foreach (var ct in completedTriggers)
			{
				triggers.TryRemove(ct.Id, out var removedTrigger);
				ct.Dispose();
			}
		}
	}
}
