using System;
using System.Collections.Generic;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Contains a dictionary of ValueOverrideLists, each keyed by a TShock Group name.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	public class GroupValueOverrides<TKey> : Dictionary<string, ValueOverrideList<TKey>>, ICloneable where TKey : ICloneable
	{
		//we cant just use dictionary's copy ctor, because it will shallow copy the values
		public object Clone()
		{
			var dest = new GroupValueOverrides<TKey>();

			foreach (var kvp in this)
			{
				var clonedValue = new ValueOverrideList<TKey>(kvp.Value);
				dest.Add(kvp.Key, clonedValue);
			}

			return dest;
		}
	}
}
