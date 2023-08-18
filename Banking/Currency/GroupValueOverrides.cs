using System.Collections.Generic;

namespace Banking.Currency
{
	/// <summary>
	/// Contains a dictionary of ValueOverrideLists, each keyed by a TShock Group name.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	public class GroupValueOverrides<TKey> : Dictionary<string, ValueOverrideList<TKey>>
	{
	}
}
