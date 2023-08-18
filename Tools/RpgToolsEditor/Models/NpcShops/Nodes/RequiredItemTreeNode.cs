using RpgToolsEditor.Controls;
using System.Windows.Forms;

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

			ImageIndex = SelectedImageIndex = 5;
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

		public override bool CanAcceptDraggedNode(ModelTreeNode node) => node is RequiredItemTreeNode;

		public override bool TryAcceptDraggedNode(ModelTreeNode draggedNode)
		{
			if (CanAcceptDraggedNode(draggedNode))
			{
				draggedNode.Remove();
				AddSibling(draggedNode);

				return true;
			}

			return false;
		}
	}
}
