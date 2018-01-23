using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcs
{
	internal static class DefinitionLoader
	{
		internal static List<T> LoadFromFile<T>(string filePath) where T : DefinitionBase
		{
			List<T> result = null;
			var typeName = typeof(T).Name;

			if( File.Exists(filePath) )
			{
				var definitions = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath));
				var failedDefinitions = new List<T>();

				foreach( var definition in definitions )
				{
					try
					{
						definition.ThrowIfInvalid();
					}
					catch( FormatException ex )
					{
						CustomNpcsPlugin.Instance.LogPrint($"An error occurred while parsing {typeName} '{definition.Name}': {ex.Message}", TraceLevel.Error);
						failedDefinitions.Add(definition);
					}
					catch( Exception ex )
					{
						CustomNpcsPlugin.Instance.LogPrint($"An error occurred while trying to load {typeName} '{definition.Name}': {ex.Message}", TraceLevel.Error);
						failedDefinitions.Add(definition);
					}
				}

				result = definitions.Except(failedDefinitions).ToList();
			}
			else
			{
				CustomNpcsPlugin.Instance.LogPrint($"Configuration for {typeName} does not exist. Expected config file to be at: {filePath}", TraceLevel.Error);
				result = new List<T>();
			}
			
			return result;
		}
	}
}
