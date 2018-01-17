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
	//public class SafeBooMethod
	//{
	//	Func<object,object[]> func;

	//	public SafeBooMethod(MethodInfo methodInfo)
	//	{
	//		methodInfo.CreateDelegate(typeof(Func<object>));

	//		methodInfo.Invoke()
	//	}
	//}
	
	public class BooScriptCompiler
	{
		static BooScriptCompiler Instance = new BooScriptCompiler();

		CompilerParameters parameters;
		BooCompiler compiler;

		private BooScriptCompiler()
		{
			compiler = new BooCompiler();

			parameters = compiler.Parameters;

			parameters.GenerateInMemory = true;
			parameters.Ducky = true;
			parameters.WhiteSpaceAgnostic = false;
			parameters.LoadDefaultReferences();

			//foreach(var ass in assemblies )
			//{
			//	Debug.Print($"Referenced assembly: {ass}");
			//}

			//var otapiAss = assemblies.Where(a => a.FullName.Contains("OTAPI")).Select( a => Assembly.Load( a )).SingleOrDefault();
			var assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
			var otapiAss = Assembly.GetAssembly(typeof(Vector2));
			var tshockAss = Assembly.GetAssembly(typeof(TSPlayer));
			var pluginAss = Assembly.GetExecutingAssembly();

			parameters.References.Add(otapiAss);
			parameters.References.Add(tshockAss);
			parameters.References.Add(pluginAss);

			parameters.OutputType = CompilerOutputType.Library;
			parameters.OutputAssembly = "scripts.dll";
			parameters.GenerateCollectible = true; //dont leak assemblies...
			parameters.Pipeline = new CompileToMemory();//...when we compile them to memory.
														//parameters.Pipeline = new CompileToFile();
														//parameters.Pipeline = new Parse();
			
		}

		public static Assembly Compile(string assemblyName, List<string> fileNames)
		{
			Instance.parameters.OutputAssembly = assemblyName;
			
			Instance.parameters.Input.Clear();
			foreach(var fname in fileNames)
				Instance.parameters.Input.Add(new FileInput(fname));

			var context = Instance.compiler.Run();

			if( context.Errors.Count > 0 )
			{
				foreach( var err in context.Errors )
				{
					//CustomNpcsPlugin.Instance.LogPrint(err.Message, TraceLevel.Error);
					Console.WriteLine($"{err.LexicalInfo.FileName} {err.LexicalInfo.Line},{err.LexicalInfo.Column}: {err.Message}");
				}

				return null;
			}

			if( context.Warnings.Count > 0 )
			{
				foreach( var warn in context.Warnings )
				{
					//CustomNpcsPlugin.Instance.LogPrint(warning.Message, TraceLevel.Warning);
					Console.WriteLine($"{warn.LexicalInfo.FileName} {warn.LexicalInfo.Line},{warn.LexicalInfo.Column}: {warn.Message}");
				}
			}

			return context.GeneratedAssembly;
		}
	}
}
