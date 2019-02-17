using Boo.Lang.Compiler.Ast;
using BooTS;
using Corruption.PluginSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Scripting
{
	internal class ScriptAssemblyManager
	{
		Dictionary<string, ScriptAssemblyHolder> scriptAssemblies { get; set; }

		internal ScriptAssemblyManager()
		{
			scriptAssemblies = new Dictionary<string, ScriptAssemblyHolder>();
		}

		internal void Clear()
		{
			scriptAssemblies.Clear();
		}

		internal Assembly GetOrCompile(string scriptPath)
		{
			if(scriptAssemblies.TryGetValue(scriptPath,out var holder))
			{
				//if(holder.Assembly==null)
				//{
				//	if(holder.shouldRecompile(scriptPath))
				//	{
				//		Debug.Print($"Recompiling {scriptPath}.");
				//		holder = Compile(scriptPath);
				//		scriptAssemblies[scriptPath] = holder;
				//	}
				//}

				return holder.Assembly;
			}

			holder = Compile(scriptPath);
			scriptAssemblies.Add(scriptPath, holder);
			
			return holder.Assembly;
		}

		private ScriptAssemblyHolder Compile(string scriptPath)
		{
			var bc = new BooScriptCompiler();

			bc.Configure(ScriptHelpers.GetReferences(), ScriptHelpers.GetDefaultImports());

			var name = Path.GetFileNameWithoutExtension(scriptPath);

			var convertStep = new ModuleToInstanceClassStep();
			convertStep.SourceModuleName = name;
			convertStep.TargetClassName = $"{name}Quest";
			convertStep.TargetBaseClassName = "CustomQuests.Quests.Quest";
			convertStep.TargetMethodName = "OnRun";
			convertStep.TargetMethodModifiers = TypeMemberModifiers.Protected | TypeMemberModifiers.Override;

			var pipe = bc.InternalCompiler.Parameters.Pipeline;
			pipe.InsertAfter(typeof(InjectImportsStep), convertStep);

			var assName = $"Quest_{name}.dll";
			var holder = new ScriptAssemblyHolder(DateTime.Now);
			var context = bc.Compile(assName, new string[] { scriptPath });
									
			CustomQuestsPlugin.Instance.LogPrintBooErrors(context);
			CustomQuestsPlugin.Instance.LogPrintBooWarnings(context);

			if( context.Errors.Count == 0 )
			{
				holder.Assembly = context.GeneratedAssembly;
				CustomQuestsPlugin.Instance.LogPrint($"Compiled quest {name}", TraceLevel.Info);
			}
			else
			{
				CustomQuestsPlugin.Instance.LogPrint($"Failed to compile {name}.", TraceLevel.Error);
			}

			return holder;
		}
		
		class ScriptAssemblyHolder
		{
			internal DateTime LastCompileTime { get; private set; }
			internal Assembly Assembly { get; set; }

			internal ScriptAssemblyHolder(DateTime lastCompileTime)
			{
				LastCompileTime = lastCompileTime;
				Assembly = null;
			}

			internal bool shouldRecompile(string scriptPath)
			{
				if( File.Exists(scriptPath) && File.GetLastWriteTime(scriptPath) > LastCompileTime )
					return true;

				return false;
			}
		}
	}
}
