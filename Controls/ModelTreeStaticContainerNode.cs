using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgToolsEditor.Controls
{
	public class ModelTreeStaticContainerNode : ModelTreeNode
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

		//public virtual void SetChildModels(IList<IModel> models)
		//{
		//}
	}
}
