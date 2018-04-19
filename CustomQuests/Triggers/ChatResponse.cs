using System;
using System.Linq;
using CustomQuests.Quests;
using JetBrains.Annotations;
using TerrariaApi.Server;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a chat response trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class ChatResponse : Trigger
    {
        private readonly string _message;
        private readonly bool _onlyLeader;
        private readonly Party party;

        private bool _responded;

		/// <summary>
		///     Initializes a new instance of the <see cref="ChatResponse" /> class with the specified party and message.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		/// <param name="onlyLeader"><c>true</c> if only the leader can respond; otherwise, <c>false</c>.</param>
		public ChatResponse( Party party, string message, bool onlyLeader)
		{
			this.party = party ?? throw new ArgumentNullException(nameof(party));
			_message = message ?? throw new ArgumentNullException(nameof(message));
			_onlyLeader = onlyLeader;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ChatResponse" /> class with the specified party and message.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="message">The message, which must not be <c>null</c>.</param>
		public ChatResponse(Party party, string message) : this(party,message,false)
		{
		}

		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(CustomQuestsPlugin.Instance, OnChat);
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(CustomQuestsPlugin.Instance, OnChat, int.MaxValue);
        }

        /// <inheritdoc />
        protected override bool UpdateImpl() => _responded;

        private void OnChat(ServerChatEventArgs args)
        {
            if( ( _onlyLeader ? args.Who == party.Leader.Player.Index : party.Any(p => p.Player.Index == args.Who) ) &&
			   args.Text.Equals(_message, StringComparison.OrdinalIgnoreCase) )
			{
				_responded = true;
				args.Handled = true;
			}

		}
    }
}
