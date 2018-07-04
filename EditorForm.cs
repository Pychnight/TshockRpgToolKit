using RpgToolsEditor.Controls;
using RpgToolsEditor.Models;
using RpgToolsEditor.Models.CustomNpcs;
using RpgToolsEditor.Models.CustomQuests;
using RpgToolsEditor.Models.Leveling;
using RpgToolsEditor.Models.NpcShops;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

		List<ObjectEditor> objectEditors;
		InvasionEditor invasionsEditor;
		//NpcEditor npcsEditor;
		//ProjectileEditor projectilesEditor;
		//NpcShopsEditor npcShopsEditor;
		//LevelingEditor levelingEditor;
		//QuestsEditor questsEditor;

		public EditorForm()
		{
			InitializeComponent();
			
			invasionsEditor = (InvasionEditor)tabControlMain.TabPages[0].Controls[0];
			invasionsEditor.OpenFileDialog = openFileDialogInvasions;
			invasionsEditor.SaveFileDialog = saveFileDialogInvasions;
			
			//npcsEditor = (NpcEditor)tabControlMain.TabPages[1].Controls[0];
			//npcsEditor.OpenFileDialog = openFileDialogNpcs;
			//npcsEditor.SaveFileDialog = saveFileDialogNpcs;
			
			//projectilesEditor = (ProjectileEditor)tabControlMain.TabPages[2].Controls[0];
			//projectilesEditor.OpenFileDialog = openFileDialogProjectiles;
			//projectilesEditor.SaveFileDialog = saveFileDialogProjectiles;

			//npcShopsEditor = (NpcShopsEditor)tabControlMain.TabPages[3].Controls[0];
			//npcShopsEditor.OpenFileDialog = openFileDialogNpcShop;
			//npcShopsEditor.SaveFileDialog = saveFileDialogNpcShop;
			//npcShopsEditor.CanAddCategory = false;
			//npcShopsEditor.SupportMultipleItems = false;
			
			//levelingEditor = (LevelingEditor)tabControlMain.TabPages[4].Controls[0];
			//levelingEditor.OpenFileDialog = openFileDialogLeveling;
			//levelingEditor.SaveFileDialog = saveFileDialogLeveling;
			//levelingEditor.CanAddCategory = false;
			//levelingEditor.SupportMultipleItems = false;

			//questsEditor = (QuestsEditor)tabControlMain.TabPages[5].Controls[0];
			//questsEditor.OpenFileDialog = openFileDialogQuests;
			//questsEditor.SaveFileDialog = saveFileDialogQuests;
			//questsEditor.CanAddCategory = false;
			//questsEditor.SupportMultipleItems = true;

			objectEditors = new List<ObjectEditor>()
			{
				invasionsEditor
				//npcsEditor
				//projectilesEditor
				//npcShopsEditor,
				//levelingEditor,
				//questsEditor
			};

			foreach( var editor in objectEditors )
			{
				//refresh on property change
				editor.PropertyChanged += (s, a) =>
				{
					//...but only if its the currently selected tab.
					var selectedIndex = tabControlMain.SelectedIndex;

					if( s == tabControlMain.TabPages[selectedIndex].Controls[0] )
					{
						refreshObjectEditorExternalDisplay(selectedIndex);
						refreshObjectEditorExternalControls(selectedIndex);
					}
				};
			}

			var npcsEditor = (ModelTreeEditor)tabControlMain.TabPages[1].Controls[0];
			npcsEditor.OpenFileDialog = openFileDialogNpcs;
			npcsEditor.SaveFileDialog = saveFileDialogNpcs;
			npcsEditor.ModelTree = new NpcsModelTree();

			var projectilesEditor = (ModelTreeEditor)tabControlMain.TabPages[2].Controls[0];
			projectilesEditor.OpenFileDialog = openFileDialogProjectiles;
			projectilesEditor.SaveFileDialog = saveFileDialogProjectiles;
			projectilesEditor.ModelTree = new ProjectilesModelTree();
			
			var npcShopsTreeEditor = (ModelTreeEditor)tabControlMain.TabPages[3].Controls[0];
			npcShopsTreeEditor.OpenFileDialog = openFileDialogNpcShop;
			npcShopsTreeEditor.SaveFileDialog = saveFileDialogNpcShop;
			npcShopsTreeEditor.ModelTree = new NpcShopsModelTree();

			var levelingTreeEditor = (ModelTreeEditor)tabControlMain.TabPages[4].Controls[0];
			levelingTreeEditor.OpenFileDialog = openFileDialogLeveling;
			levelingTreeEditor.SaveFileDialog = saveFileDialogLeveling;
			levelingTreeEditor.ModelTree = new ClassModelTree();

			var questTreeEditor = (ModelTreeEditor)tabControlMain.TabPages[5].Controls[0];
			questTreeEditor.OpenFileDialog = openFileDialogQuests;
			questTreeEditor.SaveFileDialog = saveFileDialogQuests;
			questTreeEditor.ModelTree = new QuestInfoModelTree();
			
			//start on projectiles page for now...
			tabControlMain.SelectedIndex = 1;
		}

		private ObjectEditor getSelectedEditor()
		{
			var editor = objectEditors[tabControlMain.SelectedIndex];
			return editor;
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
			if( selectedIndex >= 1 )
				return;

			var editor = objectEditors[selectedIndex];
			var value = editor.Caption;
			var hasFilePath = string.IsNullOrWhiteSpace(value);
			var dirty = editor.IsTreeDirty ? "*" : "";

			if( hasFilePath )
				Text = $"{AppName}";
			else
				Text = $"{AppName} - {value}{dirty}"; 
		}

		private void refreshObjectEditorExternalControls(int selectedIndex)
		{
			if( selectedIndex >= 1 )
				return;

			var editor = objectEditors[selectedIndex];
			
			//Making this a no-op now, it makes no sense. Save will always fallback to SaveAs if needed...
			//saveToolStripMenuItem.Enabled = editor.HasCurrentFilePath;
		}

		private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedIndex = tabControlMain.SelectedIndex;

			refreshObjectEditorExternalDisplay(selectedIndex);
			refreshObjectEditorExternalControls(selectedIndex);
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
			var editor = getSelectedEditor();
			editor.NewFile();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var editor = getSelectedEditor();
			editor.OpenFile();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var editor = getSelectedEditor();
			editor.SaveFile();
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var editor = getSelectedEditor();
			editor.SaveFileAs();
		}
	}
}
