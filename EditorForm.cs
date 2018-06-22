using CustomNpcsEdit.Controls;
using CustomNpcsEdit.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomNpcsEdit
{
	internal partial class EditorForm : Form
	{
		List<ObjectEditor> objectEditors;
		InvasionEditor invasionsEditor;
		NpcEditor npcsEditor;
		ProjectileEditor projectilesEditor;

		public EditorForm()
		{
			InitializeComponent();
			
			invasionsEditor = (InvasionEditor)tabControlMain.TabPages[0].Controls[0];
			invasionsEditor.OpenFileDialog = openFileDialogNpcs;
			invasionsEditor.SaveFileDialog = saveFileDialogNpcs;
			
			npcsEditor = (NpcEditor)tabControlMain.TabPages[1].Controls[0];
			npcsEditor.OpenFileDialog = openFileDialogNpcs;
			npcsEditor.SaveFileDialog = saveFileDialogNpcs;
			
			projectilesEditor = (ProjectileEditor)tabControlMain.TabPages[2].Controls[0];
			projectilesEditor.OpenFileDialog = openFileDialogProjectiles;
			projectilesEditor.SaveFileDialog = saveFileDialogProjectiles;

			objectEditors = new List<ObjectEditor>()
			{
				invasionsEditor,
				npcsEditor,
				projectilesEditor
			};

			foreach( var editor in objectEditors )
			{
				//refresh on property change
				editor.PropertyChanged += (s, a) =>
				{
					//...but only if its the currently selected tab.
					var selectedIndex = tabControlMain.SelectedIndex;

					if( s == tabControlMain.TabPages[selectedIndex].Controls[0] )
						refreshObjectEditorExternalDisplay(selectedIndex);
				};
			}
			
			//start on projectiles page for now...
			//tabControl1.SelectedIndex = 0;
		}
		
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			var unsavedData = objectEditors.FirstOrDefault(ed => ed.IsTreeDirty) != null;

			if(unsavedData)
			{
				var result = MessageBox.Show("There are unsaved changes present. Are you sure you want to exit?", "Unsaved Data", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

				if( result != DialogResult.OK )
				{
					e.Cancel = true;
				}
			}
		}

		private void refreshObjectEditorExternalDisplay(int selectedIndex)
		{
			var editor = objectEditors[selectedIndex];
			var value = editor.Caption;

			Text = $"CustomNpcsEdit - {value}";
		}

		private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			refreshObjectEditorExternalDisplay(tabControlMain.SelectedIndex);
		}
	}
}
