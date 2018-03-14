using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs
{
	public abstract class DefinitionBase
	{
		/// <summary>
		/// Gets or sets the path to the file that this Definition is defined in.
		/// </summary>
		public string FilePath { get; set; } = "";

		public abstract string Name { get; protected internal set; } //only marking this as abstract so that derived classes override it, so we can apply json attributes. not sure if they'll work otherwise.

		public abstract string ScriptPath { get; protected internal set; }

		protected internal abstract void ThrowIfInvalid();
	}
}
