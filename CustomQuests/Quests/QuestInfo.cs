using Corruption.PluginSupport;
using System;
using TShockAPI;

namespace CustomQuests.Quests
{
	/// <summary>
	///     Represents information about a quest.
	/// </summary>
	public sealed class QuestInfo : IValidator
	{
		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		public string Name { get; set; } = "New Quest";

		/// <summary>
		///     Gets or sets the friendly name.
		/// </summary>
		public string FriendlyName { get; set; }

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		///     Gets or sets the script path.
		/// </summary>
		public string ScriptPath { get; set; }

		/// <summary>
		///     Gets or sets the maximum number of parties that can concurrently do the quest.
		/// </summary>
		public int MaxConcurrentParties { get; set; }

		/// <summary>
		///     Gets or sets the maximum party size.
		/// </summary>
		public int MaxPartySize { get; set; }

		/// <summary>
		///     Gets or sets the minimum party size.
		/// </summary>
		public int MinPartySize { get; set; }

		/// <summary>
		///     Gets or sets the maximum number of repeats. -1 = infinite.
		/// </summary>
		public int MaxRepeats { get; set; }

		/// <summary>
		///		Gets or sets the period of time from first quest attempt, to resetting the quest attempts counter.
		/// </summary>
		public TimeSpan RepeatResetInterval { get; set; } = TimeSpan.FromDays(1.0d);

		/// <summary>
		///     Gets or sets the required region name.
		/// </summary>
		public string RequiredRegionName { get; set; }

		/// <summary>
		///		Gets or sets whether party members are allowed to rejoin the quest.
		/// </summary>
		public bool AllowRejoin { get; set; }

		public override string ToString() => $"{{{Name}|'{FriendlyName}'}}";

		public ValidationResult Validate()
		{
			var result = new ValidationResult(this);

			if (string.IsNullOrWhiteSpace(Name))
				result.Errors.Add(new ValidationError($"{nameof(Name)} is null or whitespace."));

			if (MaxConcurrentParties < 1)
				result.Warnings.Add(new ValidationWarning($"{nameof(MaxConcurrentParties)} is less than 1. No parties can embark on this quest."));

			if (MaxPartySize < 1)
				result.Warnings.Add(new ValidationWarning($"{nameof(MaxPartySize)} is less than 1. No parties can embark on this quest."));

			if (!string.IsNullOrWhiteSpace(RequiredRegionName))
			{
				var region = TShock.Regions.GetRegionByName(RequiredRegionName);

				if (region == null)
					result.Errors.Add(new ValidationError($"Cound not find {nameof(RequiredRegionName)}, '{RequiredRegionName}'."));
			}

			return result;
		}
	}
}
