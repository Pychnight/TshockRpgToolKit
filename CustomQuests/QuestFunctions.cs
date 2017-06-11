using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua;
using TShockAPI;

namespace CustomQuests
{
    /// <summary>
    ///     Provides functions for quest scripts.
    /// </summary>
    public static class QuestFunctions
    {
        /// <summary>
        ///     Broadcasts the specified message.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static void Broadcast([NotNull] string message, Color color)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            TShock.Utils.Broadcast(message, color);
        }

        /// <summary>
        ///     Broadcasts the specified message.
        /// </summary>
        /// <param name="message">The message, which must not be <c>null</c>.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <c>null</c>.</exception>
        [LuaGlobal]
        [UsedImplicitly]
        public static void Broadcast([NotNull] string message, byte r, byte g, byte b)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            TShock.Utils.Broadcast(message, r, g, b);
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
    }
}
