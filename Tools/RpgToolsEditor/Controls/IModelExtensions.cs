using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RpgToolsEditor.Controls
{
	public static class IModelExtensions
	{
		public static void TryAddCopySuffix(this IModel model)
		{
			const string suffix = "(Copy)";

			if (!model.Name.EndsWith(suffix))
			{
				model.Name = model.Name + suffix;
			}
		}

		/// <summary>
		/// Used to find duplicate names, and allow calling code to take actions to guard against overwriting data.
		/// </summary>
		public static IEnumerable<string> FindDuplicateNames(this IEnumerable<IModel> imodels)
		{
			var includedNames = new HashSet<string>();
			var duplicatedNames = new HashSet<string>();

			foreach (var name in imodels.Select(im => im.Name))
			{
				if (!includedNames.Contains(name))
					includedNames.Add(name);
				else
					duplicatedNames.Add(name);
			}

			return duplicatedNames;
		}

		/// <summary>
		/// Throw an Exception if duplicate names are found.
		/// </summary>
		public static void ThrowOnDuplicateNames(this IEnumerable<IModel> imodels)
		{
			var duplicates = imodels.FindDuplicateNames();

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

				throw new Exception($"Duplicate Names found:\n{sb.ToString()}");
			}
		}
	}
}
