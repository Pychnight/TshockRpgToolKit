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
		public IList<TCustomType> Definitions { get; protected set; }
		
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

			var newModuleManager = new ModuleManager(ScriptHelpers.GetReferences(),
													ScriptHelpers.GetDefaultImports(),
													GetEnsuredMethodSignatures());

			foreach( var f in booScripts )
				newModuleManager.Add(f);
			
			Dictionary<string, CompilerContext> results = null;

			if( ModuleManager != null )
				results = newModuleManager.IncrementalCompile(ModuleManager);
			else
				results = newModuleManager.Compile();
			
			ModuleManager = newModuleManager;

			foreach( var kvp in results )
			{
				var context = kvp.Value;

				CustomNpcsPlugin.Instance.LogPrintBooErrors(context);

				if( context.Errors.Count < 1 )
					CustomNpcsPlugin.Instance.LogPrintBooWarnings(context);

				var scriptAssembly = context.GeneratedAssembly;

				if( scriptAssembly != null )
				{
					CustomNpcsPlugin.Instance.LogPrint($"Compiled {kvp.Key}.", TraceLevel.Info);

					var fileInput = (FileInput)context.Parameters.Input.First();

					foreach( var def in Definitions )
					{
						if( string.IsNullOrWhiteSpace(def.ScriptPath) )
							continue;

						var fileName = Path.Combine(BasePath, def.ScriptPath);

						if( fileName == fileInput.Name )
						{
							def.LinkToScriptAssembly(scriptAssembly);
							
						}
					}
				}
				else
				{
					CustomNpcsPlugin.Instance.LogPrint($"Compile failed on {kvp.Key}.", TraceLevel.Info);
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
