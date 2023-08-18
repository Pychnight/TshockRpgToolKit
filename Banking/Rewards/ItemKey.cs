using Newtonsoft.Json;
using System;

namespace Banking.Rewards
{
	/// <summary>
	/// Key for FishingReward ValueOverrides.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class ItemKey : IEquatable<ItemKey>
	{
		[JsonProperty(Order = 0)]
		public int ItemId { get; set; }

		[JsonProperty(Order = 1)]
		public byte Prefix { get; set; }

		public ItemKey(int itemId, byte prefix)
		{
			ItemId = itemId;
			Prefix = prefix;
		}

		public bool Equals(ItemKey other) => ItemId == other.ItemId && Prefix == other.Prefix;

		public override bool Equals(object obj)
		{
			ItemKey other = obj as ItemKey;

			if (other != null)
				return Equals(other);
			else
				return false;
		}

		public override int GetHashCode() => ItemId ^ Prefix;
	}
}
