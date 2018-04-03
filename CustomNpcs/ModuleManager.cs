using Boo.Lang.Compiler;
using BooTS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs
{
	/// <summary>
	/// Caches and manages script compiling.
	/// </summary>
	public class ModuleManager
	{
		Dictionary<string, ModuleInfo> modules;
		private IEnumerable<Assembly> references;
		private IEnumerable<string> defaultImports;
		private IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures;

		public Assembly this[string fileName]
		{
			get
			{
				modules.TryGetValue(fileName, out var mi);
				return mi.Assembly;
			}
		}

		/// <summary>
		/// Gets or sets a prefix to apply to the modules built by this manager. 
		/// </summary>
		public string AssemblyNamePrefix { get; set; } = "";

		/// <summary>
		/// Gets whether the contained modules have already been compiled.
		/// </summary>
		public bool Compiled { get; private set; }

		public ModuleManager( IEnumerable<Assembly> references, IEnumerable<string> defaultImports, IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures )
		{
			modules = new Dictionary<string, ModuleInfo>();
			this.references = references;
			this.defaultImports = defaultImports;
			this.ensuredMethodSignatures = ensuredMethodSignatures;
		}

		private void compileGuard()
		{
			if( Compiled )
				throw new InvalidOperationException("Already compiled.");
		}

		/// <summary>
		/// Tries to add a module by its file path. If the module has already been added, this is silently ignored.
		/// </summary>
		/// <param name="fileName">File path to boo source file.</param>
		public void Add(string fileName)
		{
			compileGuard();

			if( string.IsNullOrWhiteSpace(fileName) )
				return;
						
			if(!modules.Keys.Contains(fileName))
			{
				var mi = new ModuleInfo();
				modules.Add(fileName,mi);
			}
		}

		/// <summary>
		/// Attempts to compile all added filenames, return a Dictionary of filenames to compilation status. If compilation has already run, this throws an exception. 
		/// </summary>
		/// <returns>Dictionary of file paths to Boo.Lang.CompilerContext's.</returns>
		public Dictionary<string,CompilerContext> Compile()
		{
			compileGuard();

			Dictionary<string, CompilerContext> contexts = new Dictionary<string, CompilerContext>();
			
			foreach(var kvp in modules)
			{
				if( kvp.Value.IsValid )
					continue;

				var scriptPath = kvp.Key;
				var scriptName = Path.GetFileNameWithoutExtension(scriptPath);
				var assemblyName = $"{AssemblyNamePrefix}{scriptName}.dll";
				var buildTime = DateTime.Now;
				var context = BooScriptCompiler.Compile(assemblyName,
														new string[] { scriptPath },
														references,
														defaultImports,
														ensuredMethodSignatures);

				contexts.Add(scriptPath, context);

				if(context.Errors.Count<1)
				{
					var mi = modules[scriptPath];

					mi.Assembly = context.GeneratedAssembly;
					mi.BuildTime = buildTime;
				}
			}

			Compiled = true;

			return contexts;
		}

		/// <summary>
		/// Attempts to intelligently compile added file paths, by using another ModuleManger as the previous state. 
		/// </summary>
		/// <param name="previous">ModuleManager holding the previous compiled modules.</param>
		/// <returns>Dictionary of file paths to Boo.Lang.CompilerContext's.</returns>
		public Dictionary<string,CompilerContext> IncrementalCompile(ModuleManager previous)
		{
			compileGuard();

			var result = new Dictionary<string, CompilerContext>();

			if( previous != null && previous.Compiled == true )
			{
				var additionalFiles = new List<string>();
				
				//find matching filenames to see if we need to recompile
				var sharedFilenames = previous.modules.Keys.Where(k => modules.ContainsKey(k)).Select(k => k);

				foreach( var sf in sharedFilenames )
				{
					var mi = previous.modules[sf];

					if( mi.IsValid )
					{
						var fileModifiedTime = File.GetLastWriteTime(sf);

						if(mi.BuildTime < fileModifiedTime)
						{
							//source file is newer than assebmly... rebuild.
							CustomNpcsPlugin.Instance.LogPrint($"{sf} has been modified.", TraceLevel.Info);
							additionalFiles.Add(sf);
						}
						else
						{
							//dont rebuild, copy existing module to our new ModuleManager;
							this.modules[sf] = mi;
						}
					}
					else
					{
						// assembly invalid, must recompile.
						CustomNpcsPlugin.Instance.LogPrint($"{sf} has no valid Assembly.", TraceLevel.Info);
						additionalFiles.Add(sf);
					}
				}

				//find new filenames that we must compile
				var newFilenames = modules.Keys.Where(k => !previous.modules.Keys.Contains(k));
				
				foreach(var nf in newFilenames)
				{
					CustomNpcsPlugin.Instance.LogPrint($"{nf} is a new module.", TraceLevel.Info);
					Add(nf);
				}
				
				foreach(var af in additionalFiles)
				{
					Add(af);
				}

				return Compile();
			}
			else
			{
				return Compile();
			}
		}

		class ModuleInfo
		{
			public Assembly Assembly;
			public DateTime BuildTime;
			public bool IsValid => Assembly != null;
		}
	}
}
