using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.PluginSupport
{
	/// <summary>
	/// Interface for types that can check themselves, and/or contained data, and return a <see cref="ValidationResult"/> with error and warning information.
	/// </summary>
	public interface IValidator
	{
		/// <summary>
		/// Runs integrity checking, and returns a <see cref="ValidationResult"/> containing error and warnings found.   
		/// </summary>
		/// <returns>ValidationResult.</returns>
		ValidationResult Validate();
	}
}
