using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using JetBrains.Annotations;
using TerrariaApi.Server;
using TShockAPI;

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
        private readonly TSPlayer _player;

        private int _amount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="KillNpcs" /> class with the specified player, NPC name, and amount.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <param name="npcName">The NPC name, which must not be <c>null</c>.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="player" /> or <paramref name="npcName" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
        public KillNpcs([NotNull] TSPlayer player, [NotNull] string npcName, int amount = 1)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _npcName = npcName ?? throw new ArgumentNullException(nameof(npcName));
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
            if (args.Handled || args.Msg.whoAmI != _player.Index)
            {
                return;
            }

            if (args.MsgID == PacketTypes.NpcStrike || args.MsgID == PacketTypes.NpcItemStrike)
            {
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    var npcId = data.ReadInt16();
                    LastStrucks[npcId] = _player.Index;
                }
            }
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            var npc = args.npc;
            if (LastStrucks.TryGetValue(npc.whoAmI, out var lastStruck) && lastStruck == _player.Index &&
                npc.FullName.Equals(_npcName, StringComparison.OrdinalIgnoreCase))
            {
                LastStrucks.Remove(npc.whoAmI);
                --_amount;
            }
        }
    }
}
