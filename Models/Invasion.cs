using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace CustomNpcsEdit.Models
{
	/// <summary>
	///     Represents an invasion definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Invasion //: DefinitionBase, IDisposable
	{
		/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty(Order = 0)]
		public string Name { get; set; } = "example";

		/// <summary>
		///     Gets the script path.
		/// </summary>
		[JsonProperty(Order = 1)]
		public string ScriptPath { get; set; }

		/// <summary>
		///     Gets the NPC point values.
		/// </summary>
		[JsonProperty(Order = 2)]
		public Dictionary<string, int> NpcPointValues { get; set; } = new Dictionary<string, int>();

		/// <summary>
		///     Gets the completed message.
		/// </summary>
		[JsonProperty(Order = 3)]
		public string CompletedMessage { get; set; } = "The example invasion has ended!";

		/// <summary>
		///     Gets a value indicating whether the invasion should occur at spawn only.
		/// </summary>
		[JsonProperty(Order = 4)]
		public bool AtSpawnOnly { get; set; }

		/// <summary>
		///     Gets a value indicating whether the invasion should scale by the number of players.
		/// </summary>
		[JsonProperty(Order = 5)]
		public bool ScaleByPlayers { get; set; }

		/// <summary>
		///     Gets the waves.
		/// </summary>
		[JsonProperty(Order = 6)]
		public List<WaveDefinition> Waves { get; set; } = new List<WaveDefinition>();

		public Invasion()
		{
		}

		public Invasion(Invasion other)
		{
			Name = other.Name;
			ScriptPath = other.ScriptPath;

			//make a copy function
			//NpcPointValues = other.NpcPointValues?.ToDictionary(

			CompletedMessage = other.CompletedMessage;
			AtSpawnOnly = other.AtSpawnOnly;
			ScaleByPlayers = other.ScaleByPlayers;

			//make better copy function
			Waves = other.Waves?.ToList();
		}
	}
}
