﻿using Newtonsoft.Json;
using RpgToolsEditor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms.Design;

namespace RpgToolsEditor.Models.CustomNpcs
{
	/// <summary>
	///     Represents an invasion definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Invasion : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New Invasion";

		/// <summary>
		///     Gets the name.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 0)]
		public string Name
		{
			get => name;
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		/// <summary>
		///     Gets the script path.
		/// </summary>
		[Category("Basic Properties")]
		[Editor(typeof(FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[JsonProperty(Order = 1)]
		public string ScriptPath { get; set; }

		/// <summary>
		///     Gets the NPC point values.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 2)]
		public Dictionary<string, int> NpcPointValues { get; set; } = new Dictionary<string, int>();

		/// <summary>
		///     Gets the completed message.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 3)]
		public string CompletedMessage { get; set; } = "The invasion has ended!";

		/// <summary>
		///     Gets a value indicating whether the invasion should occur at spawn only.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 4)]
		public bool AtSpawnOnly { get; set; }

		/// <summary>
		///     Gets a value indicating whether the invasion should scale by the number of players.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 5)]
		public bool ScaleByPlayers { get; set; }

		/// <summary>
		///     Gets the waves.
		/// </summary>
		[Browsable(false)]
		[Category("Basic Properties")]
		[JsonProperty(Order = 6)]
		public List<Wave> Waves { get; set; } = new List<Wave>();

		public Invasion()
		{
		}

		public Invasion(Invasion other)
		{
			Name = other.Name;
			ScriptPath = other.ScriptPath;

			//make a copy function
			NpcPointValues = new Dictionary<string, int>(other.NpcPointValues);

			CompletedMessage = other.CompletedMessage;
			AtSpawnOnly = other.AtSpawnOnly;
			ScaleByPlayers = other.ScaleByPlayers;

			//make better copy function
			Waves = other.Waves.Select(w => new Wave(w)).ToList();
		}

		object ICloneable.Clone() => new Invasion(this);
	}
}
