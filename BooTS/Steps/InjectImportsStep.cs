using System.Collections.Generic;

namespace BooTS
{
	/// <summary>
	/// Injects a list of namespaces to import into a module. 
	/// </summary>
	public class InjectImportsStep : AbstractTransformerCompilerStep
	{
		public HashSet<string> Namespaces { get; private set; }

		public InjectImportsStep()
		{
			this.Namespaces = new HashSet<string>();
		}

		public void SetDefaultImports(IEnumerable<string> namespaces)
		{
			this.Namespaces.Clear();

			foreach (var n in namespaces)
			{
				this.Namespaces.Add(n);
			}
		}

		public override void Run() => base.Visit<Module>(base.CompileUnit.Modules);

		public override void OnModule(Module node)
		{
			foreach (var ns in Namespaces)
			{
				var import = new Import(ns);
				node.Imports.Add(import);
			}
		}
	}
}
