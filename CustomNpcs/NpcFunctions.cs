using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using NLua;
using TShockAPI;

namespace CustomNpcs
{
    /// <summary>
    ///     Provides functions for NPC scripts.
    /// </summary>
    public static class NpcFunctions
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
    }
}
