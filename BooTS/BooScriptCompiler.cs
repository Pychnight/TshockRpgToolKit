using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Environments;
//using Corruption;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
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
		BooCompiler compiler;
		InjectImportsStep injectImportsStep;
		EnsureMethodSignaturesStep ensureMethodSignaturesStep;

		public BooCompiler InternalCompiler { get { return compiler; } }

		public BooScriptCompiler()
		{
			compiler = BooHelpers.CreateBooCompiler();
			var parameters = compiler.Parameters;

			//parameters.Environment = 

			parameters.GenerateInMemory = true;
			parameters.Ducky = true;
			parameters.WhiteSpaceAgnostic = false;
			parameters.References.Clear();
			//parameters.LoadDefaultReferences();//do not use! This will cause failures on future calls, within the context of a TShockPlugin(unknown reason).
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

		public void Configure(IEnumerable<Assembly> references, IEnumerable<string> imports = null, IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures = null)
		{
			var ps = compiler.Parameters;
			
			//add references
			ps.References.Clear();

			if( references != null )
			{
				foreach( var r in references )
					ps.References.Add(r);
			}

			//add default imports
			injectImportsStep.Namespaces.Clear();

			if( imports != null )
			{
				if( imports != null )
					injectImportsStep.SetDefaultImports(imports);
			}

			//add ensure method sigs
			ensureMethodSignaturesStep.SetEnsuredMethodSignatures(ensuredMethodSignatures);
		}

		public CompilerContext Compile(string assemblyName, IEnumerable<string> fileNames)
		{
			var ps = compiler.Parameters;

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

			return compiler.Run();
		}
		
		public static CompilerContext Compile(string assemblyName, IEnumerable<string> fileNames, IEnumerable<Assembly> references, IEnumerable<string> imports = null, IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures = null)
		{
			var cc = new BooScriptCompiler();
			var ps = cc.compiler.Parameters;

			cc.Configure(references, imports, ensuredMethodSignatures);

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
		
			return cc.compiler.Run();
		}

#if EXPERIMENT_PARALLEL

		public static Dictionary<string,BooScriptAssembly> ParallelCompile(IEnumerable<BooScriptAssemblyConfig> scriptConfigs, IEnumerable<Assembly> references, IEnumerable<string> imports = null, IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures = null)
		{
			var assemblies = new ConcurrentDictionary<string, BooScriptAssembly>();
			var configs = new Dictionary<string, BooScriptAssemblyConfig>();
			var bc = new BlockingCollection<CompilerContext>();
			
			foreach(var cfg in scriptConfigs)
			{
				if(!configs.ContainsKey(cfg.AssemblyName))
				{
					configs.Add(cfg.AssemblyName, cfg);
				}
				else
				{
					//duplicate found
					Console.WriteLine($"Skipping build of duplicate Assembly, '{cfg.AssemblyName}'");
				}
			}
			
			//compiles scripts into assemblies, using n tasks/threads.
			var compileTask = Task.Factory.StartNew(() =>
			{
				var result = Parallel.ForEach(configs.Values, cfg =>
				{
					var buildTime = DateTime.Now;
					var context = Compile(cfg.AssemblyName, cfg.FileNames, references, imports, ensuredMethodSignatures);
					var scriptAssembly = new BooScriptAssembly(buildTime, context);
					
					assemblies.TryAdd(cfg.AssemblyName, scriptAssembly);

					bc.Add(context);
				});

				bc.CompleteAdding();
			});

			//consumes contexts, and writes out errors or warnings..
			var reportTask = Task.Factory.StartNew(() =>
			{
				try
				{
					Debug.WriteLine($"Starting reporting...");

					while( true )
					{
						var context = bc.Take();

						if( context.Errors.Count > 0 )
						{
							foreach( var err in context.Errors )
								Console.WriteLine($"Error: {err.LexicalInfo.FullPath} {err.LexicalInfo.Line},{err.LexicalInfo.Column} {err.Message}");
						}
						else if( context.Warnings.Count > 0 )
						{
							foreach( var war in context.Warnings )
								Console.WriteLine($"Warning: {war.LexicalInfo.FullPath} {war.LexicalInfo.Line},{war.LexicalInfo.Column} {war.Message}");
						}
					}
				}
				catch( InvalidOperationException ioex )
				{
					//end
					Debug.WriteLine($"BlockingCollection threw InvalidOperationException. ( Probably completed. )");
				}
				catch( Exception ex )
				{
					Debug.WriteLine($"BlockingCollection threw exception; {ex.Message}");
				}
			});

			Task.WaitAll(reportTask, compileTask);

			//Task.WaitAll(compileTask);

			return new Dictionary<string, BooScriptAssembly>(assemblies);
		}

		public static Dictionary<string, BooScriptAssembly> SerialCompile(IEnumerable<BooScriptAssemblyConfig> scriptConfigs, IEnumerable<Assembly> references, IEnumerable<string> imports = null, IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures = null)
		{
			var assemblies = new Dictionary<string, BooScriptAssembly>();
			var configs = new Dictionary<string, BooScriptAssemblyConfig>();
			var bc = new BlockingCollection<CompilerContext>();

			foreach( var cfg in scriptConfigs )
			{
				if( !configs.ContainsKey(cfg.AssemblyName) )
				{
					configs.Add(cfg.AssemblyName, cfg);
				}
				else
				{
					//duplicate found
					Console.WriteLine($"Skipping build of duplicate Assembly, '{cfg.AssemblyName}'");
				}
			}

			//compiles scripts into assemblies, using n tasks/threads.
			foreach(var kvp in configs)
			{
				var cfg = kvp.Value;
				var buildTime = DateTime.Now;
				var context = Compile(cfg.AssemblyName, cfg.FileNames, references, imports, ensuredMethodSignatures);
				var scriptAssembly = new BooScriptAssembly(buildTime, context);

				assemblies.Add(cfg.AssemblyName, scriptAssembly);

				if( context.Errors.Count > 0 )
				{
					foreach( var err in context.Errors )
						Console.WriteLine($"Error: {err.LexicalInfo.FullPath} {err.LexicalInfo.Line},{err.LexicalInfo.Column} {err.Message}");
				}
				else if( context.Warnings.Count > 0 )
				{
					foreach( var war in context.Warnings )
						Console.WriteLine($"Warning: {war.LexicalInfo.FullPath} {war.LexicalInfo.Line},{war.LexicalInfo.Column} {war.Message}");
				}
			}
			
			return assemblies;
		}

#endif
	}
}
