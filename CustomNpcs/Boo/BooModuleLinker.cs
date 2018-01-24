using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace CustomNpcs
{
	/// <summary>
	/// Provides a simple, efficient means for finding and linking to Boo module methods in a compiled Assembly. 
	/// </summary>
	public class BooModuleLinker
	{
		Type type;
		Dictionary<string, MethodInfo> methods;

		public string FilePath { get; private set; }
		public string ModuleName {  get { return type.Name; } }
		public int MethodCount {  get { return methods.Count;  } }

		public MethodInfo this[string methodName]
		{
			get
			{
				methods.TryGetValue(methodName, out var result);
				return result;
			}
		}

		public BooModuleLinker(Assembly assembly, string filePath )
		{
			FilePath = filePath;
			var moduleName = getModuleNameFromFilePath(filePath);

			type = assembly.GetType(moduleName, false, true);//ignore case.

			if(type!=null)
			{
				var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

				this.methods = new Dictionary<string, MethodInfo>(methods.Length);

				foreach( var m in methods )
					this.methods.Add(m.Name, m);
			}
			else
				this.methods = new Dictionary<string, MethodInfo>();
		}

		private string getModuleNameFromFilePath(string filePath)
		{
			var fileName = Path.GetFileNameWithoutExtension(filePath);
			var moduleName = $"{fileName}Module";

			return moduleName;
		}

		public T TryCreateDelegate<T>(string methodName) where T : class
		{
			T result = null;

			try
			{
				result = this[methodName]?.CreateDelegate(typeof(T)) as T;
			}
			catch( Exception ex )
			{
				Debug.Print(ex.Message);
			}

			return result;
		}
	}
}
