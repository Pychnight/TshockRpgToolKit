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

		protected override string HelpTopic => base.HelpTopic;

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

		protected override string GetDisplayText(object value)
		{
			var item = (ValueOverride<TileKey>)value;
			var key = item.Key;
			string val = item.ValueString;

			if( string.IsNullOrWhiteSpace(item.ValueString) )
				val = "???";
			
			return $"Tile:{key.Type}, Wall:{key.Wall} = {val}";
		}

		protected override void ShowHelp()
		{
			base.ShowHelp();
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

		protected override string GetDisplayText(object value)
		{
			var item = (ValueOverride<ItemKey>)value;
			var key = item.Key;
			string val = item.ValueString;

			if( string.IsNullOrWhiteSpace(item.ValueString) )
				val = "???";

			return $"ItemId:{key.ItemId}, Prefix:{key.Prefix} = {val}";
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

		protected override string GetDisplayText(object value)
		{
			var item = (ValueOverride<string>)value;
			var key = item.Key;
			string val = item.ValueString;

			if( string.IsNullOrWhiteSpace(item.ValueString) )
				val = "???";

			return $"Key:{key} = {val}";
		}
	}

	// Collection editors with custom captions to match the properties they are tied to.
	
	public class KillingCollectionEditor : StringKeyCollectionEditor
	{
		public KillingCollectionEditor() : base("Killing Overrides") { }

		protected override string GetDisplayText(object value)
		{
			var item = (ValueOverride<string>)value;
			var key = item.Key;
			string val = item.ValueString;

			if( string.IsNullOrWhiteSpace(item.ValueString) )
				val = "???";

			return $"NPC:{key} = {val}";
		}
	}

	public class MiningCollectionEditor : TileKeyCollectionEditor
	{
		public MiningCollectionEditor() : base("Mining Overrides") { }
	}

	public class PlacingCollectionEditor : TileKeyCollectionEditor
	{
		public PlacingCollectionEditor() : base("Placing Overrides") { }
	}

	public class PlayingCollectionEditor : StringKeyCollectionEditor
	{
		public PlayingCollectionEditor() : base("Playing Overrides") { }
	}

	public class FishingCollectionEditor : ItemKeyCollectionEditor
	{
		public FishingCollectionEditor() : base("Fishing Overrides") { }
	}

}
