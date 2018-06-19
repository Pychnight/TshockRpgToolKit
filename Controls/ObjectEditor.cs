using CustomNpcsEdit.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CustomNpcsEdit.Controls
{
	public interface IModel : INotifyPropertyChanged
	{
		string Name { get; set; }
	}

	public partial class ObjectEditor : UserControl
	{
		public OpenFileDialog OpenFileDialog { get; set; }
		public SaveFileDialog SaveFileDialog { get; set; }
		
		protected IList BoundItems
		{
			get => listBoxItems.DataSource as IList;
			set => listBoxItems.DataSource = value;
		}
								
		public ObjectEditor()
		{
			InitializeComponent();

			//removing this, really slows down file loads for some reason...
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;

			OnPostInitialize();
		}

		protected virtual void OnPostInitialize()
		{
		}

		public void SetBindingCollection(IList dataSource, string displayMember = "Name", string valueMember = "Name")
		{
			listBoxItems.DisplayMember = displayMember;
			listBoxItems.ValueMember = valueMember;
			BoundItems = dataSource;
		}

		public void Clear()
		{
			propertyGridItemEditor.SelectedObject = null;
			//listBoxItems.DataSource = null;

			toolStripLabelFileName.Text = "";

			treeViewItems.Nodes.Clear();
		}

		protected virtual object OnCreateItem()
		{
			throw new NotImplementedException();
		}

		protected virtual object OnCopyItem(object source)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnFileLoad(string fileName)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnFileSave(string fileName)
		{
			throw new NotImplementedException();
		}
				
		private void toolStripButtonNewFile_Click(object sender, EventArgs e)
		{
			Clear();

			var items = BoundItems;
			items?.Clear();
		}

		private void listBoxItems_SelectedValueChanged(object sender, EventArgs e)
		{
			propertyGridItemEditor.SelectedObject = listBoxItems.SelectedItem;
		}

		private void toolStripButtonAddItem_Click(object sender, EventArgs e)
		{
			var items = BoundItems;

			if( items == null )
				return;

			var newItem = OnCreateItem();
			items.Add(newItem);

			//treeview
			var node = new BoundTreeNode();
			node.BoundObject = (IModel)OnCreateItem();

			var selectedNode = treeViewItems.SelectedNode;

			if( selectedNode != null )
			{
				selectedNode.InsertAfter(node);
			}
			else
			{
				treeViewItems.Nodes.Add(node);
			}
		}
		
		private void toolStripButtonCopy_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;

			if( index > -1 && BoundItems!=null )
			{
				var copy = OnCopyItem(listBoxItems.SelectedItem);
				
				BoundItems.Insert(++index,copy);
			}


			//treeview
			var selectedNode = treeViewItems.SelectedNode;

			if(selectedNode!=null)
			{
				var src = ((BoundTreeNode)selectedNode).BoundObject;
				var copy = OnCopyItem(src);

				var newNode = new BoundTreeNode();
				newNode.BoundObject = (IModel)copy;

				selectedNode.InsertAfter(newNode);
			}
		}

		private void toolStripButtonDeleteItem_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;

			if(index>-1 && BoundItems!=null)
			{
				BoundItems.RemoveAt(index);
			}
			
			//treeview

			var selectedNode = treeViewItems.SelectedNode;

			if(selectedNode!=null)
			{
				treeViewItems.Nodes.Remove(selectedNode);
			}
		}

		private void toolStripButtonMoveUp_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;

			if( index > 0 && BoundItems!=null )
			{
				var item = BoundItems[index];

				BoundItems.RemoveAt(index);
				BoundItems.Insert(--index, item);
				listBoxItems.SelectedIndex = index;
			}
		}

		private void toolStripButtonMoveDown_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;
			var lastIndex = BoundItems.Count - 1;

			if( index > -1 && index < lastIndex )
			{
				var item = BoundItems[index];

				BoundItems.RemoveAt(index);

				index++;

				BoundItems.Insert(index, item);
				listBoxItems.SelectedIndex = index;
			}
		}

		private void toolStripButtonFileOpen_Click(object sender, EventArgs e)
		{
			var result = OpenFileDialog.ShowDialog();

			if(result== DialogResult.OK)
			{
				try
				{
					Clear();
					OnFileLoad(OpenFileDialog.FileName);
					toolStripLabelFileName.Text = OpenFileDialog.FileName;
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void toolStripButtonFileSaveAs_Click(object sender, EventArgs e)
		{
			var result = SaveFileDialog.ShowDialog();

			if(result == DialogResult.OK)
			{
				try
				{
					OnFileSave(SaveFileDialog.FileName);
					toolStripLabelFileName.Text = SaveFileDialog.FileName;
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		//----
		protected void SetTreeViewModels<T>(IList<BoundTreeNode> boundTreeNodes) where T : IModel
		{
			treeViewItems.Nodes.Clear();
			
			foreach(var bt in boundTreeNodes)
			{
				treeViewItems.Nodes.Add(bt);
			}
		}

		private void treeViewItems_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var selected = (BoundTreeNode)e.Node;

			propertyGridItemEditor.SelectedObject = selected.BoundObject;
		}

		private void treeViewItems_ItemDrag(object sender, ItemDragEventArgs e)
		{
			DoDragDrop(e.Item, DragDropEffects.Move);
		}

		private void treeViewItems_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		private void treeViewItems_DragDrop(object sender, DragEventArgs e)
		{
			// Retrieve the client coordinates of the drop location.
			Point targetPoint = treeViewItems.PointToClient(new Point(e.X, e.Y));

			// Retrieve the node at the drop location.
			BoundTreeNode targetNode = (BoundTreeNode)treeViewItems.GetNodeAt(targetPoint);

			// Retrieve the node that was dragged.
			BoundTreeNode draggedNode = (BoundTreeNode)e.Data.GetData(typeof(BoundTreeNode));

			// Confirm that the node at the drop location is not 
			// the dragged node and that target node isn't null
			// (for example if you drag outside the control)
			if( !draggedNode.Equals(targetNode) && targetNode != null )
			{
				if(targetNode.CanAcceptDraggedNode(draggedNode))
				{
					// Remove the node from its current 
					// location and add it to the node at the drop location.
					draggedNode.Remove();
					//targetNode.Nodes.Add(draggedNode);

					if(targetNode.ShouldInsertDraggedNodeAsChild(draggedNode))
					{
						targetNode.Nodes.Add(draggedNode);
						// Expand the node at the location 
						// to show the dropped node.
						targetNode.Expand();
					}
					else
					{
						targetNode.InsertAfter(draggedNode);
					}
				}
			}
			else if(targetNode == null)
			{
				//were dropping at top level... ie, the treeview itself
				if( draggedNode.BoundObject is IncludeModel )
					return;//includes can only live within categories.

				draggedNode.Remove();
				treeViewItems.Nodes.Add(draggedNode);
			}
		}
	}
}
