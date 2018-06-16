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
using System.Windows.Forms.Design;
using System.ComponentModel;

namespace CustomNpcsEdit.Models
{
	/// <summary>
	///     Represents an invasion definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Invasion : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		
		string name = "New Invasion";

		/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty(Order = 0)]
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		/// <summary>
		///     Gets the script path.
		/// </summary>
		[JsonProperty(Order = 1)]
		[Editor(typeof(FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
		public string CompletedMessage { get; set; } = "The invasion has ended!";

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
			NpcPointValues = new Dictionary<string,int>(other.NpcPointValues);

			CompletedMessage = other.CompletedMessage;
			AtSpawnOnly = other.AtSpawnOnly;
			ScaleByPlayers = other.ScaleByPlayers;

			//make better copy function
			Waves = other.Waves.Select(w => new WaveDefinition(w)).ToList();
		}
	}
}
