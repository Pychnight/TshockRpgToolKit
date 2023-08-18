using Corruption.PluginSupport;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CustomNpcs.Invasions
{
	/// <summary>
	///     Represents a wave definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WaveDefinition : IValidator
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

		public ValidationResult Validate()
		{
			var result = new ValidationResult(this);

			if (NpcWeights == null)
				result.Errors.Add(new ValidationError($"{nameof(NpcWeights)} is null."));

			if (NpcWeights.Count == 0)
				result.Errors.Add(new ValidationError($"{nameof(NpcWeights)} must not be empty."));

			if (NpcWeights.Any(kvp => kvp.Value <= 0))
				result.Errors.Add(new ValidationError($"{nameof(NpcWeights)} must contain positive weights."));

			if (PointsRequired <= 0)
				result.Errors.Add(new ValidationError($"{nameof(PointsRequired)} must be positive."));

			if (MaxSpawns <= 0)
				result.Errors.Add(new ValidationError($"{nameof(MaxSpawns)} must be positive."));

			if (SpawnRate <= 0)
				result.Errors.Add(new ValidationError($"{nameof(SpawnRate)} must be positive."));

			if (StartMessage == null)
				result.Errors.Add(new ValidationError($"{nameof(StartMessage)} is null."));

			return result;
		}
	}
}
