using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TShockAPI;
using TShockAPI.Localization;

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

        private static int? GetItemIdFromName(string name)
        {
            for (var i = 0; i < Main.maxItemTypes; ++i)
            {
                var itemName = EnglishLanguage.GetItemNameById(i);
                if (itemName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return i;
                }
            }
            return null;
        }

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
        ///     Buffs the party with the specified type.
        /// </summary>
        /// <param name="buffType">The type.</param>
        /// <param name="seconds">The number of seconds, which must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="seconds" /> is not positive.</exception>
        [UsedImplicitly]
        public void Buff(int buffType, int seconds)
        {
            if (seconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be positive.");
            }

            foreach (var player in _players)
            {
                player.SetBuff(buffType, seconds * 60);
            }
        }

        /// <summary>
        ///     Damages the party.
        /// </summary>
        /// <param name="damage">The damage, which must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="damage" /> is not positive.</exception>
        [UsedImplicitly]
        public void Damage(int damage)
        {
            if (damage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be positive.");
            }

            foreach (var player in _players)
            {
                player.DamagePlayer(damage);
            }
        }

        /// <summary>
        ///     Gives the specified item to the party.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <param name="stackSize">The stack size, which must be positive.</param>
        /// <param name="prefix">The prefix, which must be within range.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Either <paramref name="stackSize" /> is not positive or <paramref name="prefix" /> is too large.
        /// </exception>
        [UsedImplicitly]
        public void GiveItem([NotNull] string name, int stackSize = 1, byte prefix = 0)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (stackSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stackSize), "Stack size must be positive.");
            }
            if (prefix > PrefixID.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix must be within range.");
            }

            var itemId = GetItemIdFromName(name);
            if (itemId == null)
            {
                throw new FormatException($"Invalid item name '{name}'.");
            }

            foreach (var player in _players)
            {
                player.GiveItem((int)itemId, "", 20, 42, stackSize, prefix);
            }
        }

        /// <summary>
        ///     Heals the party.
        /// </summary>
        /// <param name="health">The health, which must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="health" /> is not positive.</exception>
        [UsedImplicitly]
        public void Heal(int health)
        {
            if (health <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(health), "Health must be positive.");
            }

            foreach (var player in _players)
            {
                player.Heal(health);
            }
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
        ///     Removes the specified item from the party. This requires SSC to work.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <param name="stackSize">The stack size, which must be positive.</param>
        /// <param name="prefix">The prefix, which must be within range.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Either <paramref name="stackSize" /> is not positive or <paramref name="prefix" /> is too large.
        /// </exception>
        [UsedImplicitly]
        public void RemoveItems([NotNull] string name, int stackSize = 1, byte prefix = 0)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (stackSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stackSize), "Stack size must be positive.");
            }
            if (prefix > PrefixID.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix must be within range.");
            }

            var itemId = GetItemIdFromName(name);
            if (itemId == null)
            {
                throw new FormatException($"Invalid item name '{name}'.");
            }

            foreach (var player in _players)
            {
                var required = stackSize;
                var offset = 0;

                void Check(IReadOnlyList<Item> items)
                {
                    for (var i = 0; i < items.Count && required > 0; ++i)
                    {
                        var item = items[i];
                        if (item.type == itemId && item.prefix == prefix)
                        {
                            var loss = Math.Min(item.stack, required);
                            item.stack -= loss;
                            required -= loss;
                            player.SendData(PacketTypes.PlayerSlot, "", player.Index, offset + i);
                        }
                    }
                    offset += items.Count;
                }

                var tplayer = player.TPlayer;
                Check(tplayer.inventory);
                Check(tplayer.armor);
                Check(tplayer.dye);
                Check(tplayer.miscEquips);
                Check(tplayer.miscDyes);
                Check(tplayer.bank.item);
                Check(tplayer.bank2.item);
                Check(new[] {tplayer.trashItem});
                Check(tplayer.bank3.item);
            }
        }

        /// <summary>
        ///     Revokes the quest with the specified name for the party.
        /// </summary>
        /// <param name="name">The quest name, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void RevokeQuest([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            foreach (var session in _players.Select(CustomQuestsPlugin.Instance.GetSession))
            {
                session.RevokeQuest(name);
            }
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
        ///     Sends a status to the party.
        /// </summary>
        /// <param name="status">The status, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="status" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void SendStatus([NotNull] string status)
        {
            if (status == null)
            {
                throw new ArgumentNullException(nameof(status));
            }

            foreach (var player in _players)
            {
                var text = status + "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" +
                           "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
                player.SendData(PacketTypes.Status, text);
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

        /// <summary>
        ///     Sends the tile square to the party.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="radius">The radius, which must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="radius" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void SendTileSquare(int x, int y, int radius = 1)
        {
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
            }

            foreach (var player in _players)
            {
                player.SendTileSquare(x, y, radius);
            }
        }

        /// <summary>
        ///     Sends a warning message to the party.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void SendWarningMessage([NotNull] string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (var player in _players)
            {
                player.SendWarningMessage(message);
            }
        }

		/// <summary>
		///     Sets a message that party members may retrieve to see their progress.
		/// </summary>
		/// <param name="questStatus">The status, which must not be <c>null</c>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="questStatus" /> is <c>null</c>.</exception>
		[UsedImplicitly]
		public void SetQuestStatus([NotNull] string questStatus)
		{
			if (questStatus == null)
			{
				throw new ArgumentNullException(nameof(questStatus));
			}
			
			foreach(var player in _players)
			{
				var session = CustomQuestsPlugin.Instance.GetSession(player);

				if(session!=null && session.CurrentQuest!=null)
				{
					session.CurrentQuest.QuestStatus = questStatus;
				}
			}
		}

		/// <summary>
		///     Teleports the party to the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		[UsedImplicitly]
        public void Teleport(int x, int y)
        {
            foreach (var player in _players)
            {
                player.Teleport(16 * x, 16 * y);
            }
        }

        /// <summary>
        ///     Unlocks the quest with the specified name for the party.
        /// </summary>
        /// <param name="name">The quest name, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [UsedImplicitly]
        public void UnlockQuest([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            foreach (var session in _players.Select(CustomQuestsPlugin.Instance.GetSession))
            {
                session.UnlockQuest(name);
            }
        }
    }
}
