using System.Collections.Generic;

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
}
