using System;
using JetBrains.Annotations;
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
