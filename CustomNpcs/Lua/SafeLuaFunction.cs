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
				ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, $"A Lua script error has originated from {executor}:", TraceLevel.Error);
				ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, lsex.ToString(), TraceLevel.Error);
				if( lsex.InnerException != null )
				{
					ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, lsex.InnerException.ToString(), TraceLevel.Error);
				}
				ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, $"Containing function will be disabled from further execution.", TraceLevel.Error);
				WrappedFunction = null;
			}
			catch( LuaException lex )
			{
				ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, $"A Lua error has originated from {executor}:", TraceLevel.Error);
				ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, lex.ToString(), TraceLevel.Error);

				if( lex.InnerException != null )
				{
					ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, lex.InnerException.ToString(), TraceLevel.Error);
				}
			}
			catch( Exception ex )
			{
				ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, $"An error has occurred in managed code, while interacting with Lua code ( {executor} ):", TraceLevel.Error);
				ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, ex.ToString(), TraceLevel.Error);

				if( ex.InnerException != null )
				{
					ServerApi.LogWriter.PluginWriteLine(CustomNpcsPlugin.Instance, ex.InnerException.ToString(), TraceLevel.Error);
				}
			}

			return null;
		}
	}
}
