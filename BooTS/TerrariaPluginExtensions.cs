using Boo.Lang.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;

namespace BooTS
{
	public static class TerrariaPluginExtensions
	{
		private static void logPrint(TerrariaPlugin plugin, string message, TraceLevel level)
		{
			ServerApi.LogWriter.PluginWriteLine(plugin, message, level);
		}

		public static void LogPrintBooErrors(this TerrariaPlugin plugin, CompilerContext context )
		{
			foreach( var err in context.Errors )
			{
				logPrint(plugin,$"{err.LexicalInfo.FileName} {err.LexicalInfo.Line},{err.LexicalInfo.Column}: {err.Message}", TraceLevel.Error);
			}
		}

		public static void LogPrintBooWarnings(this TerrariaPlugin plugin, CompilerContext context)
		{
			foreach( var warn in context.Warnings )
			{
				logPrint(plugin,$"{warn.LexicalInfo.FileName} {warn.LexicalInfo.Line},{warn.LexicalInfo.Column}: {warn.Message}", TraceLevel.Warning);
			}
		}
	}
}
