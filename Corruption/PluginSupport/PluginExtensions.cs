using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;

namespace Corruption.PluginSupport
{
	public static class TerrariaPluginExtensions
	{
		/// <summary>
		/// Wrapper around <see cref="ServerApi.LogWriter.PluginWriteLine"/>
		/// </summary>
		/// <param name="plugin"></param>
		/// <param name="message"></param>
		/// <param name="kind"></param>
		public static void LogPrint(this TerrariaPlugin plugin, string message, TraceLevel kind = TraceLevel.Info )
		{
			//try safeguard against occasional NREs thrown on abrupt shutdown.
			if( plugin == null )
				return;

			ServerApi.LogWriter?.PluginWriteLine(plugin, message, kind);
		}

		/// <summary>
		/// Convienance method for logging and printing errors and warnings found in a <see cref="ValidationResult" />.
		/// </summary>
		/// <param name="plugin"></param>
		/// <param name="validationResult"></param>
		public static void LogPrint(this TerrariaPlugin plugin, ValidationResult validationResult)
		{
			var traceLevel = TraceLevel.Info;
			var endPart = ".";
			int warnings = 0;
			int errors = 0;

			validationResult.GetTotals(ref errors, ref warnings);

			if (warnings == 0 && errors == 0)
				return;
						
			if (warnings>0)
				traceLevel = TraceLevel.Warning;

			if (errors>0)
				traceLevel = TraceLevel.Error;

			if(validationResult.Source!=null)
				endPart = $" in {validationResult.Source.ToString()}.";
						
			plugin.LogPrint($"Found {errors} Errors, {warnings} Warnings{endPart}", traceLevel);

			RecurseLogPrintValidationResult(plugin, validationResult);
		}

		private static void RecurseLogPrintValidationResult(TerrariaPlugin plugin, ValidationResult validationResult)
		{
			foreach (var err in validationResult.Errors)
				plugin.LogPrint(err.ToString(), TraceLevel.Error);
			
			foreach (var warn in validationResult.Warnings)
				plugin.LogPrint(warn.ToString(), TraceLevel.Warning);
			
			foreach (var ch in validationResult.Children)
				RecurseLogPrintValidationResult(plugin, ch);
		}
	}
}
