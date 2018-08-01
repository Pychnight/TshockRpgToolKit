using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	/// <summary>
	/// Key for FishingReward ValueOverrides.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class ItemKey : IEquatable<ItemKey>
	{
		//item
		int itemId;
		byte prefix;
		
		//prefix
		public ItemKey(int itemId, byte prefix)
		{
			this.itemId = itemId;
			this.prefix = prefix;
		}

		public bool Equals(ItemKey other)
		{
			return itemId == other.itemId && prefix == other.prefix;
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
			return itemId ^ prefix;
		}
	}
}
