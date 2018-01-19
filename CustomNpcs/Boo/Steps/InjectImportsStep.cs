using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;

namespace CustomNpcs
{
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

			foreach( var n in namespaces )
			{
				this.Namespaces.Add(n);
			}
		}

		public override void Run()
		{
			base.Visit<Module>(base.CompileUnit.Modules);
		}

		public override void OnModule(Module node)
		{
			foreach(var ns in Namespaces)
			{
				var import = new Import(ns);
				node.Imports.Add(import);
			}
		}
	}
}
