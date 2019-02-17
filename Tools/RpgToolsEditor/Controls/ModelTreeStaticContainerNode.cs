using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Controls
{
	public abstract class ModelTreeStaticContainerNode : ModelTreeNode
	{
		public ModelTreeStaticContainerNode(string text = "Container")
		{
			CanEditModel = false;
			CanAdd = true;
			CanCopy = false;
			CanDelete = false;
			CanDrag = false;

			Text = text;
		}

		public virtual void AddChildModels(IList<IModel> models)
		{
			foreach( var m in models )
			{
				AddChildModel(m);
			}
		}

		public abstract void AddChildModel(IModel model);

		public virtual IList<IModel> GetChildModels()
		{
			var models = new List<IModel>();

			foreach(var n in Nodes)
			{
				var modelTreeNode = n as ModelTreeNode;

				if(modelTreeNode!=null && modelTreeNode.Model!=null)
				{
					models.Add(modelTreeNode.Model);	
				}
			}

			return models;
		}
	}
}
