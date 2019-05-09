using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corruption.PluginSupport
{
	//TODO: Would be nice to unify *ALL* json data under this layout...
	/// <summary>
	///	Structured json format that includes version and metadata properties, along with a data payload. 
	/// </summary>
	/// <typeparam name="TData">Custom data payload.</typeparam>
	[JsonObject(MemberSerialization.OptIn)]
	public class DefinitionFile<TData> where TData : new()
	{
		/// <summary>
		/// Gets or sets a version number that can be used for compatibility checks.
		/// </summary>
		[JsonProperty(Order = 0)]
		public float Version { get; set; }

		/// <summary>
		/// Gets or sets an optional object with user defined data.
		/// </summary>
		[JsonProperty(Order = 1)]
		public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

		/// <summary>
		/// Gets or sets the data payload contained within the file.
		/// </summary>
		[JsonProperty(Order = 2)]
		public TData Data { get; set; } = new TData();
	}
}
