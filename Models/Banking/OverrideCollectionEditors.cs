using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.Banking
{
	//we can't use new() constraint on System.String -- so we have to make 3 collection editors that can
	//insert keys for the overrides, else the keys will never show up or be added in the property grid.
	public class TileKeyCollectionEditor : CollectionEditor
	{
		public TileKeyCollectionEditor() : base(typeof(ValueOverrideList<TileKey>)) { }

		protected override object CreateInstance(Type itemType)
		{
			if( itemType == typeof(ValueOverride<TileKey>) )
			{
				var item = new ValueOverride<TileKey>();
				item.Key = new TileKey();
				return item;
			}

			return base.CreateInstance(itemType);
		}
	}

	public class ItemKeyCollectionEditor : CollectionEditor
	{
		public ItemKeyCollectionEditor() : base(typeof(ValueOverrideList<ItemKey>)) { }

		protected override object CreateInstance(Type itemType)
		{
			if( itemType == typeof(ValueOverride<ItemKey>) )
			{
				var item = new ValueOverride<ItemKey>();
				item.Key = new ItemKey();
				return item;
			}

			return base.CreateInstance(itemType);
		}
	}

	public class StringKeyCollectionEditor : CollectionEditor
	{
		public StringKeyCollectionEditor() : base(typeof(ValueOverrideList<string>)) { }

		protected override object CreateInstance(Type itemType)
		{
			if( itemType == typeof(ValueOverride<string>) )
			{
				var item = new ValueOverride<string>();
				item.Key = "";
				return item;
			}

			return base.CreateInstance(itemType);
		}
	}
}
