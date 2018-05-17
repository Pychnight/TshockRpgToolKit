using System;
using System.Collections.Generic;
using System.Linq;
using CustomQuests.Quests;
using TerrariaApi.Server;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a chat response trigger.
    /// </summary>
    public sealed class ChatResponse : Trigger
    {
        private readonly string responseString;
		private IEnumerable<PartyMember> members;
		private bool responded;

		/// <summary>
		///     Initializes a new instance of the <see cref="ChatResponse" /> class with the specified party and message.
		/// </summary>
		/// <param name="partyMembers">The party, which must not be <c>null</c>.</param>
		/// <param name="responseString">The response string to check for.</param>
		public ChatResponse( IEnumerable<PartyMember> partyMembers, string responseString)
		{
			members = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));
			this.responseString = responseString ?? throw new ArgumentNullException(nameof(responseString));
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="ChatResponse"/> class with the specified PartyMember and key response.
		/// </summary>
		/// <param name="partyMember">The PartyMember.</param>
		/// <param name="responseString">The response string to check for.</param>
		public ChatResponse( PartyMember partyMember, string responseString)
			: this(partyMember.ToEnumerable(), responseString )
		{
		}
		
		protected override void Dispose(bool disposing)
        {
            if (disposing)
				ServerApi.Hooks.ServerChat.Deregister(CustomQuestsPlugin.Instance, OnChat);
            
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(CustomQuestsPlugin.Instance, OnChat, int.MaxValue);
        }

        /// <inheritdoc />
        protected internal override TriggerStatus UpdateImpl() => responded.ToTriggerStatus();

        private void OnChat(ServerChatEventArgs args)
        {
            if( members.Any(p => p.Index == args.Who) && args.Text.Equals(responseString, StringComparison.OrdinalIgnoreCase) )
			{
				responded = true;
				args.Handled = true;
			}
		}
    }
}
