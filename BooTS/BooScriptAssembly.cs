#if EXPERIMENT_PARALLEL

using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BooTS
{
	/// <summary>
	/// Wraps a Boo generated, dynamic Assembly. 
	/// </summary>
	public class BooScriptAssembly
	{
		public Assembly Assembly { get; internal set; }
		public DateTime BuildTime { get; internal set; }
		public IList<string> SourceFiles { get; private set; }
		public bool IsValid { get; private set; }
		
		internal BooScriptAssembly(DateTime buildTime, CompilerContext context)
		{
			BuildTime = buildTime;

			var fileNames = context.Parameters.Input.Where(i => i is FileInput)
													.Select(fi => fi.Name)
													.Distinct()
													.ToList();

			SourceFiles = fileNames.AsReadOnly();
			IsValid = context.Errors.Count == 0 && context.GeneratedAssembly != null;
			Assembly = context.GeneratedAssembly;
		}

		//public bool RequiresRebuild()
		//{
		//	if( !IsValid )
		//		return true;

			
		//}
	}
}

#endif
