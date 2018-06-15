using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomNpcsEdit.Models;
using System.Collections;

namespace CustomNpcsEdit.Controls
{
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
		}
		
		private void toolStripButtonCopy_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;

			if( index > -1 && BoundItems!=null )
			{
				var copy = OnCopyItem(listBoxItems.SelectedItem);
				
				BoundItems.Insert(++index,copy);
			}
		}

		private void toolStripButtonDeleteItem_Click(object sender, EventArgs e)
		{
			var index = listBoxItems.SelectedIndex;

			if(index>-1 && BoundItems!=null)
			{
				BoundItems.RemoveAt(index);
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
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
}
