using Corruption.PluginSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CustomNpcs
{
	//HACK - we create a "dummy" Definition, solely for the purpose of holding category and other definition include information( within our initial, "raw" def list )
	//later this will get all get transformed into a single list of deserialized definitions
	/// <summary>
	/// Pseudo definition for Categories and Includes. This type is only used for deserialization of Categories, and never backs a CustomType. 
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class CategoryDefinition : DefinitionBase
	{
		[JsonIgnore]//...in case later down the road, DefinitionBase opts in on this property!
		public override string Name { get => Category; set => Category = value; }

		[JsonIgnore]
		public override string ScriptPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		[JsonProperty(Order = 0)]
		public string Category { get; set; }

		[JsonProperty(Order = 1)]
		public List<string> Includes { get; set; }

		[JsonIgnore]
		public Dictionary<string, DefinitionInclude> DefinitionIncludes { get; set; } = new Dictionary<string, DefinitionInclude>();

		internal List<TDefinition> TryLoadIncludes<TDefinition>(string parentFilePath) where TDefinition : DefinitionBase
		{
			var result = new List<TDefinition>();
			var basePath = Path.GetDirectoryName(parentFilePath);

			foreach (var includeName in Includes)
			{
				var includePath = Path.Combine(basePath, includeName);

				try
				{
					var json = File.ReadAllText(includePath);
					var definitions = JsonConvert.DeserializeObject<List<TDefinition>>(json);

					foreach (var def in definitions)
						def.FilePosition = new FilePosition(includePath);

					result.AddRange(definitions);
				}
				catch (Exception ex)
				{
					CustomNpcsPlugin.Instance.LogPrint($"An error occurred while loading definition file '{includePath}': {ex.Message}", TraceLevel.Error);
				}
			}

			return result;
		}
	}
}
