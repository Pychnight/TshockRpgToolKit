using NLua;
using NLua.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomNpcs
{
	/// <summary>
	/// A safe wrapper for LuaFunctions, which automatically handles exceptions and disables future calls if a LuaScriptException is thrown.
	/// </summary>
	public class SafeLuaFunction
	{
		public bool IsLoaded { get => WrappedFunction != null; }
		public LuaFunction WrappedFunction { get; private set; }

		public SafeLuaFunction(LuaFunction luaFunction)
		{
			WrappedFunction = luaFunction;
		}

		public object[] Call(string executor, params object[] args)
		{
			try
			{
				if( !IsLoaded )
					return null;

				return WrappedFunction.Call(args);
			}
			catch( LuaScriptException lsex )
			{
				TShock.Log.ConsoleError($"[CustomNpcs] A Lua script error occurred from {executor}:");
				TShock.Log.ConsoleError(lsex.ToString());
				if( lsex.InnerException != null )
				{
					TShock.Log.ConsoleError(lsex.InnerException.ToString());
				}
				TShock.Log.ConsoleError($"[CustomNpcs] Disabling function from further execution.");
				WrappedFunction = null;
			}
			catch( LuaException lex )
			{
				TShock.Log.ConsoleError($"[CustomNpcs] A Lua error occurred from {executor}:");
				TShock.Log.ConsoleError(lex.ToString());
				if( lex.InnerException != null )
				{
					TShock.Log.ConsoleError(lex.InnerException.ToString());
				}
			}
			catch( Exception ex )
			{
				TShock.Log.ConsoleError($"[CustomNpcs] An error occurred in managed code, while interacting with Lua code ( {executor} ):");
				TShock.Log.ConsoleError(ex.ToString());
				if( ex.InnerException != null )
				{
					TShock.Log.ConsoleError(ex.InnerException.ToString());
				}
			}

			return null;
		}
	}
}
