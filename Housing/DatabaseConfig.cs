using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Housing
{
	[JsonObject(MemberSerialization.OptIn)]
	public class DatabaseConfig
	{
		[JsonProperty(Order = 0)]
		public string DatabaseType { get; set; } = "sqlite";

		[JsonProperty(Order = 1)]
		public string ConnectionString { get; set; } = $"uri=file://housing/db.sqlite,Version=3";
	}
}
