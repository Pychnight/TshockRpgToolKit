using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;

namespace CustomQuests.Scripting
{
	//this only handles a single module currently.!

	/// <summary>
	/// Converts a Boo module's global statements and members into an instance class.
	/// </summary>
	public class ModuleToInstanceClassStep : AbstractTransformerCompilerStep
	{
		/// <summary>
		/// Fully qualified name of module to convert from.
		/// </summary>
		public string SourceModuleName { get; set; }

		/// <summary>
		/// Fully qualified type name of the class to derive from, if any.
		/// </summary>
		public string TargetBaseClassName { get; set; }

		/// <summary>
		/// Name of the class to create.
		/// </summary>
		public string TargetClassName { get; set; }

		/// <summary>
		/// Name of the target method which will hold the global statements.
		/// </summary>
		public string TargetMethodName { get; set; }

		public TypeMemberModifiers TargetMethodModifiers { get; set; } = TypeMemberModifiers.Public;

		public override void Run()
		{
			base.Visit<Module>(base.CompileUnit.Modules);
		}

		public override void OnModule(Module node)
		{
			if( node.FullName != SourceModuleName )
				return;

			Debug.Print($"Converting module '{node.FullName}' to instance class...");
			var globals = node.Globals;//we need to take all these statements, and make them a method body
			var members = node.Members;

			node.Replace(node.Globals, null);
			node.Members = new TypeMemberCollection();

			var klass = new ClassDefinition();

			klass.Name = TargetClassName;
			var baseClassRef = new SimpleTypeReference(TargetBaseClassName);
			klass.BaseTypes.Add(baseClassRef);
			klass.Visibility = TypeMemberModifiers.Public;

			var meth = new Method(TargetMethodName);
			meth.Visibility = TargetMethodModifiers; //TypeMemberModifiers.Protected | TypeMemberModifiers.Override;
			//meth.ReturnType = typeref
			meth.Body = globals;
				
			klass.Members.Add(meth);

			foreach(var m in members)
				klass.Members.Add(m);

			node.Members.Add(klass);

			Debug.Print(klass.ToCodeString());
		}
	}
}
