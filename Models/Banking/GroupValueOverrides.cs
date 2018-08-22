using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Contains a dictionary of ValueOverrideLists, each keyed by a TShock Group name.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	public class GroupValueOverrides<TKey> : Dictionary<string,ValueOverrideList<TKey>>
	{
	}
}
