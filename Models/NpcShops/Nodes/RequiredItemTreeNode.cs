using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.NpcShops
{
	public class RequiredItemTreeNode : ModelTreeNode
	{
		public RequiredItemTreeNode()
		{
			CanEditModel = true;
			CanAdd = true;
			CanCopy = true;
			CanDelete = true;
			CanDrag = true;
		}

		public override object Clone()
		{
			var clone = (RequiredItemTreeNode)base.Clone();

			clone.Model = (IModel)this.Model?.Clone();//call clone on a RequiredItem, if it exists.

			return clone;
		}

		public override void TryDropWithNoTarget(TreeView treeView)
		{
			//do nothing, this is not allowed for shop products.
		}
	}
}
