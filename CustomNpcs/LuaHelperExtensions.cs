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
		public static T? GetResult<T>(this object[] luaResults) where T : struct
		{
			if(luaResults!=null && luaResults.Length > 0 && luaResults[0] is T)
				return (T)luaResults[0];
			
			return null;
		}

		public static SafeLuaFunction GetSafeFunction(this Lua lua, string functionName)
		{
			var luaFunction = lua[functionName] as LuaFunction;

			return new SafeLuaFunction(luaFunction);
		}
	}
}
