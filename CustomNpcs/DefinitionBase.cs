using Corruption.PluginSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs
{
	/// <summary>
	/// Base class for custom definitions and pseudo definitions, like <see cref="CategoryDefinition"/>.
	/// </summary>
	public abstract class DefinitionBase : IValidator
	{
		public abstract string Name { get; protected internal set; } //only marking this as abstract so that derived classes override it, so we can apply json attributes. not sure if they'll work otherwise.
		public abstract string ScriptPath { get; protected internal set; }

		[JsonIgnore]
		public FilePosition FilePosition { get; set; }
				
		/// <summary>
		/// Runs validation for the definition, to check for and report on any error or warning conditions.
		/// </summary>
		/// <returns>A ValidationResult.</returns>
		public virtual ValidationResult Validate()
		{
			var result = new ValidationResult();
			return result;
		}

		/// <summary>
		/// Helper method that creates a string with the format FILENAME LINENUMBER,LINE {'DEFINITION.NAME'}.
		/// </summary>
		/// <param name="definition"></param>
		/// <returns></returns>
		internal static string CreateValidationSourceString(DefinitionBase definition)
		{
			var namePart = definition.Name != null ? $" '{definition.Name}'" : "";
			var result = $"{definition.FilePosition}{namePart}";

			return result;
		}

		/// <summary>
		/// Links a definition to an Assembly generated from a script. 
		/// </summary>
		/// <param name="assembly">Generated Assembly for ScriptPath.</param>
		public bool LinkToScriptAssembly(Assembly assembly)
		{
			//later add better error reporting / handling here!
			//...

			return OnLinkToScriptAssembly(assembly);
		}

		/// <summary>
		/// Allows a derived definition class to control the linking process to an Assembly generated from a script. 
		/// </summary>
		/// <param name="assembly">Generated Assembly for ScriptPath.</param>
		protected virtual bool OnLinkToScriptAssembly(Assembly assembly)
		{
			return false;
		}

		public virtual void OnDispose()
		{
		}
	}
}
