using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using TShockAPI;

namespace CustomQuests
{
    /// <summary>
    ///     Represents a party of players.
    /// </summary>
    public sealed class Party : IEnumerable<TSPlayer>
    {
        private readonly List<TSPlayer> _players = new List<TSPlayer>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Party" /> class with the specified leader.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <param name="leader">The leader, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">
        ///     Either <paramref name="name" /> or <paramref name="leader" /> is <c>null</c>.
        /// </exception>
        public Party([NotNull] string name, [NotNull] TSPlayer leader)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Leader = leader ?? throw new ArgumentNullException(nameof(leader));
            _players.Add(Leader);
        }

        /// <summary>
        ///     Gets the party count.
        /// </summary>
        public int Count => _players.Count;

        /// <summary>
        ///     Gets the leader.
        /// </summary>
        [NotNull]
        public TSPlayer Leader { get; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        [NotNull]
        public string Name { get; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Gets an enumerator iterating through the players.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<TSPlayer> GetEnumerator() => _players.GetEnumerator();

        /// <summary>
        ///     Adds the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
        public void Add([NotNull] TSPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            _players.Add(player);
        }

        /// <summary>
        ///     Removes the specified player.
        /// </summary>
        /// <param name="player">The player, which must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the player was removed; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <c>null</c>.</exception>
        public void Remove([NotNull] TSPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            _players.Remove(player);
        }

        /// <summary>
        ///     Sends data to the party.
        /// </summary>
        /// <param name="packetType">The packet type.</param>
        /// <param name="text">The text.</param>
        /// <param name="number">The first number.</param>
        /// <param name="number2">The second number.</param>
        /// <param name="number3">The third number.</param>
        /// <param name="number4">The fourth number.</param>
        /// <param name="number5">The fifth number.</param>
        public void SendData(PacketTypes packetType, string text = "", int number = 0, float number2 = 0,
            float number3 = 0, float number4 = 0, int number5 = 0)
        {
            foreach (var player in _players)
            {
                player.SendData(packetType, text, number, number2, number3, number4, number5);
            }
        }

        /// <summary>
        ///     Sends an error message to the party.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void SendErrorMessage([NotNull] string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (var player in _players)
            {
                player.SendErrorMessage(message);
            }
        }

        /// <summary>
        ///     Sends an informational message to the party.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void SendInfoMessage([NotNull] string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (var player in _players)
            {
                player.SendInfoMessage(message);
            }
        }

        /// <summary>
        ///     Sends a message to the party.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void SendMessage([NotNull] string message, Color color)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (var player in _players)
            {
                player.SendMessage(message, color);
            }
        }

        /// <summary>
        ///     Sends a message to the party.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void SendMessage([NotNull] string message, byte r, byte g, byte b)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (var player in _players)
            {
                player.SendMessage(message, r, g, b);
            }
        }

        /// <summary>
        ///     Sends a success message to the party.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void SendSuccessMessage([NotNull] string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (var player in _players)
            {
                player.SendSuccessMessage(message);
            }
        }
    }
}
