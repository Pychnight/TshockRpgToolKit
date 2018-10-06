using Boo.Lang.Compiler;
using Corruption.PluginSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BooTS
{
	/// <summary>
	/// A standalone, general purpose script that can be executed on demand, or loaded via filename convention and executed for specific events.
	/// </summary>
	internal class Script : BooScriptAssembly
	{
		internal const string Prefix = "boots_";

		/// <summary>
		/// Entry point for all Scripts.
		/// </summary>
		internal Action<object[]> OnRun { get; set; }
				
		internal Script(string filePath) :
			base( new string[] { filePath })
		{
		}

		//HACK - work around an api design wart. BooScriptAssembly.Build() expecs a Func with a BooScriptAssembly instance, but we also want a standalone instance method for our own use...
		internal static CompilerContext Compile(BooScriptAssembly scriptAssembly)
		{
			return ((Script)scriptAssembly).Compile();
		}
		
		internal CompilerContext Compile()
		{
			var filePath = SourceFiles.FirstOrDefault()?.FilePath;

			if (!File.Exists(filePath))
			{
				return null;
			}

			var assName = Prefix + Path.GetFileNameWithoutExtension(filePath) + ".exe";
			var cc = new BooScriptCompiler();

			var refs = ScriptHelpers.GetReferences();
			var imports = ScriptHelpers.GetDefaultImports();

			cc.Configure(refs, imports);
			//cc.InternalCompiler.Parameters.OutputType = Boo.Lang.Compiler.CompilerOutputType.ConsoleApplication;
			cc.InternalCompiler.Parameters.OutputType = Boo.Lang.Compiler.CompilerOutputType.Library;
			var context = cc.Compile(assName, new List<string>() { filePath });

			if (context.Errors.Count > 0)
			{
				BooScriptingPlugin.Instance.LogPrintBooErrors(context);
				return context;//dont bother listing warnings...
			}

			if (context.Warnings.Count > 0)
			{
				BooScriptingPlugin.Instance.LogPrintBooWarnings(context);
			}
						
			if(!Link(context.GeneratedAssembly))
			{
				BooScriptingPlugin.Instance.LogPrint($"Unable to link boo script '{filePath}'.");
				return null;
			}
			
			
			return context;
		}

		bool Link(Assembly ass)
		{
			//if (!IsBuilt)
			//	return false;
			if (ass == null)
				return false;

			var filePath = SourceFiles.FirstOrDefault()?.FilePath;

			if (string.IsNullOrWhiteSpace(filePath))
				return false;

			var linker = new BooModuleLinker(ass, filePath);

			OnRun = linker.TryCreateDelegate<Action<object[]>>("OnRun");

			return true;
		}
		
		internal bool Run(params object[] args)
		{
			try
			{
				//ass?.EntryPoint?.Invoke(null,new object[1] { new string[0] } );
				//Assembly?.EntryPoint?.Invoke(null, new object[1] { args });
				//Assembly?.EntryPoint?.Invoke(null, args);
				OnRun?.Invoke(args);
				return true;
			}
			catch( Exception ex )
			{
				BooScriptingPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
				return false;
			}
		}
	}
}
