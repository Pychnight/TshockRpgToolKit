using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNpcsEdit.Models.Leveling
{
	public class StringHashSetCollectionEditor : CollectionEditor
	{
		public StringHashSetCollectionEditor() : base(type: typeof(List<string>))
		{
		}

		//public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		//{
		//	return base.EditValue(context, provider, value);
		//}

		//public override string ToString()
		//{
		//	return base.ToString();
		//}

		//protected override bool CanRemoveInstance(object value)
		//{
		//	//return base.CanRemoveInstance(value);
		//	return true;
		//}

		//protected override bool CanSelectMultipleInstances()
		//{
		//	return base.CanSelectMultipleInstances();
		//}

		//protected override CollectionForm CreateCollectionForm()
		//{
		//	return base.CreateCollectionForm();
		//}

		//protected override Type CreateCollectionItemType()
		//{
		//	return base.CreateCollectionItemType();
		//	//return typeof(string);
		//}

		//protected override object CreateInstance(Type itemType)
		//{
		//	//return base.CreateInstance(itemType);
		//	return base.CreateInstance(itemType);
		//}

		//protected override Type[] CreateNewItemTypes()
		//{
		//	return base.CreateNewItemTypes();
		//}

		//protected override void DestroyInstance(object instance)
		//{
		//	base.DestroyInstance(instance);
		//}

		//protected override string GetDisplayText(object value)
		//{
		//	return base.GetDisplayText(value);
		//}

		//protected override object[] GetItems(object editValue)
		//{
		//	//return base.GetItems(editValue);
		//	var hs = (HashSet<string>)editValue;
		//	var items = hs.ToArray();

		//	return items;
		//}

		//protected override IList GetObjectsFromInstance(object instance)
		//{
		//	return base.GetObjectsFromInstance(instance);
		//}

		//protected override object SetItems(object editValue, object[] value)
		//{
		//	return base.SetItems(editValue, value);
		//}
	}
}
