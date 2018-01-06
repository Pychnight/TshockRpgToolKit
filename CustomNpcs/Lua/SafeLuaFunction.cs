using NLua;
using NLua.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
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
				CustomNpcsPlugin.Instance.LogPrint($"A Lua script error has originated from {executor}:", TraceLevel.Error);
				CustomNpcsPlugin.Instance.LogPrint(lsex.ToString(), TraceLevel.Error);
				if( lsex.InnerException != null )
				{
					CustomNpcsPlugin.Instance.LogPrint(lsex.InnerException.ToString(), TraceLevel.Error);
				}
				CustomNpcsPlugin.Instance.LogPrint( $"Containing function will be disabled from further execution.", TraceLevel.Error);
				WrappedFunction = null;
			}
			catch( LuaException lex )
			{
				CustomNpcsPlugin.Instance.LogPrint($"A Lua error has originated from {executor}:", TraceLevel.Error);
				CustomNpcsPlugin.Instance.LogPrint(lex.ToString(), TraceLevel.Error);

				if( lex.InnerException != null )
				{
					CustomNpcsPlugin.Instance.LogPrint(lex.InnerException.ToString(), TraceLevel.Error);
				}
			}
			catch( Exception ex )
			{
				CustomNpcsPlugin.Instance.LogPrint($"An error has occurred in managed code, while interacting with Lua code ( {executor} ):", TraceLevel.Error);
				CustomNpcsPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);

				if( ex.InnerException != null )
				{
					CustomNpcsPlugin.Instance.LogPrint(ex.InnerException.ToString(), TraceLevel.Error);
				}
			}

			return null;
		}
	}
}
