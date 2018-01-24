using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomNpcs
{
	public class BooScriptCompiler
	{
		static BooScriptCompiler Instance = new BooScriptCompiler();

		CompilerParameters parameters;
		BooCompiler compiler;

		InjectImportsStep injectImportsStep;
		EnsureMethodSignaturesStep ensureMethodSignaturesStep;

		private BooScriptCompiler()
		{
			compiler = new BooCompiler();

			parameters = compiler.Parameters;

			parameters.GenerateInMemory = true;
			parameters.Ducky = true;
			parameters.WhiteSpaceAgnostic = false;
			//parameters.References.Clear();
			//parameters.LoadDefaultReferences();
			//parameters.StdLib = false;
			
			//var otapiAss = assemblies.Where(a => a.FullName.Contains("OTAPI")).Select( a => Assembly.Load( a )).SingleOrDefault();
			var assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
			var sysAss = Assembly.GetAssembly(typeof(Random));
			var otapiAss = Assembly.GetAssembly(typeof(Vector2));
			var tshockAss = Assembly.GetAssembly(typeof(TSPlayer));
			var pluginAss = Assembly.GetExecutingAssembly();

			parameters.DisabledWarnings.Add("BCW0016");//dont warn about unused namespaces...

			//parameters.References.Add(sysAss);
			parameters.References.Add(otapiAss);
			parameters.References.Add(tshockAss);
			parameters.References.Add(pluginAss);

			//parameters.AddAssembly(sysAss);
			//parameters.AddAssembly(otapiAss);
			//parameters.AddAssembly(tshockAss);
			//parameters.AddAssembly(pluginAss);
			
			parameters.OutputType = CompilerOutputType.Library;
			parameters.OutputAssembly = "scripts.dll";
			parameters.GenerateCollectible = true; //dont leak assemblies...
			var pipeline = new CompileToMemory();//...when we compile them to memory.
												 //parameters.Pipeline = new CompileToFile();
												 //parameters.Pipeline = new Parse();

			injectImportsStep = new InjectImportsStep();
			pipeline.Insert(1,injectImportsStep);

			ensureMethodSignaturesStep = new EnsureMethodSignaturesStep();

			pipeline.Insert(2, ensureMethodSignaturesStep);

			parameters.Pipeline = pipeline;
		}

		public static Assembly Compile(string assemblyName, List<string> fileNames, IEnumerable<string> imports = null, IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures = null)
		{
			Instance.parameters.OutputAssembly = assemblyName;
			
			Instance.parameters.Input.Clear();

			//multiple npc types may use the same script
			var distinctFileNames = fileNames.Distinct();

			foreach(var fname in distinctFileNames)
				Instance.parameters.Input.Add(new FileInput(fname));

			Instance.injectImportsStep.Namespaces.Clear();
			if(imports!=null)
			{
				Instance.injectImportsStep.SetDefaultImports(imports);
			}

			Instance.ensureMethodSignaturesStep.SetEnsuredMethodSignatures(ensuredMethodSignatures);
			
			var context = Instance.compiler.Run();

			if( context.Errors.Count > 0 )
			{
				foreach( var err in context.Errors )
				{
					CustomNpcsPlugin.Instance.LogPrint($"{err.LexicalInfo.FileName} {err.LexicalInfo.Line},{err.LexicalInfo.Column}: {err.Message}", TraceLevel.Error);
				}

				return null;
			}

			if( context.Warnings.Count > 0 )
			{
				foreach( var warn in context.Warnings )
				{
					CustomNpcsPlugin.Instance.LogPrint($"{warn.LexicalInfo.FileName} {warn.LexicalInfo.Line},{warn.LexicalInfo.Column}: {warn.Message}", TraceLevel.Warning);
				}
			}

			//context.GeneratedAssembly.DebugDumpAssemblies();
			
			return context.GeneratedAssembly;
		}
	}
}
