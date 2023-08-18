using Corruption.PluginSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CustomNpcs
{
	public class DefinitionInclude : List<DefinitionBase>, IValidator
	{
		public string FilePath { get; private set; }

		public static DefinitionInclude Load<TDefinition>(string filePath) where TDefinition : DefinitionBase
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException($"Unable to find file '{filePath}'.", filePath);

			var result = new DefinitionInclude();
			result.FilePath = filePath;
			var definitionType = typeof(TDefinition);
			var typeName = definitionType.Name;

			var json = File.ReadAllText(filePath);
			var array = JArray.Parse(json);
			var children = array.Children().Where(jt => jt.Type == JTokenType.Object);

			DefinitionBase baseDef = null;

			foreach (var child in children)
			{
				if (child["Category"] != null)
				{
					var category = child.ToObject<CategoryDefinition>();
					//do category specific things...
					baseDef = category;
				}
				else
				{
					var def = child.ToObject<TDefinition>();
					//do TDefinition specific things...
					baseDef = def;
				}

				//set file and line info
				var filePos = baseDef.FilePosition = new FilePosition(filePath);
				var lineInfo = child as IJsonLineInfo;

				if (lineInfo?.HasLineInfo() == true)
				{
					filePos.Line = lineInfo.LineNumber;
					filePos.Column = lineInfo.LinePosition;

					//Debug.Print($"{filePath} [{baseDef.LineNumber},{baseDef.LinePosition}] {baseDef.Name}");
					Debug.Print($"{filePos} {baseDef.Name}");
				}

				result.Add(baseDef);
			}

			//load includes
			//Debug.Print("Categories are currently disabled.");
			var categories = result.OfType<CategoryDefinition>();

			foreach (var category in categories)
			{
				foreach (var inc in category.Includes)
				{
					var relativeIncludePath = Path.Combine(Path.GetDirectoryName(filePath), inc);
					var defInclude = DefinitionInclude.Load<TDefinition>(relativeIncludePath);
					category.DefinitionIncludes.Add(inc, defInclude);
				}
			}

			return result;
		}

		/// <summary>
		/// Creates a list of TDefinitions, loading defintitions found in Categories and Includes. 
		/// </summary>
		/// <typeparam name="TDefinition"></typeparam>
		/// <param name="sourceDefinitions"></param>
		/// <returns></returns>
		public static List<TDefinition> Flatten<TDefinition>(List<DefinitionBase> sourceDefinitions) where TDefinition : DefinitionBase
		{
			var result = new List<TDefinition>(sourceDefinitions.Count);

			foreach (var baseDef in sourceDefinitions)
			{
				var def = baseDef as TDefinition;

				if (def != null)
				{
					result.Add(def);
				}
				else if (baseDef is CategoryDefinition)
				{
					var category = (CategoryDefinition)baseDef;

					foreach (var kvp in category.DefinitionIncludes)
						result.AddRange(DefinitionInclude.Flatten<TDefinition>(kvp.Value));
				}
			}

			return result;
		}

		public ValidationResult Validate()
		{
			var result = new ValidationResult();

			foreach (var def in this)
			{
				var childResult = def.Validate();

				result.Children.Add(childResult);
			}

			return result;
		}
	}
}
