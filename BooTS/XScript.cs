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
	/// Temporary type/name for standalone scripts that are only used for their side effects.
	/// </summary>
	public class XScript
	{
		public const string Prefix = "boots_";

		Assembly ass;

		public XScript(string filePath)
		{
			ass = null;

			if( !File.Exists(filePath) )
			{
				return;
			}

			var assName = Prefix + Path.GetFileNameWithoutExtension(filePath) + ".exe";
			var cc = new BooScriptCompiler();

			//var refs = BooHelpers.GetSystemAssemblies();
			//refs.AddRange(BooHelpers.GetBooLangAssemblies());

			//var imports = new List<string>()
			//{

			//};

			var refs = ScriptHelpers.GetReferences();
			var imports = ScriptHelpers.GetDefaultImports();

			cc.Configure(refs, imports);
			cc.InternalCompiler.Parameters.OutputType = Boo.Lang.Compiler.CompilerOutputType.ConsoleApplication;
			var context = cc.Compile(assName, new List<string>() { filePath });

			if( context.Errors.Count > 0 )
			{
				BooScriptingPlugin.Instance.LogPrintBooErrors(context);
				return;
			}

			if( context.Warnings.Count > 0 )
			{
				BooScriptingPlugin.Instance.LogPrintBooWarnings(context);
			}

			ass = context.GeneratedAssembly;
		}

		public bool Run(params string[] args)
		{
			try
			{
				//ass?.EntryPoint?.Invoke(null,new object[1] { new string[0] } );
				ass?.EntryPoint?.Invoke(null, new object[1] { args });
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
