using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Models.Banking
{
	public class ItemKeyConverter : ExpandableObjectConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if( destinationType == typeof(ItemKey) )
				return true;

			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if( destinationType == typeof(System.String) && value is ItemKey )
			{
				var ik = (ItemKey)value;

				return "ItemId: " + ik.ItemId + ", Prefix: " + ik.Prefix;
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		//public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
		//{
		//	return base.CreateInstance(context, propertyValues);
		//}

		//public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
		//{
		//	return base.GetCreateInstanceSupported(context);
		//}
	}
}
