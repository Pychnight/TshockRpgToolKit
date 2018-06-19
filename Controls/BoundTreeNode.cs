using CustomNpcsEdit.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomNpcsEdit.Controls
{
	public class BoundTreeNode : TreeNode // where T : IModel, INotifyPropertyChanged
	{
		IModel boundObject;

		public IModel BoundObject
		{
			get => boundObject;
			set
			{
				if(boundObject != null)
					boundObject.PropertyChanged -= OnPropertyChanged;

				boundObject = value;

				if( boundObject != null )
				{
					boundObject.PropertyChanged += OnPropertyChanged;

					OnPropertyChanged(boundObject, new PropertyChangedEventArgs("Name"));//hack to get name to update on object change.
				}

				if( boundObject is CategoryModel )
				{
					this.ImageIndex = this.SelectedImageIndex = 1;
					//var cm = boundObject as CategoryModel;
					//cm.LoadIncludes(cm.BasePath);
				}
				else if( boundObject is IncludeModel)
				{
					this.ImageIndex = this.SelectedImageIndex = 2;
				}
				else
					this.ImageIndex = this.SelectedImageIndex = 0;

			}
		}

		protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch( args.PropertyName )
			{
				case "Name":
					this.Text = BoundObject.Name;
					break;
			}
		}
	}
}
