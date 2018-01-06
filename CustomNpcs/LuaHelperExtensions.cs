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
	public static class LuaHelperExtensions
	{
		/// <summary>
		/// Helper to get a single, typed result from a LuaFunction or SafeLuaFunction call.
		/// </summary>
		/// <typeparam name="T">Type of expected result.</typeparam>
		/// <param name="luaResults">Object array of results</param>
		/// <returns>The result value if successful, or null otherwise.</returns>
		public static T? GetResult<T>(this object[] luaResults) where T : struct
		{
			if(luaResults!=null && luaResults.Length > 0 && luaResults[0] is T)
				return (T)luaResults[0];
			
			return null;
		}

		/// <summary>
		/// Creates a SafeLuaFunction wrapper for the named function. 
		/// </summary>
		/// <param name="lua">Lua instance.</param>
		/// <param name="functionName">Function name.</param>
		/// <returns>A SafeLuaFunction.</returns>
		public static SafeLuaFunction GetSafeFunction(this Lua lua, string functionName)
		{
			var luaFunction = lua[functionName] as LuaFunction;

			return new SafeLuaFunction(luaFunction);
		}
	}
}
