using CustomNpcsEdit.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CustomNpcsEdit.Controls
{
	public partial class ObjectEditor : UserControl, INotifyPropertyChanged
	{
		string currentFilePath = "";
		public string CurrentFilePath
		{
			get => currentFilePath;
			set
			{
				currentFilePath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFilePath)));
			}
		}

		bool isTreeDirty;
		public bool IsTreeDirty
		{
			get => isTreeDirty;
			set 
			{
				isTreeDirty = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTreeDirty)));
			}
		}
				
		public string Caption => CurrentFilePath + ( IsTreeDirty ? "*" : "" );

		bool canAddCategory = true;
		public bool CanAddCategory
		{
			get => canAddCategory;
			set 
			{
				canAddCategory = value;
				toolStripButtonAddCategory.Enabled = canAddCategory;
			}
		}

		bool supportMultipleItems = true;
		public bool SupportMultipleItems
		{
			get => supportMultipleItems;
			set
			{
				supportMultipleItems = value;
				toolStripButtonAddItem.Enabled = toolStripButtonCopy.Enabled = toolStripButtonDeleteItem.Enabled = value;
				//treeViewItems.Visible = value;
			}
		}
				
		public OpenFileDialog OpenFileDialog { get; set; }
		public SaveFileDialog SaveFileDialog { get; set; }

		protected PropertyGrid PropertyGrid => propertyGridItemEditor;

		public event PropertyChangedEventHandler PropertyChanged;

		public ObjectEditor()
		{
			InitializeComponent();

			//removing this, really slows down file loads for some reason...
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		}
		
		public void Clear()
		{
			propertyGridItemEditor.SelectedObject = null;
			//listBoxItems.DataSource = null;

			treeViewItems.Nodes.Clear();

			IsTreeDirty = false;
			//toolStripLabelFileName.Text = "";
			CurrentFilePath = "";
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
				
		protected void SetTreeViewModels<T>(IList<BoundTreeNode> boundTreeNodes) where T : IModel
		{
			treeViewItems.Nodes.Clear();

			foreach( var bt in boundTreeNodes )
			{
				treeViewItems.Nodes.Add(bt);
			}
		}

		protected IList<BoundTreeNode> GetTreeViewModels()
		{
			var nodes = treeViewItems.Nodes;
			var boundTreeNodes = nodes.Cast<BoundTreeNode>().ToList();
			return boundTreeNodes;
		}
		
		public void NewFile()
		{
			if( IsTreeDirty )
			{
				var result = MessageBox.Show("There are unsaved changes present. Proceed?", "Delete Items?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

				if( result != DialogResult.OK )
					return;
			}

			Clear();
			
			if(!SupportMultipleItems)
			{
				//we're in single item mode, so create a default item...
				var item = OnCreateItem();
				propertyGridItemEditor.SelectedObject = item;
			}
		}

		private void toolStripButtonNewFile_Click(object sender, EventArgs e)
		{
			NewFile();
		}
		
		private void toolStripButtonAddItem_Click(object sender, EventArgs e)
		{
			//var items = BoundItems;

			//if( items == null )
			//	return;

			//var newItem = OnCreateItem();
			//items.Add(newItem);

			//treeview
			
			var selectedNode = treeViewItems.SelectedNode;
			var node = new BoundTreeNode();

			//if we've already selected a node, what type of model to create, and how to add it?
			if(selectedNode!=null)
			{
				var boundNode = (BoundTreeNode)selectedNode;
				var selectedModel = boundNode.BoundObject;

				if(selectedModel is CategoryModel)
				{
					node.BoundObject = new IncludeModel();
					selectedNode.Nodes.Add(node);
					selectedNode.Expand();
					//treeViewItems.SelectedNode = node;
					IsTreeDirty = true;
					return;
				}
				else if(selectedModel is IncludeModel)
				{
					node.BoundObject = (IModel)OnCreateItem();
					selectedNode.Nodes.Add(node);
					selectedNode.Expand();
					//treeViewItems.SelectedNode = node;
					IsTreeDirty = true;
					return;
				}
			}

			node.BoundObject = (IModel)OnCreateItem();

			//insert or add to root?
			if( selectedNode != null )
			{
				selectedNode.InsertAfter(node);
				IsTreeDirty = true;
			}
			else
			{
				treeViewItems.Nodes.Add(node);
				IsTreeDirty = true;
			}

			//treeViewItems.SelectedNode = node;
		}
		
		private void toolStripButtonCopy_Click(object sender, EventArgs e)
		{
			//var index = listBoxItems.SelectedIndex;

			//if( index > -1 && BoundItems!=null )
			//{
			//	var copy = OnCopyItem(listBoxItems.SelectedItem);
				
			//	BoundItems.Insert(++index,copy);
			//}


			//treeview
			var selectedNode = (BoundTreeNode)treeViewItems.SelectedNode;

			if(selectedNode!=null && selectedNode.CanCopy())
			{
				var src = ((BoundTreeNode)selectedNode).BoundObject;
				var copy = OnCopyItem(src);

				var newNode = new BoundTreeNode();
				newNode.BoundObject = (IModel)copy;

				selectedNode.InsertAfter(newNode);
				IsTreeDirty = true;
			}
		}

		private void toolStripButtonDeleteItem_Click(object sender, EventArgs e)
		{
			var selectedNode = treeViewItems.SelectedNode;

			if(selectedNode!=null)
			{
				treeViewItems.Nodes.Remove(selectedNode);
				IsTreeDirty = true;
			}
		}

		private void toolStripButtonMoveUp_Click(object sender, EventArgs e)
		{
			//var index = listBoxItems.SelectedIndex;

			//if( index > 0 && BoundItems!=null )
			//{
			//	var item = BoundItems[index];

			//	BoundItems.RemoveAt(index);
			//	BoundItems.Insert(--index, item);
			//	listBoxItems.SelectedIndex = index;
			//}
		}

		private void toolStripButtonMoveDown_Click(object sender, EventArgs e)
		{
			//var index = listBoxItems.SelectedIndex;
			//var lastIndex = BoundItems.Count - 1;

			//if( index > -1 && index < lastIndex )
			//{
			//	var item = BoundItems[index];

			//	BoundItems.RemoveAt(index);

			//	index++;

			//	BoundItems.Insert(index, item);
			//	listBoxItems.SelectedIndex = index;
			//}
		}

		public void OpenFile()
		{
			if( IsTreeDirty )
			{
				var confirm = MessageBox.Show("There are unsaved changes present. This will replace the current data. Proceed?",
												"Unsaved Data",
												MessageBoxButtons.OKCancel,
												MessageBoxIcon.Warning);

				if( confirm == DialogResult.Cancel )
					return;
			}

			var result = OpenFileDialog.ShowDialog();

			if( result == DialogResult.OK )
			{
				try
				{
					Clear();
					OnFileLoad(OpenFileDialog.FileName);
					//toolStripLabelFileName.Text = OpenFileDialog.FileName;
					IsTreeDirty = false;
					CurrentFilePath = OpenFileDialog.FileName;
				}
				catch( Exception ex )
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void toolStripButtonFileOpen_Click(object sender, EventArgs e)
		{
			OpenFile();
		}

		public void SaveFileAs()
		{
			var result = SaveFileDialog.ShowDialog();

			if( result == DialogResult.OK )
			{
				try
				{
					OnFileSave(SaveFileDialog.FileName);
					//toolStripLabelFileName.Text = SaveFileDialog.FileName;
					IsTreeDirty = false;
					CurrentFilePath = SaveFileDialog.FileName;
				}
				catch( Exception ex )
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void toolStripButtonFileSaveAs_Click(object sender, EventArgs e)
		{
			SaveFileAs();
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
						IsTreeDirty = true;
					}
					else
					{
						targetNode.InsertAfter(draggedNode);
						IsTreeDirty = true;
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
				IsTreeDirty = true;
			}
		}

		private void toolStripButtonAddCategory_Click(object sender, EventArgs e)
		{
			var categoryModel = new CategoryModel();
			categoryModel.Name = "New Category";

			var node = new BoundTreeNode();
			node.BoundObject = categoryModel;

			treeViewItems.Nodes.Add(node);
			IsTreeDirty = true;
		}

		private void propertyGridItemEditor_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			IsTreeDirty = true;
		}
	}
}
