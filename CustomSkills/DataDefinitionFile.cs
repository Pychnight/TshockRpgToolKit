using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomSkills
{
	//TODO: Would be nice to unify *ALL* json data under this layout...
	/// <summary>
	///	Structured json data format that includes version and metadata properties. 
	/// </summary>
	/// <typeparam name="TData">Custom data payload.</typeparam>
	[JsonObject(MemberSerialization.OptIn)]
	public class DataDefinitionFile<TData> where TData : new()
	{
		/// <summary>
		/// Gets or sets a version number that can be used for compatibility checks.
		/// </summary>
		[JsonProperty(Order = 0)]
		public float Version { get; set; }

		/// <summary>
		/// Gets or sets optional an object with user defined data.
		/// </summary>
		[JsonProperty(Order = 1)]
		public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

		/// <summary>
		/// Gets or sets the data contained within the file.
		/// </summary>
		[JsonProperty(Order = 2)]
		public TData Data { get; set; } = new TData();
	}
}
