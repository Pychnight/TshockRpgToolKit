using System;
using JetBrains.Annotations;
using NLua.Exceptions;
using TShockAPI;

namespace CustomNpcs
{
    internal static class Utils
    {
        private static readonly object LuaLock = new object();

        public static void TryExecuteLua([NotNull] Action action)
        {
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
