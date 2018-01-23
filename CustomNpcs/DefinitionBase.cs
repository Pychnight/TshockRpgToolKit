using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs
{
	public abstract class DefinitionBase
	{
		public abstract string Name { get; protected internal set; } //only marking this as abstract so that derived classes override it, so we can apply json attributes. not sure if they'll work otherwise.

		protected internal abstract void ThrowIfInvalid();
	}
}
