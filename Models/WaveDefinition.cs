using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CustomNpcsEdit.Models
{ 
	/// <summary>
	///     Represents a wave definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WaveDefinition
	{
		/// <summary>
		///     Gets the NPC weights.
		/// </summary>
		[JsonProperty(Order = 0)]
		public Dictionary<string, int> NpcWeights { get; set; } = new Dictionary<string, int>();

		/// <summary>
		///     Gets the points required to advance.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int PointsRequired { get; set; }

		/// <summary>
		///     Gets the maximum spawns.
		/// </summary>
		[JsonProperty(Order = 2)]
		public int MaxSpawns { get; set; } = 10;

		/// <summary>
		///     Gets the spawn rate.
		/// </summary>
		[JsonProperty(Order = 3)]
		public int SpawnRate { get; set; } = 20;

		/// <summary>
		///     Gets the start message.
		/// </summary>
		[JsonProperty(Order = 4)]
		public string StartMessage { get; set; } = "The wave has started!";

		/// <summary>
		///     Gets the miniboss.
		/// </summary>
		[JsonProperty(Order = 5)]
		public string Miniboss { get; set; }

		public WaveDefinition()
		{
		}

		public WaveDefinition(WaveDefinition other)
		{
			NpcWeights = new Dictionary<string, int>(other.NpcWeights);//we can copy this, values are valuetypes... and strings should work.
			PointsRequired = other.PointsRequired;
			MaxSpawns = other.MaxSpawns;
			SpawnRate = other.SpawnRate;
			StartMessage = other.StartMessage;
			Miniboss = other.Miniboss;
		}

		public override string ToString()
		{
			return $"Wave: \"{StartMessage}\"";
		}
	}
}
