using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgToolsEditor.Controls
{
	public abstract class ModelTree
	{
		public abstract IList<ModelTreeNode> CreateTree();
		public abstract IList<ModelTreeNode> LoadTree(string path);
		public abstract void SaveTree(IList<ModelTreeNode> tree, string path);
		
		/// <summary>
		/// Called when the user Adds an item when no node is selected.
		/// </summary>
		/// <returns>ModelTreeNode.</returns>
		public abstract ModelTreeNode CreateDefaultItem();
	}

	public class ModelTreeNode : TreeNode//, INotifyPropertyChanged
	{
		IModel model;
		public IModel Model
		{
			get => model;
			set
			{
				onModelChange(value,model);
			}
		}
		
		public bool CanEditModel { get; protected set; }
		public bool CanAddChild { get; protected set; }
		public bool CanCopy { get; protected set; }
		public bool CanDelete { get; protected set; }
		public bool CanDrag { get; protected set; }

		void onModelChange(IModel newModel, IModel oldModel)
		{
			if(oldModel!=null)
			{
				oldModel.PropertyChanged -= onModelPropertyChanged;
			}

			model = newModel;

			if(newModel!=null)
			{
				newModel.PropertyChanged += onModelPropertyChanged;

				onModelPropertyChanged(newModel, new PropertyChangedEventArgs("Name"));//hack to get name to update on object change.
			}
		}

		private void onModelPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch( args.PropertyName )
			{
				case "Name":
					this.Text = Model.Name;
					break;
			}
			
			//OnModelPropertyChanged((IModel)sender, args.PropertyName);
		}

		//protected void OnModelPropertyChanged(IModel model, string propertyName)
		//{
		//	this.Text = model.Name;
		//}

		public virtual ModelTreeNode AddItem()
		{
			return null;
		}

		public virtual ModelTreeNode Copy()
		{
			return null;
		}

		//internal void Delete()
		//{
		//	throw new NotImplementedException();
		//}
	}
}
