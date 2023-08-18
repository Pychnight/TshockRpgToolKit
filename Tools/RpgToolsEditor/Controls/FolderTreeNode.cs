namespace RpgToolsEditor.Controls
{
	public class FolderTreeNode : ModelTreeNode
	{
		public FolderTreeNode() : base()
		{
			CanDelete = false;
			CanCopy = false;
			CanDrag = false;

			ImageIndex = SelectedImageIndex = 2;//yeah, were hardcoded into using index 2 for now :/
		}

		public override bool CanAcceptDraggedNode(ModelTreeNode node) => false;
	}
}
