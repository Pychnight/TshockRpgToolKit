using RpgToolsEditor.Controls;
using RpgToolsEditor.Models;
using RpgToolsEditor.Models.Banking;
using RpgToolsEditor.Models.CustomNpcs;
using RpgToolsEditor.Models.CustomQuests;
using RpgToolsEditor.Models.Leveling;
using RpgToolsEditor.Models.NpcShops;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgToolsEditor
{
	internal partial class EditorForm : Form
	{
		readonly string AppName = "RPG Tools Editor";

		public EditorForm()
		{
			InitializeComponent();
			
			var invasionsEditor = (ModelTreeEditor)tabControlMain.TabPages[0].Controls[0];
			invasionsEditor.OpenFileDialog = openFileDialogInvasions;
			invasionsEditor.SaveFileDialog = saveFileDialogInvasions;
			invasionsEditor.ModelTree = new InvasionModelTree();
			invasionsEditor.AddExtendedItemControls(new CategoryItemControls<Invasion, InvasionTreeNode>());
			invasionsEditor.ItemImageList = imageListInvasions;

			var npcsEditor = (ModelTreeEditor)tabControlMain.TabPages[1].Controls[0];
			npcsEditor.OpenFileDialog = openFileDialogNpcs;
			npcsEditor.SaveFileDialog = saveFileDialogNpcs;
			npcsEditor.ModelTree = new NpcsModelTree();
			npcsEditor.AddExtendedItemControls(new CategoryItemControls<Npc,NpcTreeNode>());
			npcsEditor.ItemImageList = imageListNpcs;

			var projectilesEditor = (ModelTreeEditor)tabControlMain.TabPages[2].Controls[0];
			projectilesEditor.OpenFileDialog = openFileDialogProjectiles;
			projectilesEditor.SaveFileDialog = saveFileDialogProjectiles;
			projectilesEditor.ModelTree = new ProjectilesModelTree();
			projectilesEditor.AddExtendedItemControls(new CategoryItemControls<Projectile, ProjectileTreeNode>());
			projectilesEditor.ItemImageList = imageListProjectiles;

			var npcShopsTreeEditor = (ModelTreeEditor)tabControlMain.TabPages[3].Controls[0];
			npcShopsTreeEditor.OpenFileDialog = openFileDialogNpcShop;
			npcShopsTreeEditor.SaveFileDialog = saveFileDialogNpcShop;
			npcShopsTreeEditor.ModelTree = new NpcShopsModelTree();
			npcShopsTreeEditor.ItemImageList = imageListNpcShops;
			npcShopsTreeEditor.UseSingleFolderTreeNode = true;//force special mode with root folder.

			var levelingTreeEditor = (ModelTreeEditor)tabControlMain.TabPages[4].Controls[0];
			levelingTreeEditor.OpenFileDialog = openFileDialogLeveling;
			levelingTreeEditor.SaveFileDialog = saveFileDialogLeveling;
			levelingTreeEditor.ModelTree = new ClassModelTree();
			levelingTreeEditor.ItemImageList = imageListLeveling;
			levelingTreeEditor.UseSingleFolderTreeNode = true;//force special mode with root folder.

			var questTreeEditor = (ModelTreeEditor)tabControlMain.TabPages[5].Controls[0];
			questTreeEditor.OpenFileDialog = openFileDialogQuests;
			questTreeEditor.SaveFileDialog = saveFileDialogQuests;
			questTreeEditor.ModelTree = new QuestInfoModelTree();
			questTreeEditor.ItemImageList = imageListQuests;

			var bankingTreeEditor = (ModelTreeEditor)tabControlMain.TabPages[6].Controls[0];
			//bankingTreeEditor.OpenFileDialog = openFileDialogQuests;
			//bankingTreeEditor.SaveFileDialog = saveFileDialogQuests;
			bankingTreeEditor.ModelTree = new BankingModelTree();
			bankingTreeEditor.ItemImageList = imageListQuests;
			bankingTreeEditor.UseSingleFolderTreeNode = true;//force special mode with root folder.

			//start on this page during development...
			//tabControlMain.SelectedIndex = 0;

			//handle property changed, in order to update filepath and dirty status...
			foreach( var editor in enumerateModelTreeEditors() )
			{
				//refresh on property change
				editor.PropertyChanged += (s, a) =>
				{
					//...but only if its the currently selected tab.
					var selectedIndex = tabControlMain.SelectedIndex;

					if( s == tabControlMain.TabPages[selectedIndex].Controls[0] )
					{
						refreshObjectEditorExternalDisplay(selectedIndex);
					}
				};
			}
		}

		private ModelTreeEditor getModelTreeEditor(int index)
		{
			var page = tabControlMain.TabPages[index];
			var editor = page.Controls[0] as ModelTreeEditor;
			return editor;
		}

		private ModelTreeEditor getSelectedModelTreeEditor()
		{
			var editor = getModelTreeEditor(tabControlMain.SelectedIndex);
			return editor;
		}

		private IList<ModelTreeEditor> enumerateModelTreeEditors()
		{
			var editors = tabControlMain.TabPages.Cast<TabPage>()
												.Select(p => (ModelTreeEditor)p.Controls[0])
												.ToList();
			return editors;
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			var editors = enumerateModelTreeEditors();
			var unsavedData = editors.FirstOrDefault(ed => ed.IsTreeDirty) != null;

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
			var editor = getModelTreeEditor(selectedIndex);
			var value = editor.Caption;
			var hasFilePath = string.IsNullOrWhiteSpace(value);
			var dirty = editor.IsTreeDirty ? "*" : "";

			//HACK! try to clean up the display text, since its really using a folder path,
			//not a file path( lets chop off any file parts ) 
			if( editor.UseSingleFolderTreeNode)
			{
				if(!string.IsNullOrWhiteSpace(value) && !Directory.Exists(value))
				{
					//lets assume this is file path then, so get the directory name
					value = Path.GetDirectoryName(value);
				}
			}

			if( hasFilePath )
				Text = $"{AppName}";
			else
				Text = $"{AppName} - {value}{dirty}"; 
		}

		private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedIndex = tabControlMain.SelectedIndex;

			refreshObjectEditorExternalDisplay(selectedIndex);
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var version = Assembly.GetExecutingAssembly().GetName().Version;
			
			MessageBox.Show($"{AppName} v{version}",
							"About",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information);
		}

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var editor = getSelectedModelTreeEditor();
			editor.NewFile();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var editor = getSelectedModelTreeEditor();
			editor.OpenFile();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var editor = getSelectedModelTreeEditor();
			editor.SaveFile();
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var editor = getSelectedModelTreeEditor();
			editor.SaveFileAs();
		}
	}
}
