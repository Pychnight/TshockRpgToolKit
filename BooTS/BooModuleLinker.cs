using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BooTS
{
	/// <summary>
	/// Provides a simple, efficient means for finding and linking to Boo module methods in a compiled Assembly. 
	/// </summary>
	public class BooModuleLinker
	{
		Type type;
		Dictionary<string, MethodInfo> methods;

		public string FilePath { get; private set; }
		public string ModuleName => type.Name;
		public int MethodCount => methods.Count;

		public MethodInfo this[string methodName]
		{
			get
			{
				methods.TryGetValue(methodName, out var result);
				return result;
			}
		}

		/// <summary>
		/// Creates a BooModuleLinker for an assembly created from the given script file path.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="filePath"></param>
		/// <param name="bindingFlags"></param>
		public BooModuleLinker(Assembly assembly, string filePath, BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public)
		{
			FilePath = filePath;
			var moduleName = GetModuleNameForFilePath(filePath);

			type = assembly.GetType(moduleName, false, true);//ignore case.

			if (type != null)
			{
				var methods = type.GetMethods(bindingFlags);

				this.methods = new Dictionary<string, MethodInfo>(methods.Length);

				foreach (var m in methods)
					this.methods.Add(m.Name, m);
			}
			else
				this.methods = new Dictionary<string, MethodInfo>();
		}

		/// <summary>
		/// Constructs a Boo module name for the given file path.
		/// </summary>
		/// <param name="filePath">File path of module.</param>
		/// <returns>The compiled module name.</returns>
		public static string GetModuleNameForFilePath(string filePath)
		{
			var fileName = Path.GetFileNameWithoutExtension(filePath);

			fileName = fileName.Replace(' ', '_');

			var moduleName = $"{fileName}Module";

			return moduleName;
		}

		/// <summary>
		/// Attempts to find and creates a delegate for the named static method.
		/// </summary>
		/// <typeparam name="T">Delegate type</typeparam>
		/// <param name="methodName">Name of method.</param>
		/// <returns>Delegate instance if successful, null if not.</returns>
		public T TryCreateDelegate<T>(string methodName) where T : class
		{
			T result = null;

			try
			{
				result = this[methodName]?.CreateDelegate(typeof(T)) as T;
			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message);
			}

			return result;
		}
	}
}
