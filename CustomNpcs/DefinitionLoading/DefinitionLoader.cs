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
		internal static List<TDefinition> LoadFromFile<TDefinition>(string filePath) where TDefinition : DefinitionBase
		{
			List<TDefinition> result = null;
			var definitionType = typeof(TDefinition);
			var typeName = definitionType.Name;

			if( File.Exists(filePath) )
			{
				var definitions = DeserializeFile<TDefinition>(filePath);
				var usedNames = new HashSet<string>();
				var failedDefinitions = new List<TDefinition>();

				foreach( var definition in definitions )
				{
					try
					{
						ValidationResult validationResult = null;

						if( usedNames.Contains(definition.Name) )
						{
							//throw new Exception($"A definition with the name '{definition.Name}' already exists.");
							validationResult = new ValidationResult();
							validationResult.Errors.Add(new ValidationError($"A definition with the name '{definition.Name}' already exists."));
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
						//CustomNpcsPlugin.Instance.LogPrint($"{definition.FilePath}: An error occurred while trying to load {typeName} '{definition.Name}': {ex.Message}", TraceLevel.Error);
						CustomNpcsPlugin.Instance.LogPrint($"{definition.FilePosition.FilePath}: An error occurred while trying to load {typeName} '{definition.Name}': {ex.Message}", TraceLevel.Error);
						failedDefinitions.Add(definition);
					}
				}

				result = definitions.Except(failedDefinitions).ToList();
			}
			else
			{
				CustomNpcsPlugin.Instance.LogPrint($"Configuration for {typeName} does not exist. Expected config file to be at: {filePath}", TraceLevel.Error);
				result = new List<TDefinition>();
			}
			
			return result;
		}

		static List<TDefinition> DeserializeFile<TDefinition>(string filePath) where TDefinition : DefinitionBase
		{
			var expandedDefinitions = new List<TDefinition>();
			
			if( File.Exists(filePath) )
			{
				var json = File.ReadAllText(filePath);
				var definitionType = typeof(TDefinition);
				var rawDefinitions = (List<DefinitionBase>)JsonConvert.DeserializeObject(json,
																						typeof(List<DefinitionBase>),
																						new DefinitionOrCategoryJsonConverter(definitionType));

				var settings = new JsonSerializerSettings();
				
				//settings.Error = (s, a) =>
				//{
				//	Debug.Print($"JsonError! CurrentObject: {a.CurrentObject}");
				//	Debug.Print($"JsonError! ErrorContext.Path: {a.ErrorContext.Path}");
				//};

				//settings.Converters.Add(new DefinitionOrCategoryJsonConverter(definitionType));

				//var rawDefinitions = (List<DefinitionBase>)JsonConvert.DeserializeObject(json, typeof(List<DefinitionBase>), settings);

				foreach ( var rawDef in rawDefinitions )
				{
					if( rawDef is TDefinition )
					{
						//this is a real definition
						rawDef.FilePosition = new FilePosition(filePath);
						expandedDefinitions.Add(rawDef as TDefinition);
					}
					else if( rawDef is CategoryDefinition )
					{
						//this is a placeholder definition, which points to included definitions.
						var placeholder = rawDef as CategoryDefinition;
						var includedDefinitions = placeholder.TryLoadIncludes<TDefinition>(filePath);

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
