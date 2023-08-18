using System;
using System.ComponentModel;
using System.Globalization;

namespace RpgToolsEditor.Models.Banking
{
	public class TileKeyConverter : ExpandableObjectConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(TileKey))
				return true;

			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => base.ConvertFrom(context, culture, value);

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(System.String) && value is TileKey)
			{
				var tk = (TileKey)value;

				return "Type: " + tk.Type + ", Wall: " + tk.Wall;
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
