using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.CustomNpcs
{ 
	/// <summary>
	///     Represents a wave definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Wave : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New Wave";

		//[Browsable(false)]
		[Description("This is a design time aid, and not currently read or saved by the CustomNpcs plugin.")]
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
		///     Gets the NPC weights.
		/// </summary>
		[JsonProperty(Order = 1)]
		public Dictionary<string, int> NpcWeights { get; set; } = new Dictionary<string, int>();

		/// <summary>
		///     Gets the points required to advance.
		/// </summary>
		[JsonProperty(Order = 2)]
		public int PointsRequired { get; set; }

		/// <summary>
		///     Gets the maximum spawns.
		/// </summary>
		[JsonProperty(Order = 3)]
		public int MaxSpawns { get; set; } = 10;

		/// <summary>
		///     Gets the spawn rate.
		/// </summary>
		[JsonProperty(Order = 4)]
		public int SpawnRate { get; set; } = 20;

		/// <summary>
		///     Gets the start message.
		/// </summary>
		[JsonProperty(Order = 5)]
		public string StartMessage { get; set; } = "The wave has started!";

		/// <summary>
		///     Gets the miniboss.
		/// </summary>
		[JsonProperty(Order = 6)]
		public string Miniboss { get; set; }
		
		public Wave()
		{
		}

		public Wave(Wave other)
		{
			NpcWeights = new Dictionary<string, int>(other.NpcWeights);//we can copy this, values are valuetypes... and strings should work.
			PointsRequired = other.PointsRequired;
			MaxSpawns = other.MaxSpawns;
			SpawnRate = other.SpawnRate;
			StartMessage = other.StartMessage;
			Miniboss = other.Miniboss;

			Name = other.Name;
		}

		public override string ToString()
		{
			//return $"Wave: \"{StartMessage}\"";
			return Name;
		}

		public object Clone()
		{
			return new Wave(this);
		}
	}
}
