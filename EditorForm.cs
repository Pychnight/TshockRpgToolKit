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
	public partial class EditorForm : Form
	{
		public EditorForm()
		{
			InitializeComponent();

			var projectilesEditor = (ObjectEditor)tabControl1.TabPages[2].Controls[0];
			projectilesEditor.OpenFileDialog = openFileDialogProjectiles;
			projectilesEditor.SaveFileDialog = saveFileDialogProjectiles;

			//start on projectiles page for now...
			tabControl1.SelectedIndex = 2;
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
