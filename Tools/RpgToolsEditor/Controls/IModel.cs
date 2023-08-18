using System;
using System.ComponentModel;

namespace RpgToolsEditor.Controls
{
	public interface IModel : INotifyPropertyChanged, ICloneable
	{
		string Name { get; set; }
	}
}
