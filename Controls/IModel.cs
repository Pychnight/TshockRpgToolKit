using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Controls
{
	public interface IModel : INotifyPropertyChanged, ICloneable
	{
		string Name { get; set; }
	}

	public static class IModelExtensions
	{
		public static void TryAddCopySuffix(this IModel model)
		{
			const string suffix = "(Copy)";

			if(	!model.Name.EndsWith(suffix))
			{
				model.Name = model.Name + suffix;
			}
		}
	}
}
