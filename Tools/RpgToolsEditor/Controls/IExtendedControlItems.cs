using System.Windows.Forms;

namespace RpgToolsEditor.Controls
{
	/// <summary>
	/// Interface for Adding additional, non standard controls to manipulate the model-views in the ModelTreeEditor's TreeView.
	/// </summary>
	public interface IExtendedItemControls
	{
		ToolStrip CreateToolStrip(TreeView treeView);
	}
}
