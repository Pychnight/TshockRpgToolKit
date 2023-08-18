using System.Windows.Forms;

namespace RpgToolsEditor.Controls
{
	public static class TreeNodeExtensions
	{
		public static void InsertAfter(this TreeNode target, TreeNode source)
		{
			if (target == source)
				return;//cant insert itself after..

			var parent = target.Parent;

			if (parent != null)
			{
				var targetIndex = target.Index + 1;
				parent.Nodes.Insert(targetIndex, source);
			}
			else
			{
				var targetIndex = target.Index + 1;
				target.TreeView.Nodes.Insert(targetIndex, source);
				//target.TreeView.Nodes.Add(source);
			}
		}
	}
}
