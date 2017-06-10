using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using System.Linq;
using JetBrains.Annotations;
using TerrariaApi.Server;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a kill NPCs trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class KillNpcs : Trigger
    {
        private static readonly Dictionary<int, int> LastStrucks = new Dictionary<int, int>();

        private readonly string _npcName;
        private readonly Party _party;

        private int _amount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="KillNpcs" /> class with the specified party, NPC name, and amount.
        /// </summary>
        /// <param name="party">The party, which must not be <c>null</c>.</param>
        /// <param name="npcName">The NPC name, or <c>null</c> for any NPC.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="party" /> or <paramref name="npcName" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
        public KillNpcs([NotNull] Party party, [CanBeNull] string npcName = null, int amount = 1)
        {
            _party = party ?? throw new ArgumentNullException(nameof(party));
            _npcName = npcName;
            _amount = amount > 0
                ? amount
                : throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(CustomQuestsPlugin.Instance, OnNetGetData);
            ServerApi.Hooks.NpcKilled.Register(CustomQuestsPlugin.Instance, OnNpcKilled);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(CustomQuestsPlugin.Instance, OnNetGetData);
                ServerApi.Hooks.NpcKilled.Deregister(CustomQuestsPlugin.Instance, OnNpcKilled);
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override bool UpdateImpl() => _amount == 0;

        private void OnNetGetData(GetDataEventArgs args)
        {
            if (args.Handled || _party.All(p => p.Index != args.Msg.whoAmI))
            {
                return;
            }

            if (args.MsgID == PacketTypes.NpcStrike || args.MsgID == PacketTypes.NpcItemStrike)
            {
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    var npcId = data.ReadInt16();
                    LastStrucks[npcId] = args.Msg.whoAmI;
                }
            }
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            var npc = args.npc;
            if (LastStrucks.TryGetValue(npc.whoAmI, out var lastStruck) && _party.Any(p => p.Index == lastStruck) &&
                (_npcName?.Equals(npc.FullName, StringComparison.OrdinalIgnoreCase) ?? true))
            {
                LastStrucks.Remove(npc.whoAmI);
                --_amount;
            }
        }
    }
}
