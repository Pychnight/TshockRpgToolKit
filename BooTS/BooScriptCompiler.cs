using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
//using Corruption;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BooTS
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

			parameters.DisabledWarnings.Add("BCW0016");//dont warn about unused namespaces...
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

		public static CompilerContext Compile(string assemblyName, List<string> fileNames, IEnumerable<Assembly> references, IEnumerable<string> imports = null, IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures = null)
		{
			var ps = Instance.parameters;

			ps.OutputAssembly = assemblyName;
			
			//add inputs
			ps.Input.Clear();

			if( fileNames != null )
			{
				//multiple npc types may use the same script
				var distinctFileNames = fileNames.Distinct();

				foreach( var fname in distinctFileNames )
					ps.Input.Add(new FileInput(fname));
			}
			
			//add references
			ps.References.Clear();

			if( references!=null)
			{
				foreach( var r in references )
					ps.References.Add(r);
			}
							
			//add default imports
			Instance.injectImportsStep.Namespaces.Clear();
			
			if(imports!=null)
			{
				if( imports != null )
					Instance.injectImportsStep.SetDefaultImports(imports);
			}

			//add ensure method sigs
			Instance.ensureMethodSignaturesStep.SetEnsuredMethodSignatures(ensuredMethodSignatures);
			
			var context = Instance.compiler.Run();
			return context;
		}
	}
}
