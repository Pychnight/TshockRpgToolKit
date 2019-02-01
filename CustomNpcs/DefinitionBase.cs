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
	public abstract class DefinitionBase : IValidator, IJsonLineInfo
	{
		public abstract string Name { get; protected internal set; } //only marking this as abstract so that derived classes override it, so we can apply json attributes. not sure if they'll work otherwise.
		public abstract string ScriptPath { get; protected internal set; }

		/// <summary>
		/// Gets or sets the path to the file that this Definition is defined in.
		/// </summary>
		public string FilePath { get; set; } = "";
		public int LineNumber { get; set; }
		public int LinePosition { get; set; }

		bool IJsonLineInfo.HasLineInfo()
		{
			return true;
		}

		/// <summary>
		/// Runs validation for the definition, to check for and report on any error or warning conditions.
		/// </summary>
		/// <returns>A ValidationResult.</returns>
		public ValidationResult Validate()
		{
			var result = new ValidationResult();

			try
			{
				OnValidate(result);
			}
			catch(Exception ex)
			{
				//exceptions are automatically flagged as errors
				result.Errors.Add(new ValidationError(ex.Message, FilePath));
			}
						
			return result;
		}

		/// <summary>
		/// Allows a DefinitionBase derived class to run customized validation checks.
		/// </summary>
		protected virtual void OnValidate(ValidationResult result)
		{
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
