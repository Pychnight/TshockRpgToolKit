using Boo.Lang.Compiler;
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
		/// Do not use. Used only to guarantee that Boo.Lang.Extensions.dll is copied.
		/// </summary>
		public static Boo.Lang.Extensions.AssertMacro DummyExtensionLoadHack => new Boo.Lang.Extensions.AssertMacro();

		/// <summary>
		/// Do not use. Used only to guarantee that Boo.Lang.Useful.dll is copied.
		/// </summary>
		public static Boo.Lang.Useful.Collections.Set DummyUsefulLoadHack => new Boo.Lang.Useful.Collections.Set();

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

#pragma warning disable 612, 618

			var booLangAss = Assembly.LoadWithPartialName("Boo.Lang");
			var booUsefulAss = Assembly.LoadWithPartialName("Boo.Lang.Useful");
			var booExtAss = Assembly.LoadWithPartialName("Boo.Lang.Extensions");

#pragma warning restore 612, 618

			result.Add(booLangAss);
			result.Add(booUsefulAss);
			result.Add(booExtAss);

			return result;
		}

		public static List<Assembly> GetSystemAssemblies()
		{
			var result = new List<Assembly>();

#pragma warning disable 612, 618

			var mscorAss = Assembly.LoadWithPartialName("mscorlib");
			var sysAss = Assembly.LoadWithPartialName("System");
			
#pragma warning restore 612, 618

			result.Add(mscorAss);
			result.Add(sysAss);
			
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
