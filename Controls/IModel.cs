using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Controls
{
	public interface IModel : INotifyPropertyChanged
	{
		string Name { get; set; }
	}
}
