using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgToolsEditor.Controls
{
	public class ModelTreeNode : TreeNode//, INotifyPropertyChanged
	{
		IModel model;
		public IModel Model
		{
			get => model;
			set
			{
				onModelChange(value, model);
			}
		}

		public bool CanEditModel { get; protected set; }
		public bool CanAddChild { get; protected set; }
		public bool CanCopy { get; protected set; }
		public bool CanDelete { get; protected set; }
		public bool CanDrag { get; protected set; }

		public override object Clone()
		{
			var src = this;
			var dst = (ModelTreeNode)base.Clone();

			dst.CanEditModel = src.CanEditModel;
			dst.CanAddChild = src.CanAddChild;
			dst.CanCopy = src.CanCopy;
			dst.CanDelete = src.CanDelete;
			dst.CanDrag = src.CanDrag;

			if(Model!=null)
			{
				dst.Model = (IModel)src.Model.Clone();
			}

			return dst;
		}

		void onModelChange(IModel newModel, IModel oldModel)
		{
			if( oldModel != null )
			{
				oldModel.PropertyChanged -= onModelPropertyChanged;
			}

			model = newModel;

			if( newModel != null )
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
			var clone = (ModelTreeNode)Clone();

			return clone;
		}

		//internal void Delete()
		//{
		//	throw new NotImplementedException();
		//}
	}
}
