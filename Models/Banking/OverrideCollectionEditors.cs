using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.Banking
{
	/// <summary>
	/// Base class for custom CollectionEditors that allow setting the Caption text.
	/// </summary>
	public abstract class CaptionedCollectionEditor : CollectionEditor
	{
		const string DefaultCaption = "CaptionedCollectionEditor";

		string caption;

		public CaptionedCollectionEditor(string caption, Type type) : base(type)
		{
			this.caption = caption;
		}

		protected override CollectionForm CreateCollectionForm()
		{
			CollectionForm collectionForm = base.CreateCollectionForm();

			collectionForm.Text = caption ?? DefaultCaption;

			return collectionForm;
		}
	}

	//we can't use new() constraint on System.String -- so we have to make 3 collection editors that can
	//insert keys for the overrides, else the keys will never show up or be added in the property grid.

	public class TileKeyCollectionEditor : CaptionedCollectionEditor
	{
		public TileKeyCollectionEditor() : this("Tile Overrides Editor") { }
		public TileKeyCollectionEditor(string caption) : base(caption, typeof(ValueOverrideList<TileKey>)) { }

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

	public class ItemKeyCollectionEditor : CaptionedCollectionEditor
	{
		public ItemKeyCollectionEditor() : this("Item Overrides Editor") { }
		public ItemKeyCollectionEditor(string caption) : base(caption, typeof(ValueOverrideList<ItemKey>)) { }

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

	public class StringKeyCollectionEditor : CaptionedCollectionEditor
	{
		public StringKeyCollectionEditor() : this("String Overrides Editor") { }
		public StringKeyCollectionEditor(string caption) : base(caption, typeof(ValueOverrideList<string>)) { }
		
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

	// Collection editors with custom captions to match the properties they are tied to.
	public class KillingCollectionEditor : StringKeyCollectionEditor
	{
		public KillingCollectionEditor() : base("Killing Overrides") { }
	}

	public class MiningCollectionEditor : TileKeyCollectionEditor
	{
		public MiningCollectionEditor() : base("Mining Overrides") { }
	}

	public class PlacingCollectionEditor : TileKeyCollectionEditor
	{
		public PlacingCollectionEditor() : base("Placing Overrides") { }
	}

}
