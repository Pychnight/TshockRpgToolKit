using Corruption.PluginSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
				result = new ValidationResult();
				result.AddError(ex.Message, FilePath);	
			}
						
			return result;
		}

		/// <summary>
		/// Allows a DefinitionBase derived class to run customized validation checks.
		/// </summary>
		protected virtual void OnValidate(ValidationResult result)
		{
		}
	}
}
