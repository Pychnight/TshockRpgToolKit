using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BooTS
{
	public static class BooHelpers
	{
		public static MethodInfo FindByName(this IEnumerable<MethodInfo> methods, string name)
		{
			var result = methods.Where(mi => mi.Name == name).FirstOrDefault();
			return result;
		}

		public static T TryCreateDelegate<T>(this MethodInfo methodInfo) where T : class
		{
			T result = null;

			try
			{
				result = methodInfo.CreateDelegate(typeof(T)) as T;
			}
			catch( Exception ex )
			{
				throw ex;
				Debug.Print(ex.Message);
			}

			return result;
		}

		[Conditional("DEBUG")]
		public static void DebugDumpAssemblies(this Assembly assembly)
		{
			var refs = assembly.GetReferencedAssemblies();

			Debug.Print($"Assembly {assembly}");

			foreach( var r in refs )
				Debug.Print($"Reference: {r}");
		}
	}
}
