using Corruption.PluginSupport;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Leveling.Levels
{
	/// <summary>
	///     Represents a level definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class LevelDefinition : IValidator
	{
		/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty("Name", Order = 0)]
		public string Name { get; internal set; }

		/// <summary>
		///     Gets the display name.
		/// </summary>
		[JsonProperty("DisplayName", Order = 1)]
		public string DisplayName { get; internal set; }

		/// <summary>
		///     Gets the EXP required to level up.
		/// </summary>
		[JsonProperty("ExpRequired", Order = 2)]
		public long ExpRequired { get; internal set; }

		/// <summary>
		/// Gets or sets a Currency string that will be parsed into ExpRequired.
		/// </summary>
		[JsonProperty("CurrencyRequired", Order = 3)]
		public string CurrencyRequired { get; set; }

		/// <summary>
		///     Gets the prefix for the level.
		/// </summary>
		[JsonProperty("Prefix", Order = 4)]
		public string Prefix { get; internal set; } = "";

		/// <summary>
		///     Gets the set of item names allowed.
		/// </summary>
		[JsonProperty("ItemsAllowed", Order = 5)]
		public ISet<string> ItemNamesAllowed { get; internal set; } = new HashSet<string>();

		/// <summary>
		///     Gets the set of permissions granted.
		/// </summary>
		[JsonProperty("PermissionsGranted", Order = 6)]
		public ISet<string> PermissionsGranted { get; internal set; } = new HashSet<string>();

		/// <summary>
		///     Gets the list of commands to execute on leveling up.
		/// </summary>
		[JsonProperty("CommandsOnLevelUp", Order = 7)]
		public IList<string> CommandsOnLevelUp { get; internal set; } = new List<string>();

		/// <summary>
		///     Gets the list of commands to execute on leveling up, but only once.
		/// </summary>
		[JsonProperty("CommandsOnLevelUpOnce", Order = 8)]
		public IList<string> CommandsOnLevelUpOnce { get; internal set; } = new List<string>();

		/// <summary>
		///     Gets the list of commands to execute on leveling down.
		/// </summary>
		[JsonProperty("CommandsOnLevelDown", Order = 9)]
		public IList<string> CommandsOnLevelDown { get; internal set; } = new List<string>();

		public ValidationResult Validate()
		{
			string source = Name;

			if (string.IsNullOrWhiteSpace(Name))
				source = DisplayName;

			var result = new ValidationResult(source);

			if (string.IsNullOrWhiteSpace(Name))
				result.Errors.Add(new ValidationError($"{nameof(Name)} is null or whitespace."));

			if (string.IsNullOrWhiteSpace(DisplayName))
				result.Warnings.Add(new ValidationWarning($"{nameof(DisplayName)} is null or whitespace."));

			if (ExpRequired < 1)
				result.Errors.Add(new ValidationError($"{nameof(ExpRequired)} is less than 1."));

			return result;
		}
	}
}
