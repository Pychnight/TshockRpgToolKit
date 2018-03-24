using Corruption.PluginSupport;
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
	//HACK - we create a "dummy" Definition, solely for the purpose of holding category and other definition include information( within our initial, "raw" def list )
	//later this will get all get transformed into a single list of deserialized definitions
	[JsonObject(MemberSerialization.OptIn)]
	public class CategoryPlaceholderDefinition : DefinitionBase
	{
		[JsonIgnore]//...in case later down the road, DefinitionBase opts in on this property!
		public override string Name { get => throw new NotImplementedException(); protected internal set => throw new NotImplementedException(); }

		[JsonIgnore]
		public override string ScriptPath { get => throw new NotImplementedException(); protected internal set => throw new NotImplementedException(); }
			
		[JsonProperty(Order = 0)]
		public string Category { get; set; }

		[JsonProperty(Order = 1)]
		public List<string> Includes { get; set; }

		//protected internal override void ThrowIfInvalid()
		//{
		//	throw new NotImplementedException();
		//}
		
		internal List<T> TryLoadIncludes<T>(string parentFilePath) where T : DefinitionBase
		{
			var result = new List<T>();
			var basePath = Path.GetDirectoryName(parentFilePath);

			foreach( var includeName in Includes )
			{
				var includePath = Path.Combine(basePath, includeName);

				try
				{
					var json = File.ReadAllText(includePath);
					var definitions = JsonConvert.DeserializeObject<List<T>>(json);

					foreach( var def in definitions )
						def.FilePath = includePath;

					result.AddRange(definitions);
				}
				catch( Exception ex )
				{
					CustomNpcsPlugin.Instance.LogPrint($"An error occurred while loading definition file '{includePath}': {ex.Message}", TraceLevel.Error);
				}
			}

			return result;
		}
	}
}
