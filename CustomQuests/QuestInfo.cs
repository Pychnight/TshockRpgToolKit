namespace CustomQuests
{
    /// <summary>
    ///     Represents information about a quest.
    /// </summary>
    public sealed class QuestInfo
    {
        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the friendly name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        ///     Gets or sets the maximum party size.
        /// </summary>
        public int MaxPartySize { get; set; }

        /// <summary>
        ///     Gets or sets the minimum party size.
        /// </summary>
        public int MinPartySize { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}
