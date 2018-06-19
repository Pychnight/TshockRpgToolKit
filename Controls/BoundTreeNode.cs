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

		public bool CanAcceptDraggedNode(BoundTreeNode draggedNode)
		{
			if( this.BoundObject is CategoryModel )
			{
				return draggedNode.BoundObject is IncludeModel;
			}

			if( this.BoundObject is IncludeModel )
			{
				if( draggedNode.BoundObject is IncludeModel ||
					draggedNode.BoundObject is CategoryModel )
				{
					return false;
				}
				else
					return true;
			}
			
			//this must be an item type
			if( draggedNode.BoundObject is IncludeModel )
			{
				return false;
			}

			//if( this.Parent != null )
			//{

			//}

			return true;
		}

		/// <summary>
		/// Helps determine how this dropped should be handled, either by inserting after, or inserting as a child.
		/// </summary>
		/// <param name="draggedNode"></param>
		public bool ShouldInsertDraggedNodeAsChild(BoundTreeNode draggedNode)
		{
			if( this.BoundObject is CategoryModel )
			{
				return true;
			}

			if( this.BoundObject is IncludeModel )
			{
				return true;
			}

			//this must be an item type, only insert behind
			//if( draggedNode.BoundObject is IncludeModel )
			//{
			//	return false;
			//}

			return false;
		}
	}
}
