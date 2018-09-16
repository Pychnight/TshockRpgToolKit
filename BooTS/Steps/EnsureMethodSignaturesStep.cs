using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooTS
{
	public class EnsuredParameter
	{
		public string DefaultName { get; set; }
		public Type Type { get; set; } = typeof(object);

		public EnsuredParameter() {}
		public EnsuredParameter(string defaultName, Type type)
		{
			DefaultName = defaultName;
			Type = type;
		}
	}

	/// <summary>
	/// Represents a real method signature, thats used for script methods which use shorthand signatures. This is used by
	/// the EnsureMethodSignaturesStep to insert the real method signatures at compile time, and avoid the runtime costs of
	/// duck typing.
	/// </summary>
	public class EnsuredMethodSignature
	{
		public string Name { get; private set; }
		public List<EnsuredParameter> Parameters { get; private set; }
		public Type ReturnType { get; set; }

		public EnsuredMethodSignature(string name) :
			this(name, typeof(void))
		{
		}

		public EnsuredMethodSignature(string name, Type returnType) :
			this(name, returnType, new List<EnsuredParameter>())
		{
		}

		public EnsuredMethodSignature(string name, Type returnType, IEnumerable<EnsuredParameter> parameters )
		{
			Name = name;
			ReturnType = returnType;
			Parameters = new List<EnsuredParameter>(parameters);
		}

		public EnsuredMethodSignature AddParameter(string name, Type type)
		{
			var parameter = new EnsuredParameter(name, type);
			Parameters.Add(parameter);
			return this;
		}

		internal void Ensure(CompilerContext context, Method method)
		{
			if( method.Parameters.Count != this.Parameters.Count )
			{
				context.Errors.Add(CompilerErrorFactory.MethodArgumentCount(method, method.Name, this.Parameters.Count));
				return;
			}

			//ensure return type
			if(method.ReturnType==null)
				method.ReturnType = new SimpleTypeReference(ReturnType.FullName);

			//ensure parameter types			
			for( var i = 0;i<this.Parameters.Count;i++)
			{
				var paramtTypeRef = method.Parameters[i].Type;
					
				if(paramtTypeRef==null)
				{
					var ensuredType = this.Parameters[i].Type;
					TypeReference typeRef = null;

					if(ensuredType.IsGenericType)
					{
						var args = new SimpleTypeReference(ensuredType.GenericTypeArguments[0].Name);

						var name = ensuredType.Name;
						var tick = name.IndexOf('`');

						name = name.Substring(0, tick);
						
						typeRef = new GenericTypeReference(name,args);
					}
					else
					{
						typeRef = new SimpleTypeReference(ensuredType.FullName);
						
					}

					method.Parameters[i].Type = typeRef;
				}
			}
		}
	}

	/// <summary>
	/// Inserts method signatures at compile time, to avoid the runtime costs of duck typing when not specifying parameter types.
	/// </summary>
	public class EnsureMethodSignaturesStep : AbstractTransformerCompilerStep
	{
		Dictionary<string,EnsuredMethodSignature> signatures;

		public EnsureMethodSignaturesStep()
		{
			signatures = new Dictionary<string, EnsuredMethodSignature>();
		}

		public void SetEnsuredMethodSignatures(IEnumerable<EnsuredMethodSignature> ensuredMethodSignatures)
		{
			signatures.Clear();

			if( ensuredMethodSignatures == null )
				return;

			foreach(var es in ensuredMethodSignatures)
				signatures.Add(es.Name, es);
		}
		
		public override void Run()
		{
			if( signatures.Count < 1 )
				return;

			base.Visit<Module>(base.CompileUnit.Modules);
		}

		public override void OnModule(Module node)
		{
			if( !node.HasMethods )
				return;

			foreach( var m in node.Members)
									//.Where( m => m.IsPublic && m.IsStatic )
									//.Select( m => m as Method ) )
			{
				var method = m as Method;

				if(method!=null)
				{
					if( signatures.TryGetValue(method.Name, out var sig) )
						sig.Ensure(Context, method);
				}
			}
		}

		//public override void OnMethod(Method node)
		//{
		//	if(signatures.TryGetValue(node.Name, out var sig) )
		//	{
		//		sig.Ensure(Context,node);
		//	}
		//}
	}
}
