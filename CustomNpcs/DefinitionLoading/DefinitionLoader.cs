using Corruption.PluginSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
			var definitionType = typeof(T);
			var typeName = definitionType.Name;

			if( File.Exists(filePath) )
			{
				var definitions = DeserializeFromText<T>(filePath);
				var usedNames = new HashSet<string>();
				var failedDefinitions = new List<T>();

				foreach( var definition in definitions )
				{
					try
					{
						ValidationResult validationResult = null;

						if( usedNames.Contains(definition.Name) )
						{
							//throw new Exception($"A definition with the name '{definition.Name}' already exists.");
							validationResult = new ValidationResult();
							validationResult.Errors.Add(new ValidationError($"A definition with the name '{definition.Name}' already exists.", definition.FilePath, definition.LineNumber, definition.LinePosition));
						}
						else
						{
							//definition.ThrowIfInvalid();
							validationResult = definition.Validate();
						}

						if(validationResult.HasErrors)
						{
							foreach(var err in validationResult.Errors)
								CustomNpcsPlugin.Instance.LogPrint(err.ToString(), TraceLevel.Error);

							failedDefinitions.Add(definition);

							continue;
						}
						else if( validationResult.HasWarnings )
						{
							foreach( var war in validationResult.Warnings )
								CustomNpcsPlugin.Instance.LogPrint(war.ToString(), TraceLevel.Warning);
						}

						usedNames.Add(definition.Name);
					}
					catch( Exception ex )
					{
						CustomNpcsPlugin.Instance.LogPrint($"{definition.FilePath}: An error occurred while trying to load {typeName} '{definition.Name}': {ex.Message}", TraceLevel.Error);
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

		static List<T> DeserializeFromText<T>(string filePath) where T : DefinitionBase
		{
			var expandedDefinitions = new List<T>();
			
			if( File.Exists(filePath) )
			{
				var json = File.ReadAllText(filePath);
				var definitionType = typeof(T);
				var rawDefinitions = (List<DefinitionBase>)JsonConvert.DeserializeObject(json,
																						typeof(List<DefinitionBase>),
																						new DefinitionOrCategoryJsonConverter(definitionType));
				foreach( var rawDef in rawDefinitions )
				{
					if( rawDef is T )
					{
						//this is a real definition
						rawDef.FilePath = filePath;
						expandedDefinitions.Add(rawDef as T);
					}
					else if( rawDef is CategoryPlaceholderDefinition )
					{
						//this is a placeholder definition, which points to included definitions.
						var placeholder = rawDef as CategoryPlaceholderDefinition;
						var includedDefinitions = placeholder.TryLoadIncludes<T>(filePath);

						expandedDefinitions.AddRange(includedDefinitions);
					}
					//else
					//{
					//	//throw?
					//}
				}
			}

			return expandedDefinitions;
		}
	}
}
