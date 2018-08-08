using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using BooTS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs
{
	/// <summary>
	/// Base implementation for types that will manage custom type overrides of Terraria types.
	/// </summary>
	/// <typeparam name="TCustomType"></typeparam>
	public abstract class CustomTypeManager<TCustomType> where TCustomType : DefinitionBase
	{
		public string BasePath { get; protected set; }
		public string ConfigPath { get; protected set; }

		/// <summary>
		/// Gets the IList of custom definitions managed by this instance.
		/// </summary>
		public IList<TCustomType> Definitions { get; protected set; }

		/// <summary>
		/// Gets or sets the Assembly name prefix to be applied during the next compile.
		/// </summary>
		/// <remarks> This is cached and applied to each ModuleManager right before compilation.</remarks>
		public string AssemblyNamePrefix { get; protected set; } = "";

		protected ModuleManager ModuleManager { get; set; }

		/// <summary>
		/// Allows a CustomTypeManager, to return a number of EnsuredMethodSignatures, used during script linking. 
		/// </summary>
		/// <returns>IEnumerable of EnsuredMethodSignature.</returns>
		protected abstract IEnumerable<EnsuredMethodSignature> GetEnsuredMethodSignatures();
		
		/// <summary>
		/// Loads in json definition files, and attempts to compile and link to any related scripts.
		/// </summary>
		protected virtual void LoadDefinitions()
		{
			Definitions = DefinitionLoader.LoadFromFile<TCustomType>(ConfigPath);

			//get script files paths
			var booScripts = Definitions.Where(d => !string.IsNullOrWhiteSpace(d.ScriptPath))
										 .Select(d => Path.Combine(BasePath, d.ScriptPath))
										 .ToList();

			var newModuleManager = new ModuleManager(CustomNpcsPlugin.Instance,
													ScriptHelpers.GetReferences(),
													ScriptHelpers.GetDefaultImports(),
													GetEnsuredMethodSignatures());

			newModuleManager.AssemblyNamePrefix = AssemblyNamePrefix;

			foreach( var f in booScripts )
				newModuleManager.Add(f);
			
			Dictionary<string, CompilerContext> results = null;

			if( ModuleManager != null )
				results = newModuleManager.IncrementalCompile(ModuleManager);
			else
				results = newModuleManager.Compile();
			
			ModuleManager = newModuleManager;

			var scriptedDefinitions = Definitions.Where(d => !string.IsNullOrWhiteSpace(d.ScriptPath));

			foreach(var def in scriptedDefinitions)
			{
				var fileName = Path.Combine(BasePath, def.ScriptPath);

				//if newly compile assembly, examine the context, and try to link to the new assembly
				if( results.TryGetValue(fileName, out var context) )
				{
					var scriptAssembly = context.GeneratedAssembly;

					if( scriptAssembly != null )
					{
						var result = def.LinkToScriptAssembly(scriptAssembly);

						//if(!result)
						//	//	CustomNpcsPlugin.Instance.LogPrint($"Failed to link {kvp.Key}.", TraceLevel.Info);
					}
				}
				else
				{
					var scriptAssembly = ModuleManager[fileName];

					if(scriptAssembly!=null)
					{
						var result = def.LinkToScriptAssembly(scriptAssembly);

						//if(!result)
						//	//	CustomNpcsPlugin.Instance.LogPrint($"Failed to link {kvp.Key}.", TraceLevel.Info);
					}
				}
			}
		}

		/// <summary>
		///     Finds the definition with the specified name.
		/// </summary>
		/// <param name="name">The name, which must not be <c>null</c>.</param>
		/// <returns>The definition, or <c>null</c> if it does not exist.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		public virtual TCustomType FindDefinition(string name)
		{
			if( name == null )
			{
				throw new ArgumentNullException(nameof(name));
			}

			return Definitions.FirstOrDefault(d => name.Equals(d.Name, StringComparison.OrdinalIgnoreCase));
		}
	}
}
