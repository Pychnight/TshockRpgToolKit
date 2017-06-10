using System;
using System.Linq;
using CustomQuests.Triggers;
using JetBrains.Annotations;
using NLua;
using TShockAPI;

namespace CustomQuests
{
    /// <summary>
    ///     Provides functions for quest scripts.
    /// </summary>
    public class QuestFunctions
    {
        private readonly TSPlayer _player;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QuestFunctions" /> class with the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
        public QuestFunctions([NotNull] TSPlayer player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="All" /> class with the specified triggers.
        /// </summary>
        /// <param name="triggers">The triggers, which must not be <c>null</c> or contain <c>null</c>.</param>
        /// <returns>The instance.</returns>
        /// <exception cref="ArgumentException"><paramref name="triggers" /> contains <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="triggers" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [NotNull]
        [UsedImplicitly]
        public static All All(params Trigger[] triggers)
        {
            if (triggers == null)
            {
                throw new ArgumentNullException(nameof(triggers));
            }
            if (triggers.Contains(null))
            {
                throw new ArgumentException("Triggers must not contain null.", nameof(triggers));
            }

            return new All(triggers);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Any" /> class with the specified triggers.
        /// </summary>
        /// <param name="triggers">The triggers, which must not be <c>null</c> or contain <c>null</c>.</param>
        /// <returns>The instance.</returns>
        /// <exception cref="ArgumentException"><paramref name="triggers" /> contains <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="triggers" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [NotNull]
        [UsedImplicitly]
        public static Any Any(params Trigger[] triggers)
        {
            if (triggers == null)
            {
                throw new ArgumentNullException(nameof(triggers));
            }
            if (triggers.Contains(null))
            {
                throw new ArgumentException("Triggers must not contain null.", nameof(triggers));
            }

            return new Any(triggers);
        }

        /// <summary>
        ///     Executes the specified command as the server.
        /// </summary>
        /// <param name="str">The command string, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the command was executed successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="str" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static bool ExecuteCommand([NotNull] string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return Commands.HandleCommand(TSPlayer.Server, str);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Wait" /> class with the specified seconds.
        /// </summary>
        /// <param name="seconds">The seconds, which must be positive.</param>
        /// <returns>The instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="seconds" /> is not positive.</exception>
        [LuaGlobal]
        [NotNull]
        [UsedImplicitly]
        public static Wait Wait(int seconds)
        {
            if (seconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be positive.");
            }

            return new Wait(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DropItems" /> class with the specified item name and amount.
        /// </summary>
        /// <param name="itemName">The item name, which must not be <c>null</c>.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <returns>The instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="itemName" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
        [LuaGlobal]
        [NotNull]
        [UsedImplicitly]
        public DropItems DropItems([NotNull] string itemName, int amount = 1)
        {
            if (itemName == null)
            {
                throw new ArgumentNullException(nameof(itemName));
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
            }

            return new DropItems(_player, itemName, amount);
        }

        /// <summary>
        ///     Gives an item to the specified player.
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <param name="stackSize">The stack size, which must be positive.</param>
        /// <param name="prefix">The prefix.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stackSize" /> is not positive.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public void GiveItem(int itemId, int stackSize = 1, byte prefix = 0)
        {
            if (stackSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stackSize), "Stack size must be positive.");
            }

            var tplayer = _player.TPlayer;
            _player.GiveItem(itemId, "", tplayer.width, tplayer.height, stackSize, prefix);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="InArea" /> class with the specified player and positions.
        /// </summary>
        /// <param name="x">The first X position.</param>
        /// <param name="y">The first Y position.</param>
        /// <param name="x2">The second X position.</param>
        /// <param name="y2">The second Y position.</param>
        /// <returns>The instance.</returns>
        [LuaGlobal]
        [NotNull]
        [UsedImplicitly]
        public InArea InArea(int x, int y, int x2, int y2) => new InArea(_player, x, y, x2, y2);

        /// <summary>
        ///     Creates a new instance of the <see cref="KillNpcs" /> class with the specified NPC name and amount.
        /// </summary>
        /// <param name="npcName">The NPC name, which must not be <c>null</c>.</param>
        /// <param name="amount">The amount, which must be positive.</param>
        /// <returns>The instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="npcName" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
        [LuaGlobal]
        [NotNull]
        [UsedImplicitly]
        public KillNpcs KillNpcs([NotNull] string npcName, int amount = 1)
        {
            if (npcName == null)
            {
                throw new ArgumentNullException(nameof(npcName));
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
            }

            return new KillNpcs(_player, npcName, amount);
        }

        /// <summary>
        ///     Sends the specified message to the player with color.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public void SendMessage([NotNull] string message, byte r, byte g, byte b)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _player.SendMessage(message, r, g, b);
        }

        /// <summary>
        ///     Sends the specified status to the player.
        /// </summary>
        /// <param name="status">The status, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="status" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public void SendStatus([NotNull] string status)
        {
            if (status == null)
            {
                throw new ArgumentNullException(nameof(status));
            }

            const string newLines = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
            _player.SendData(PacketTypes.Status, $"{status}" + newLines);
        }
    }
}
