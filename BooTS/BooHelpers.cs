﻿using Boo.Lang.Compiler;
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
		/// <summary>
		/// "Safe" means of creating a BooCompiler within a TShock plugin. 
		/// </summary>
		/// <remarks>This creates compiler without calling LoadDefaultReferences(), which causes exceptions under TShock.</remarks>
		/// <returns>BooCompiler.</returns>
		public static BooCompiler CreateBooCompiler()
		{
			var parameters = new CompilerParameters(false);//we must supply our own parameters object to the compiler, to avoid call to LoadDefautlReferences()
			var compiler = new BooCompiler(parameters);

			return compiler;
		}

		public static List<Assembly> GetBooLangAssemblies()
		{
			var result = new List<Assembly>();

			var booLangAss = Assembly.LoadWithPartialName("Boo.Lang");
			var booUsefulAss = Assembly.LoadWithPartialName("Boo.Lang.Useful");
			var booExtAss = Assembly.LoadWithPartialName("Boo.Lang.Extensions");

			result.Add(booLangAss);
			result.Add(booUsefulAss);
			result.Add(booExtAss);

			return result;
		}

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
