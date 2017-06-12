using System;
using System.Linq;
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
        private readonly Party _party;

        private bool _responded;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChatResponse" /> class with the specified party and message.
        /// </summary>
        /// <param name="party">The party, which must not be <c>null</c>.</param>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="onlyLeader"><c>true</c> if only the leader can respond; otherwise, <c>false</c>.</param>
        public ChatResponse([NotNull] Party party, [NotNull] string message, bool onlyLeader = false)
        {
            _party = party ?? throw new ArgumentNullException(nameof(party));
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _onlyLeader = onlyLeader;
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(CustomQuestsPlugin.Instance, OnChat, int.MaxValue);
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
        protected override bool UpdateImpl() => _responded;

        private void OnChat(ServerChatEventArgs args)
        {
            if ((_onlyLeader ? args.Who == _party.Leader.Index : _party.Any(p => p.Index == args.Who)) &&
                args.Text.Equals(_message, StringComparison.OrdinalIgnoreCase))
            {
                _responded = true;
                args.Handled = true;
            }
        }
    }
}
