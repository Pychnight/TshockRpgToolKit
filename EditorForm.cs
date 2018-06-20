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
		public EditorForm()
		{
			InitializeComponent();

			var invasionsEditor = (ObjectEditor)tabControl1.TabPages[0].Controls[0];
			invasionsEditor.OpenFileDialog = openFileDialogNpcs;
			invasionsEditor.SaveFileDialog = saveFileDialogNpcs;

			var npcsEditor = (ObjectEditor)tabControl1.TabPages[1].Controls[0];
			npcsEditor.OpenFileDialog = openFileDialogNpcs;
			npcsEditor.SaveFileDialog = saveFileDialogNpcs;

			var projectilesEditor = (ObjectEditor)tabControl1.TabPages[2].Controls[0];
			projectilesEditor.OpenFileDialog = openFileDialogProjectiles;
			projectilesEditor.SaveFileDialog = saveFileDialogProjectiles;

			//start on projectiles page for now...
			tabControl1.SelectedIndex = 0;
		}
		
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			var result = MessageBox.Show("This will delete all work, and cannot be undone. Continue?", "Continue exit?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

			if( result != DialogResult.OK )
			{
				e.Cancel = true;
			}
		}
	}
}
