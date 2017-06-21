using System;
using JetBrains.Annotations;
using NLua.Exceptions;
using TShockAPI;

namespace CustomNpcs
{
    /// <summary>
    ///     Provides utility functions.
    /// </summary>
    public static class Utils
    {
        private static readonly object LuaLock = new object();

        /// <summary>
        ///     Tries to execute the Lua code contained within the specified action.
        /// </summary>
        /// <param name="action">The action, which must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action" /> is <c>null</c>.</exception>
        public static void TryExecuteLua([NotNull] Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            try
            {
                lock (LuaLock)
                {
                    action();
                }
            }
            catch (LuaException e)
            {
                TShock.Log.ConsoleError("A Lua error occurred:");
                TShock.Log.ConsoleError(e.ToString());
                if (e.InnerException != null)
                {
                    TShock.Log.ConsoleError(e.InnerException.ToString());
                }
            }
        }
    }
}
