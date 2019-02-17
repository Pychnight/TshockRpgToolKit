using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel.Design;
using System.ComponentModel;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Holds and provides fast access to ValueOverride objects, which can alter default Currency values. 
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	public class ValueOverrideList<TKey> : List<ValueOverride<TKey>> where TKey : ICloneable
	{
		public ValueOverrideList() : base()
		{
		}

		public ValueOverrideList(ValueOverrideList<TKey> source) : base()
		{
			var clonedItems = source.Select(i => (ValueOverride<TKey>)i.Clone());
			AddRange(clonedItems);
		}
		
		public override string ToString()
		{
			//ugly.
			if(typeof(TKey) == typeof(TileKey))
			{
				return $"{Count} Tile Overrides";
			}

			if(typeof(TKey) == typeof(ItemKey))
			{
				return $"{Count} Item Overrides";
			}

			if(typeof(TKey) == typeof(string) )
			{
				return $"{Count} String Overrides";
			}

			return base.ToString();
		}
	}
}
