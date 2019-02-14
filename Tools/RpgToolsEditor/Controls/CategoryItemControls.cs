using RpgToolsEditor.Models.CustomNpcs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgToolsEditor.Controls
{
	/// <summary>
	/// Extends the ModelTreeEditor with an Add Category button, for CustomNpcs based editors.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TNode"></typeparam>
	public class CategoryItemControls<TModel,TNode> : IExtendedItemControls where TModel : IModel, new()
																			where TNode : ModelTreeNode, new()
	{
		TreeView treeViewItems;
		ToolStrip toolStrip;
		ToolStripButton toolStripButtonAddCategory;

		public ToolStrip CreateToolStrip(TreeView treeView)
		{
			treeViewItems = treeView ?? throw new ArgumentNullException("TreeView");
			var ts = toolStrip = new ToolStrip();
			var tsb = toolStripButtonAddCategory = new ToolStripButton("Add New Category");

			ts.GripStyle = ToolStripGripStyle.Hidden;
			tsb.Enabled = true;
			tsb.DisplayStyle = ToolStripItemDisplayStyle.Image;
			//..."The Parent Trap". ( yes this is ugly and fragile. )
			tsb.Image = ((ModelTreeEditor)(treeView.Parent.Parent.Parent.Parent.Parent)).imageListModelTreeEditorDefaultItems.Images[1];
			
			tsb.ToolTipText = tsb.Text;
			tsb.Click += toolStripButtonAddCategory_Click;
			
			ts.Items.Add(tsb);
			//treeView.AfterSelect += linkedTreeView_AfterSelect;
			
			return ts;
		}
		
		//private void linkedTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		//{
		//	var selected = (ModelTreeNode)e.Node;
		//	var enable = false;

		//	//if(selected != null)
		//	//{
		//	//	enable = selected.Parent == null;//only allow adding categories on top level nodes
		//	//}
		//	//else
		//	//{
		//	//	enable = true;//nothing selected, we should be able to add a category
		//	//}
			
		//	toolStripButtonAddCategory.Enabled = enable;
		//}

		private void toolStripButtonAddCategory_Click(object sender, EventArgs args )
		{
			var selectedNode = treeViewItems.SelectedNode as ModelTreeNode;
			var categoryModel = new CategoryModel();
			var categoryNode = new CategoryTreeNode<TModel, TNode>(categoryModel,"");

			if( selectedNode != null && selectedNode.Parent == null )
			{
				selectedNode.AddSibling(categoryNode);//top most item, add as sibling
			}
			else
			{
				treeViewItems.Nodes.Add(categoryNode);//not top most item, so add new category to end of top most items list.
			}
		}
	}
}
