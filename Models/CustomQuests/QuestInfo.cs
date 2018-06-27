using RpgToolsEditor.Controls;
using System;
using System.ComponentModel;

namespace RpgToolsEditor.Models.CustomQuests
{
	/// <summary>
	///     Represents information about a quest.
	/// </summary>
	public sealed class QuestInfo : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New QuestInfo";

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		[Category("Basic Properties")]
		[Description("Gets or sets the name.")]
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
		///     Gets or sets the friendly name.
		/// </summary>
		[Category("Basic Properties")]
		[Description("Gets or sets the friendly name.")]
		public string FriendlyName { get; set; }

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		[Category("Basic Properties")]
		[Description("Gets or sets the description.")]
		public string Description { get; set; }

		/// <summary>
		///     Gets or sets the script path.
		/// </summary>
		[Category("Basic Properties")]
		[Description("Gets or sets the script path.")]
		public string ScriptPath { get; set; }

		/// <summary>
		///     Gets or sets the maximum number of parties that can concurrently do the quest.
		/// </summary>
		[Category("Parties")]
		[Description("Gets or sets the maximum number of parties that can concurrently do the quest.")]
		public int MaxConcurrentParties { get; set; } = -1;

		/// <summary>
		///     Gets or sets the maximum party size.
		/// </summary>
		[Category("Parties")]
		[Description("Gets or sets the maximum party size.")]
		public int MaxPartySize { get; set; } = 8;

		/// <summary>
		///     Gets or sets the minimum party size.
		/// </summary>
		[Category("Parties")]
		[Description("Gets or sets the minimum party size.")]
		public int MinPartySize { get; set; } = 1;

		/// <summary>
		///     Gets or sets the maximum number of repeats. -1 = infinite.
		/// </summary>
		[Category("Repeats")]
		[Description("Gets or sets the maximum number of repeats. -1 = infinite.")]
		public int MaxRepeats { get; set; } = -1;

		/// <summary>
		///		Gets or sets the period of time from first quest attempt, to resetting the quest attempts counter.
		/// </summary>
		[Category("Repeats")]
		[Description("Gets or sets the period of time from first quest attempt, to resetting the quest attempts counter.")]
		public TimeSpan RepeatResetInterval { get; set; } = TimeSpan.FromDays(1.0d);

		/// <summary>
		///     Gets or sets the required region name.
		/// </summary>
		[Category("Basic Properties")]
		[Description("Sets a required region name.")]
		public string RequiredRegionName { get; set; }

		/// <summary>
		///		Gets or sets whether party members are allowed to rejoin the quest.
		/// </summary>
		[Category("Parties")]
		[Description("Toggles whether party members are allowed to rejoin the quest")]
		public bool AllowRejoin { get; set; }

		public QuestInfo()
		{
		}

		public QuestInfo(QuestInfo other)
		{
			Name = other.Name;
			FriendlyName = other.FriendlyName;
			Description = other.Description;
			ScriptPath = other.ScriptPath;
			MaxConcurrentParties = other.MaxConcurrentParties;
			MaxPartySize = other.MaxPartySize;
			MinPartySize = other.MinPartySize;
			MaxRepeats = other.MaxRepeats;
			RepeatResetInterval = other.RepeatResetInterval;
			RequiredRegionName = other.RequiredRegionName;
			AllowRejoin = other.AllowRejoin;
		}
		
		public override string ToString()
		{
			return $"{{{Name}|'{FriendlyName}'}}";
		}
	}
}
