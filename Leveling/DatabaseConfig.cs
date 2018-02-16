using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leveling
{
	[JsonObject(MemberSerialization.OptIn)]
	public class DatabaseConfig
	{
		[JsonProperty(Order = 0)]
		public string DatabaseType { get; set; } = "sqlite";

		[JsonProperty(Order = 1)]
		public string ConnectionString { get; set; } = $"uri=file://leveling\\db.sqlite,Version=3";
	}
}
