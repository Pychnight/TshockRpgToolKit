using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BooTS
{
	/// <summary>
	/// Ties source file information to a Boo generated, dynamic Assembly. 
	/// </summary>
	public class BooScriptAssembly
	{
		internal HashSet<SourceFile> SourceFiles { get; set; } //really need a safe public means for getting at source files. Working with the hashset directly
															   //has potential for inconsistent state from SourceFilesChanged.

		/// <summary>
		/// Gets the time of the last successful build.
		/// </summary>
		public DateTime BuildTime { get; protected set; }

		/// <summary>
		/// Gets the generated Assembly, if one was built.
		/// </summary>
		public Assembly Assembly { get; protected set; }

		/// <summary>
		/// Gets if this BooScriptAssembly has been built, and is runnable.
		/// </summary>
		public bool IsBuilt => Assembly != null;

		/// <summary>
		/// Gets whether the set of source code files has changed.
		/// </summary>
		public bool SourceFilesChanged { get; private set; }

		public BooScriptAssembly(IEnumerable<string> fileNames)
		{
			var sourceFiles = fileNames.Select(f => new SourceFile(f));
			SourceFiles = new HashSet<SourceFile>(sourceFiles);
			SourceFilesChanged = true;
		}

		/// <summary>
		/// Sets or recreate the entire list of input files.
		/// </summary>
		/// <param name="fileNames"></param>
		public void SetSourceFiles(params string[] fileNames) => SetSourceFiles(fileNames as IEnumerable<string>);

		/// <summary>
		/// Sets or recreate the entire list of input files.
		/// </summary>
		/// <param name="fileNames"></param>
		public void SetSourceFiles(IEnumerable<string> fileNames)
		{
			SourceFiles.Clear();
			AddSourceFiles(fileNames);
		}

		/// <summary>
		/// Adds new files to the current list of input files.
		/// </summary>
		/// <param name="fileNames"></param>
		public void AddSourceFiles(IEnumerable<string> fileNames)
		{
			var sourceFiles = fileNames.Select(f => new SourceFile(f));

			foreach (var sf in sourceFiles)
				SourceFiles.Add(sf);

			SourceFilesChanged = true;
		}

		/// <summary>
		/// Removes files from the current list of input files.
		/// </summary>
		/// <param name="fileNames"></param>
		public void RemoveSourceFiles(IEnumerable<string> fileNames)
		{
			var sourceFiles = fileNames.Select(f => new SourceFile(f));

			foreach (var sf in sourceFiles)
				SourceFiles.Remove(sf);

			SourceFilesChanged = true;
		}

		/// <summary>
		/// Tests whether this BooScriptAssembly is up to date with respect to its sources, or should be rebuilt.
		/// </summary>
		/// <returns>True if up to date, false otherwise.</returns>
		public bool IsUpToDate()
		{
			if (!IsBuilt || SourceFilesChanged)
				return false;

			foreach (var sf in SourceFiles)
			{
				if (!sf.Exists)
					return false;

				if (sf.LastUpdated > BuildTime)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Checks that this BooScriptAssembly is up to date, and if not, invokes a Func to compile it.
		/// </summary>
		/// <param name="buildFunc">Func to compile.</param>
		/// <param name="context">A Boo CompilerContext.</param>
		/// <returns>True if a build happened, false otherwise.</returns>
		public bool TryRebuild(Func<BooScriptAssembly, CompilerContext> buildFunc, out CompilerContext context)
		{
			if (IsUpToDate())
			{
				context = null;
				return false;
			}
			else
				return TryBuild(buildFunc, out context);
		}

		/// <summary>
		/// Attempts to build the Assembly, regardless of whether it is up to date. This will use the passed in Func to compile.
		/// </summary>
		/// <param name="buildFunc">Func to compile.</param>
		/// <param name="context">A Boo CompilerContext.</param>
		/// <returns>True if a build happened, false otherwise.</returns>
		public bool TryBuild(Func<BooScriptAssembly, CompilerContext> buildFunc, out CompilerContext context)
		{
			if (buildFunc == null)
				throw new ArgumentNullException("buildFunc cannot be null.");

			context = buildFunc(this);

			if (context != null)
			{
				Assembly = context.GeneratedAssembly;
				BuildTime = DateTime.Now;
				SourceFilesChanged = false;
				return true;
			}

			return false;
		}
	}
}