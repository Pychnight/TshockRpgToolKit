using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Corruption.PluginSupport
{
	public static class CommandArgsExtensions
	{
		/// <summary>
		/// Attempts to get the parameter string at index, but if index is invalid, return a default string in its place. This string defaults to null.
		/// </summary>
		/// <param name="args">CommandArgs instance.</param>
		/// <param name="index">Index into the Parameters list.</param>
		/// <param name="defaultValue">An optional safe string to use, if index is out of range.</param>
		/// <returns>Parameter string, or default value if index is out of range.</returns>
		public static string GetSafeParam(this CommandArgs args, int index, string defaultValue = null)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			if (index >= args.Parameters.Count)
				return defaultValue;

			return args.Parameters[index];
		}
	}
}
