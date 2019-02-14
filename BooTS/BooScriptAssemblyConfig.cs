#if EXPERIMENT_PARALLEL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooTS
{
	/// <summary>
	/// Provides a simple means of configuring assemblies for BooScriptCompiler.ParallelCompile().
	/// </summary>
	public class BooScriptAssemblyConfig
	{
		public string AssemblyName { get; set; }
		public HashSet<string> FileNames { get; private set; }

		public BooScriptAssemblyConfig(string assemblyName, params string[] fileNames)
		{
			AssemblyName = assemblyName;
			FileNames = new HashSet<string>(fileNames);
		}
		
		public BooScriptAssemblyConfig(string assemblyName, IEnumerable<string> fileNames)
		{
			AssemblyName = assemblyName;
			FileNames = new HashSet<string>(fileNames);
		}
	}
}

#endif
