using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Key for FishingReward ValueOverrides.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[JsonObject(MemberSerialization.OptIn)]
	public class ItemKey : IEquatable<ItemKey>, ICloneable
	{
		[JsonProperty(Order = 0)]
		public int ItemId { get; set; }

		[JsonProperty(Order = 1)]
		public byte Prefix { get; set; }

		public ItemKey() { }
		
		//prefix
		public ItemKey(int itemId, byte prefix)
		{
			ItemId = itemId;
			Prefix = prefix;
		}

		public ItemKey(ItemKey source)
		{
			ItemId = source.ItemId;
			Prefix = source.Prefix;
		}

		public object Clone()
		{
			return new ItemKey(this);
		}

		public bool Equals(ItemKey other)
		{
			return ItemId == other.ItemId && Prefix == other.Prefix;
		}

		public override bool Equals(object obj)
		{
			ItemKey other = obj as ItemKey;

			if( other != null )
				return Equals(other);
			else
				return false;
		}

		public override int GetHashCode()
		{
			return ItemId ^ Prefix;
		}

		public override string ToString()
		{
			return $"ItemKey(ItemId: {ItemId}, Prefix: {Prefix})";
		}
	}
}
