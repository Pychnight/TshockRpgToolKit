using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using BooTS;
using Leveling.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Leveling.LoaderDsl
{
	class ClassCompiler
	{
		internal const string AssemblyPrefix = "ClassDef_";

		BooCompiler compiler;
		InjectImportsStep injectImportsStep;
		EnsureMethodSignaturesStep ensureMethodSignaturesStep;

		public ClassCompiler()
		{
			compiler = BooHelpers.CreateBooCompiler();
			var parameters = compiler.Parameters;

			parameters.GenerateInMemory = true;
			parameters.Ducky = true;
			parameters.WhiteSpaceAgnostic = false;
			parameters.References.Clear();
			//parameters.LoadDefaultReferences();//do not use! This will cause failures on future calls, within the context of a TShockPlugin(unknown reason).
			//parameters.StdLib = false;

			foreach( var ass in BooHelpers.GetSystemAssemblies() )
				parameters.References.Add(ass);

			foreach( var ass in BooHelpers.GetBooLangAssemblies() )
				parameters.References.Add(ass);

			var pluginAss = Assembly.GetExecutingAssembly();

			parameters.References.Add(pluginAss);

			parameters.DisabledWarnings.Add("BCW0016");//dont warn about unused namespaces...
			parameters.OutputType = CompilerOutputType.ConsoleApplication;
			parameters.OutputAssembly = "scripts.dll";
			parameters.GenerateCollectible = true; //dont leak assemblies...
			var pipeline = new CompileToMemory();//...when we compile them to memory.
												 //parameters.Pipeline = new CompileToFile();
												 //parameters.Pipeline = new Parse();

			var imports = injectImportsStep = new InjectImportsStep();
			pipeline.Insert(1, injectImportsStep);

			ensureMethodSignaturesStep = new EnsureMethodSignaturesStep();

			pipeline.Insert(2, ensureMethodSignaturesStep);

			parameters.Pipeline = pipeline;

			var namespaces = new List<string>()
			{
				"System",
				"System.Collections.Generic",
				"Microsoft.Xna.Framework",
				"TShockAPI",
				"Leveling.LoaderDsl.ClassDsl",
				"Leveling.LoaderDsl.LevelDsl"
			};

			imports.SetDefaultImports(namespaces);
		}
		
		public ClassDefinition LoadClassDefinition(string fileName)
		{
			var ps = compiler.Parameters;
			var input = new FileInput(fileName);

			ps.Input.Clear();
			ps.Input.Add(input);
			ps.OutputAssembly = $"{AssemblyPrefix}{Path.GetFileNameWithoutExtension(fileName)}.dll";

			var context = compiler.Run();

			if( context.Errors.Count > 0 )
			{
				foreach( var err in context.Errors )
				{
					var li = err.LexicalInfo;
					Debug.Print($"Error: {li.FileName} ({li.Line},{li.Column}) {err.Message}");
				}

				return null;
			}
			else
			{
				var ass = context.GeneratedAssembly;
				var def = new ClassDefinition();

				try
				{
					ClassDsl.ClassDefinition = def;
					
					var linker = new BooModuleLinker(ass, fileName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					var main = linker["Main"];
					
					//build definitions...
					main.Invoke(null, new object[1]);

					//try to link to callbacks...
					def.OnMaximumCurrency = linker["OnMaximumCurrency"]?.TryCreateDelegate<Action<object>>();
					def.OnNegativeCurrency = linker["OnNegativeCurrency"]?.TryCreateDelegate<Action<object>>();
					def.OnLevelUp = linker["OnLevelUp"]?.TryCreateDelegate<Action<object>>();
					def.OnLevelDown = linker["OnLevelDown"]?.TryCreateDelegate<Action<object>>();
					def.OnClassChange = linker["OnClassChange"]?.TryCreateDelegate<Action<object>>();
					def.OnClassMastered = linker["OnClassMastered"]?.TryCreateDelegate<Action<object>>();

					return def;
				}
				catch(Exception ex)
				{
					Debug.Print("LoadClassDefinition failed.");
					Debug.Print(ex.Message);
					return null;
				}
			}
		}
	}
}
