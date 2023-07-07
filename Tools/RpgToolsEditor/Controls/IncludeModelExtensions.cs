using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RpgToolsEditor.Controls
{
	public static class IncludeModelExtensions
	{
		/// <summary>
		/// Used to find duplicate include files, and allow calling code to take actions to guard against overwriting data.
		/// </summary>
		public static IEnumerable<string> FindDuplicateIncludes(this IEnumerable<IncludeModel> includeModels)
		{
			var includedPaths = new HashSet<string>();
			var duplicatedPaths = new HashSet<string>();

			foreach (var incPath in includeModels.Select(im => im.FullPath))
			{
				if (!includedPaths.Contains(incPath))
					includedPaths.Add(incPath);
				else
					duplicatedPaths.Add(incPath);
			}

			return duplicatedPaths;
		}

		/// <summary>
		/// Throw an Exception if duplicate include files are found.
		/// </summary>
		public static void ThrowOnDuplicateIncludes(this IEnumerable<IncludeModel> includeModels)
		{
			var duplicates = includeModels.FindDuplicateIncludes();

			if (duplicates.Count() > 0)
			{
				var sb = new StringBuilder();
				var comma = false;

				foreach (var s in duplicates)
				{
					if (comma)
						sb.AppendLine(",");

					sb.Append(s);

					comma = true;
				}

				throw new Exception($"Duplicate include files found:\n{sb.ToString()}");
			}
		}
	}
}
