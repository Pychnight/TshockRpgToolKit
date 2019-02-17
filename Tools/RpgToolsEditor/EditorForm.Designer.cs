namespace RpgToolsEditor
{
	partial class EditorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorForm));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.tabControlMain = new System.Windows.Forms.TabControl();
			this.tabPageInvasions = new System.Windows.Forms.TabPage();
			this.modelTreeEditor6 = new RpgToolsEditor.Controls.ModelTreeEditor();
			this.tabPageNpcs = new System.Windows.Forms.TabPage();
			this.modelTreeEditor5 = new RpgToolsEditor.Controls.ModelTreeEditor();
			this.tabPageProjectiles = new System.Windows.Forms.TabPage();
			this.modelTreeEditor4 = new RpgToolsEditor.Controls.ModelTreeEditor();
			this.tabPageNpcShops = new System.Windows.Forms.TabPage();
			this.modelTreeEditor2 = new RpgToolsEditor.Controls.ModelTreeEditor();
			this.tabPageLeveling = new System.Windows.Forms.TabPage();
			this.modelTreeEditor3 = new RpgToolsEditor.Controls.ModelTreeEditor();
			this.tabPageQuests = new System.Windows.Forms.TabPage();
			this.modelTreeEditor1 = new RpgToolsEditor.Controls.ModelTreeEditor();
			this.tabPageBanking = new System.Windows.Forms.TabPage();
			this.modelTreeEditorBanking = new RpgToolsEditor.Controls.ModelTreeEditor();
			this.imageListTabIcons = new System.Windows.Forms.ImageList(this.components);
			this.openFileDialogProjectiles = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogProjectiles = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialogNpcs = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogNpcs = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialogNpcShop = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogNpcShop = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialogLeveling = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogLeveling = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialogQuests = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogQuests = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialogInvasions = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogInvasions = new System.Windows.Forms.SaveFileDialog();
			this.imageListInvasions = new System.Windows.Forms.ImageList(this.components);
			this.imageListNpcs = new System.Windows.Forms.ImageList(this.components);
			this.imageListProjectiles = new System.Windows.Forms.ImageList(this.components);
			this.imageListQuests = new System.Windows.Forms.ImageList(this.components);
			this.imageListNpcShops = new System.Windows.Forms.ImageList(this.components);
			this.imageListLeveling = new System.Windows.Forms.ImageList(this.components);
			this.openFileDialogBanking = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogBanking = new System.Windows.Forms.SaveFileDialog();
			this.imageListBanking = new System.Windows.Forms.ImageList(this.components);
			this.menuStrip1.SuspendLayout();
			this.tabControlMain.SuspendLayout();
			this.tabPageInvasions.SuspendLayout();
			this.tabPageNpcs.SuspendLayout();
			this.tabPageProjectiles.SuspendLayout();
			this.tabPageNpcShops.SuspendLayout();
			this.tabPageLeveling.SuspendLayout();
			this.tabPageQuests.SuspendLayout();
			this.tabPageBanking.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1008, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
			this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.newToolStripMenuItem.Text = "&New";
			this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
			this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// toolStripSeparator
			// 
			this.toolStripSeparator.Name = "toolStripSeparator";
			this.toolStripSeparator.Size = new System.Drawing.Size(143, 6);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
			this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.saveAsToolStripMenuItem.Text = "Save &As";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(143, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator3,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator4,
            this.selectAllToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "&Edit";
			// 
			// undoToolStripMenuItem
			// 
			this.undoToolStripMenuItem.Enabled = false;
			this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
			this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
			this.undoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.undoToolStripMenuItem.Text = "&Undo";
			// 
			// redoToolStripMenuItem
			// 
			this.redoToolStripMenuItem.Enabled = false;
			this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
			this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.redoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.redoToolStripMenuItem.Text = "&Redo";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(141, 6);
			// 
			// cutToolStripMenuItem
			// 
			this.cutToolStripMenuItem.Enabled = false;
			this.cutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem.Image")));
			this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.cutToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.cutToolStripMenuItem.Text = "Cu&t";
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Enabled = false;
			this.copyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem.Image")));
			this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.copyToolStripMenuItem.Text = "&Copy";
			// 
			// pasteToolStripMenuItem
			// 
			this.pasteToolStripMenuItem.Enabled = false;
			this.pasteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem.Image")));
			this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.pasteToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.pasteToolStripMenuItem.Text = "&Paste";
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(141, 6);
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Enabled = false;
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.selectAllToolStripMenuItem.Text = "Select &All";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(113, 6);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
			this.aboutToolStripMenuItem.Text = "&About...";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Location = new System.Drawing.Point(0, 659);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(1008, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// tabControlMain
			// 
			this.tabControlMain.Controls.Add(this.tabPageInvasions);
			this.tabControlMain.Controls.Add(this.tabPageNpcs);
			this.tabControlMain.Controls.Add(this.tabPageProjectiles);
			this.tabControlMain.Controls.Add(this.tabPageNpcShops);
			this.tabControlMain.Controls.Add(this.tabPageLeveling);
			this.tabControlMain.Controls.Add(this.tabPageQuests);
			this.tabControlMain.Controls.Add(this.tabPageBanking);
			this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlMain.ImageList = this.imageListTabIcons;
			this.tabControlMain.Location = new System.Drawing.Point(0, 24);
			this.tabControlMain.Name = "tabControlMain";
			this.tabControlMain.SelectedIndex = 0;
			this.tabControlMain.Size = new System.Drawing.Size(1008, 635);
			this.tabControlMain.TabIndex = 3;
			this.tabControlMain.SelectedIndexChanged += new System.EventHandler(this.tabControlMain_SelectedIndexChanged);
			this.tabControlMain.TabIndexChanged += new System.EventHandler(this.tabControlMain_SelectedIndexChanged);
			// 
			// tabPageInvasions
			// 
			this.tabPageInvasions.Controls.Add(this.modelTreeEditor6);
			this.tabPageInvasions.ImageIndex = 0;
			this.tabPageInvasions.Location = new System.Drawing.Point(4, 23);
			this.tabPageInvasions.Name = "tabPageInvasions";
			this.tabPageInvasions.Size = new System.Drawing.Size(1000, 608);
			this.tabPageInvasions.TabIndex = 0;
			this.tabPageInvasions.Text = "Invasions";
			this.tabPageInvasions.UseVisualStyleBackColor = true;
			// 
			// modelTreeEditor6
			// 
			this.modelTreeEditor6.CurrentFilePath = "";
			this.modelTreeEditor6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.modelTreeEditor6.IsTreeDirty = false;
			this.modelTreeEditor6.Location = new System.Drawing.Point(0, 0);
			this.modelTreeEditor6.ModelTree = null;
			this.modelTreeEditor6.Name = "modelTreeEditor6";
			this.modelTreeEditor6.OpenFileDialog = null;
			this.modelTreeEditor6.SaveFileDialog = null;
			this.modelTreeEditor6.Size = new System.Drawing.Size(1000, 608);
			this.modelTreeEditor6.TabIndex = 0;
			this.modelTreeEditor6.UseSingleFolderTreeNode = false;
			// 
			// tabPageNpcs
			// 
			this.tabPageNpcs.Controls.Add(this.modelTreeEditor5);
			this.tabPageNpcs.ImageIndex = 1;
			this.tabPageNpcs.Location = new System.Drawing.Point(4, 23);
			this.tabPageNpcs.Name = "tabPageNpcs";
			this.tabPageNpcs.Size = new System.Drawing.Size(1000, 608);
			this.tabPageNpcs.TabIndex = 1;
			this.tabPageNpcs.Text = "NPCs";
			this.tabPageNpcs.UseVisualStyleBackColor = true;
			// 
			// modelTreeEditor5
			// 
			this.modelTreeEditor5.CurrentFilePath = "";
			this.modelTreeEditor5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.modelTreeEditor5.IsTreeDirty = false;
			this.modelTreeEditor5.Location = new System.Drawing.Point(0, 0);
			this.modelTreeEditor5.ModelTree = null;
			this.modelTreeEditor5.Name = "modelTreeEditor5";
			this.modelTreeEditor5.OpenFileDialog = null;
			this.modelTreeEditor5.SaveFileDialog = null;
			this.modelTreeEditor5.Size = new System.Drawing.Size(1000, 608);
			this.modelTreeEditor5.TabIndex = 0;
			this.modelTreeEditor5.UseSingleFolderTreeNode = false;
			// 
			// tabPageProjectiles
			// 
			this.tabPageProjectiles.Controls.Add(this.modelTreeEditor4);
			this.tabPageProjectiles.ImageIndex = 2;
			this.tabPageProjectiles.Location = new System.Drawing.Point(4, 23);
			this.tabPageProjectiles.Name = "tabPageProjectiles";
			this.tabPageProjectiles.Size = new System.Drawing.Size(1000, 608);
			this.tabPageProjectiles.TabIndex = 2;
			this.tabPageProjectiles.Text = "Projectiles";
			this.tabPageProjectiles.UseVisualStyleBackColor = true;
			// 
			// modelTreeEditor4
			// 
			this.modelTreeEditor4.CurrentFilePath = "";
			this.modelTreeEditor4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.modelTreeEditor4.IsTreeDirty = false;
			this.modelTreeEditor4.Location = new System.Drawing.Point(0, 0);
			this.modelTreeEditor4.ModelTree = null;
			this.modelTreeEditor4.Name = "modelTreeEditor4";
			this.modelTreeEditor4.OpenFileDialog = null;
			this.modelTreeEditor4.SaveFileDialog = null;
			this.modelTreeEditor4.Size = new System.Drawing.Size(1000, 608);
			this.modelTreeEditor4.TabIndex = 0;
			this.modelTreeEditor4.UseSingleFolderTreeNode = false;
			// 
			// tabPageNpcShops
			// 
			this.tabPageNpcShops.Controls.Add(this.modelTreeEditor2);
			this.tabPageNpcShops.ImageIndex = 3;
			this.tabPageNpcShops.Location = new System.Drawing.Point(4, 23);
			this.tabPageNpcShops.Name = "tabPageNpcShops";
			this.tabPageNpcShops.Size = new System.Drawing.Size(1000, 608);
			this.tabPageNpcShops.TabIndex = 7;
			this.tabPageNpcShops.Text = "NpcShops";
			this.tabPageNpcShops.UseVisualStyleBackColor = true;
			// 
			// modelTreeEditor2
			// 
			this.modelTreeEditor2.CurrentFilePath = "";
			this.modelTreeEditor2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.modelTreeEditor2.IsTreeDirty = false;
			this.modelTreeEditor2.Location = new System.Drawing.Point(0, 0);
			this.modelTreeEditor2.ModelTree = null;
			this.modelTreeEditor2.Name = "modelTreeEditor2";
			this.modelTreeEditor2.OpenFileDialog = null;
			this.modelTreeEditor2.SaveFileDialog = null;
			this.modelTreeEditor2.Size = new System.Drawing.Size(1000, 608);
			this.modelTreeEditor2.TabIndex = 0;
			this.modelTreeEditor2.UseSingleFolderTreeNode = false;
			// 
			// tabPageLeveling
			// 
			this.tabPageLeveling.Controls.Add(this.modelTreeEditor3);
			this.tabPageLeveling.ImageIndex = 4;
			this.tabPageLeveling.Location = new System.Drawing.Point(4, 23);
			this.tabPageLeveling.Name = "tabPageLeveling";
			this.tabPageLeveling.Size = new System.Drawing.Size(1000, 608);
			this.tabPageLeveling.TabIndex = 8;
			this.tabPageLeveling.Text = "Leveling";
			this.tabPageLeveling.UseVisualStyleBackColor = true;
			// 
			// modelTreeEditor3
			// 
			this.modelTreeEditor3.CurrentFilePath = "";
			this.modelTreeEditor3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.modelTreeEditor3.IsTreeDirty = false;
			this.modelTreeEditor3.Location = new System.Drawing.Point(0, 0);
			this.modelTreeEditor3.ModelTree = null;
			this.modelTreeEditor3.Name = "modelTreeEditor3";
			this.modelTreeEditor3.OpenFileDialog = null;
			this.modelTreeEditor3.SaveFileDialog = null;
			this.modelTreeEditor3.Size = new System.Drawing.Size(1000, 608);
			this.modelTreeEditor3.TabIndex = 0;
			this.modelTreeEditor3.UseSingleFolderTreeNode = false;
			// 
			// tabPageQuests
			// 
			this.tabPageQuests.Controls.Add(this.modelTreeEditor1);
			this.tabPageQuests.ImageIndex = 5;
			this.tabPageQuests.Location = new System.Drawing.Point(4, 23);
			this.tabPageQuests.Name = "tabPageQuests";
			this.tabPageQuests.Size = new System.Drawing.Size(1000, 608);
			this.tabPageQuests.TabIndex = 6;
			this.tabPageQuests.Text = "Quests";
			this.tabPageQuests.UseVisualStyleBackColor = true;
			// 
			// modelTreeEditor1
			// 
			this.modelTreeEditor1.CurrentFilePath = "";
			this.modelTreeEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.modelTreeEditor1.IsTreeDirty = false;
			this.modelTreeEditor1.Location = new System.Drawing.Point(0, 0);
			this.modelTreeEditor1.ModelTree = null;
			this.modelTreeEditor1.Name = "modelTreeEditor1";
			this.modelTreeEditor1.OpenFileDialog = null;
			this.modelTreeEditor1.SaveFileDialog = null;
			this.modelTreeEditor1.Size = new System.Drawing.Size(1000, 608);
			this.modelTreeEditor1.TabIndex = 0;
			this.modelTreeEditor1.UseSingleFolderTreeNode = false;
			// 
			// tabPageBanking
			// 
			this.tabPageBanking.Controls.Add(this.modelTreeEditorBanking);
			this.tabPageBanking.ImageIndex = 6;
			this.tabPageBanking.Location = new System.Drawing.Point(4, 23);
			this.tabPageBanking.Name = "tabPageBanking";
			this.tabPageBanking.Size = new System.Drawing.Size(1000, 608);
			this.tabPageBanking.TabIndex = 9;
			this.tabPageBanking.Text = "Banking";
			this.tabPageBanking.UseVisualStyleBackColor = true;
			// 
			// modelTreeEditorBanking
			// 
			this.modelTreeEditorBanking.CurrentFilePath = "";
			this.modelTreeEditorBanking.Dock = System.Windows.Forms.DockStyle.Fill;
			this.modelTreeEditorBanking.IsTreeDirty = false;
			this.modelTreeEditorBanking.Location = new System.Drawing.Point(0, 0);
			this.modelTreeEditorBanking.ModelTree = null;
			this.modelTreeEditorBanking.Name = "modelTreeEditorBanking";
			this.modelTreeEditorBanking.OpenFileDialog = null;
			this.modelTreeEditorBanking.SaveFileDialog = null;
			this.modelTreeEditorBanking.Size = new System.Drawing.Size(1000, 608);
			this.modelTreeEditorBanking.TabIndex = 0;
			this.modelTreeEditorBanking.UseSingleFolderTreeNode = false;
			// 
			// imageListTabIcons
			// 
			this.imageListTabIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTabIcons.ImageStream")));
			this.imageListTabIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListTabIcons.Images.SetKeyName(0, "Goblin-16.png");
			this.imageListTabIcons.Images.SetKeyName(1, "Green_Slime-16.png");
			this.imageListTabIcons.Images.SetKeyName(2, "Shuriken.png");
			this.imageListTabIcons.Images.SetKeyName(3, "Merchant-16.png");
			this.imageListTabIcons.Images.SetKeyName(4, "Wooden_Sword-16.png");
			this.imageListTabIcons.Images.SetKeyName(5, "Skeletron_Head-16.png");
			this.imageListTabIcons.Images.SetKeyName(6, "Gold_Coin-16.png");
			// 
			// openFileDialogProjectiles
			// 
			this.openFileDialogProjectiles.Filter = "Json files|*.json|All files|*.*";
			this.openFileDialogProjectiles.Title = "Open Custom Projectiles";
			// 
			// saveFileDialogProjectiles
			// 
			this.saveFileDialogProjectiles.Filter = "Json files|*.json|All files|*.*";
			this.saveFileDialogProjectiles.SupportMultiDottedExtensions = true;
			this.saveFileDialogProjectiles.Title = "Save Custom Projectiles";
			// 
			// openFileDialogNpcs
			// 
			this.openFileDialogNpcs.Filter = "Json files|*.json|All files|*.*";
			this.openFileDialogNpcs.Title = "Open Custom NPCs";
			// 
			// saveFileDialogNpcs
			// 
			this.saveFileDialogNpcs.Filter = "Json files|*.json|All files|*.*";
			this.saveFileDialogNpcs.SupportMultiDottedExtensions = true;
			this.saveFileDialogNpcs.Title = "Save Custom NPCs";
			// 
			// openFileDialogNpcShop
			// 
			this.openFileDialogNpcShop.CheckFileExists = false;
			this.openFileDialogNpcShop.CheckPathExists = false;
			this.openFileDialogNpcShop.Filter = "Shop files|*.shop|Json files|*.json|All files|*.*";
			this.openFileDialogNpcShop.Title = "Open Npc Shop";
			// 
			// saveFileDialogNpcShop
			// 
			this.saveFileDialogNpcShop.AddExtension = false;
			this.saveFileDialogNpcShop.CheckPathExists = false;
			this.saveFileDialogNpcShop.Filter = "Shop files|*.shop|Json files|*.json|All files|*.*";
			this.saveFileDialogNpcShop.SupportMultiDottedExtensions = true;
			this.saveFileDialogNpcShop.Title = "Save Npc Shop";
			this.saveFileDialogNpcShop.ValidateNames = false;
			// 
			// openFileDialogLeveling
			// 
			this.openFileDialogLeveling.Filter = "Class files|*.class|Json files|*.json|All files|*.*";
			this.openFileDialogLeveling.Title = "Open Class";
			// 
			// saveFileDialogLeveling
			// 
			this.saveFileDialogLeveling.Filter = "Class files|*.class|Json files|*.json|All files|*.*";
			this.saveFileDialogLeveling.SupportMultiDottedExtensions = true;
			this.saveFileDialogLeveling.Title = "Save Class";
			// 
			// openFileDialogQuests
			// 
			this.openFileDialogQuests.Filter = "Json files|*.json|All files|*.*";
			this.openFileDialogQuests.Title = "Open Quests";
			// 
			// saveFileDialogQuests
			// 
			this.saveFileDialogQuests.Filter = "Json files|*.json|All files|*.*";
			this.saveFileDialogQuests.SupportMultiDottedExtensions = true;
			this.saveFileDialogQuests.Title = "Save Quests";
			// 
			// openFileDialogInvasions
			// 
			this.openFileDialogInvasions.Filter = "Json files|*.json|All files|*.*";
			this.openFileDialogInvasions.Title = "Open Invasions";
			// 
			// saveFileDialogInvasions
			// 
			this.saveFileDialogInvasions.Filter = "Json files|*.json|All files|*.*";
			this.saveFileDialogInvasions.SupportMultiDottedExtensions = true;
			this.saveFileDialogInvasions.Title = "Save Invasions";
			// 
			// imageListInvasions
			// 
			this.imageListInvasions.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListInvasions.ImageStream")));
			this.imageListInvasions.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListInvasions.Images.SetKeyName(0, "Goblin-16.png");
			this.imageListInvasions.Images.SetKeyName(1, "triangle_red.png");
			this.imageListInvasions.Images.SetKeyName(2, "floppy_35inch_blue.png");
			this.imageListInvasions.Images.SetKeyName(3, "select_none.png");
			this.imageListInvasions.Images.SetKeyName(4, "ellipse_blue.png");
			// 
			// imageListNpcs
			// 
			this.imageListNpcs.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListNpcs.ImageStream")));
			this.imageListNpcs.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListNpcs.Images.SetKeyName(0, "Green_Slime-16.png");
			this.imageListNpcs.Images.SetKeyName(1, "triangle_red.png");
			this.imageListNpcs.Images.SetKeyName(2, "floppy_35inch_blue.png");
			this.imageListNpcs.Images.SetKeyName(3, "key_golden.png");
			this.imageListNpcs.Images.SetKeyName(4, "ellipse_blue.png");
			this.imageListNpcs.Images.SetKeyName(5, "select_none.png");
			// 
			// imageListProjectiles
			// 
			this.imageListProjectiles.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListProjectiles.ImageStream")));
			this.imageListProjectiles.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListProjectiles.Images.SetKeyName(0, "Shuriken-16.png");
			this.imageListProjectiles.Images.SetKeyName(1, "triangle_red.png");
			this.imageListProjectiles.Images.SetKeyName(2, "floppy_35inch_blue.png");
			this.imageListProjectiles.Images.SetKeyName(3, "ellipse_blue.png");
			// 
			// imageListQuests
			// 
			this.imageListQuests.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListQuests.ImageStream")));
			this.imageListQuests.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListQuests.Images.SetKeyName(0, "Skeletron_Head-16.png");
			this.imageListQuests.Images.SetKeyName(1, "triangle_red.png");
			this.imageListQuests.Images.SetKeyName(2, "floppy_35inch_blue.png");
			this.imageListQuests.Images.SetKeyName(3, "ellipse_blue.png");
			// 
			// imageListNpcShops
			// 
			this.imageListNpcShops.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListNpcShops.ImageStream")));
			this.imageListNpcShops.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListNpcShops.Images.SetKeyName(0, "Merchant-16.png");
			this.imageListNpcShops.Images.SetKeyName(1, "triangle_red.png");
			this.imageListNpcShops.Images.SetKeyName(2, "directory_closed.png");
			this.imageListNpcShops.Images.SetKeyName(3, "tag_blue.png");
			this.imageListNpcShops.Images.SetKeyName(4, "tag_red.png");
			this.imageListNpcShops.Images.SetKeyName(5, "CO2_add_blue.png");
			// 
			// imageListLeveling
			// 
			this.imageListLeveling.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLeveling.ImageStream")));
			this.imageListLeveling.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListLeveling.Images.SetKeyName(0, "Wooden_Sword-16.png");
			this.imageListLeveling.Images.SetKeyName(1, "triangle_red.png");
			this.imageListLeveling.Images.SetKeyName(2, "directory_closed.png");
			this.imageListLeveling.Images.SetKeyName(3, "shield_blue.png");
			// 
			// openFileDialogBanking
			// 
			this.openFileDialogBanking.CheckFileExists = false;
			this.openFileDialogBanking.CheckPathExists = false;
			this.openFileDialogBanking.Filter = "Currency files|*.currency|Json files|*.json|All files|*.*";
			this.openFileDialogBanking.Title = "Open Currency";
			// 
			// saveFileDialogBanking
			// 
			this.saveFileDialogBanking.AddExtension = false;
			this.saveFileDialogBanking.CheckPathExists = false;
			this.saveFileDialogBanking.Filter = "Currency files|*.currency|Json files|*.json|All files|*.*";
			this.saveFileDialogBanking.SupportMultiDottedExtensions = true;
			this.saveFileDialogBanking.Title = "Save Currency";
			this.saveFileDialogBanking.ValidateNames = false;
			// 
			// imageListBanking
			// 
			this.imageListBanking.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListBanking.ImageStream")));
			this.imageListBanking.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListBanking.Images.SetKeyName(0, "Gold_Coin-16.png");
			this.imageListBanking.Images.SetKeyName(1, "triangle_red.png");
			this.imageListBanking.Images.SetKeyName(2, "directory_closed.png");
			// 
			// EditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1008, 681);
			this.Controls.Add(this.tabControlMain);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "EditorForm";
			this.Text = "RPG Tools Editor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditorForm_FormClosing);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.tabControlMain.ResumeLayout(false);
			this.tabPageInvasions.ResumeLayout(false);
			this.tabPageNpcs.ResumeLayout(false);
			this.tabPageProjectiles.ResumeLayout(false);
			this.tabPageNpcShops.ResumeLayout(false);
			this.tabPageLeveling.ResumeLayout(false);
			this.tabPageQuests.ResumeLayout(false);
			this.tabPageBanking.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.TabControl tabControlMain;
		private System.Windows.Forms.TabPage tabPageInvasions;
		private System.Windows.Forms.TabPage tabPageNpcs;
		private System.Windows.Forms.TabPage tabPageProjectiles;
		private System.Windows.Forms.OpenFileDialog openFileDialogProjectiles;
		private System.Windows.Forms.SaveFileDialog saveFileDialogProjectiles;
		private System.Windows.Forms.OpenFileDialog openFileDialogNpcs;
		private System.Windows.Forms.SaveFileDialog saveFileDialogNpcs;
		private System.Windows.Forms.ImageList imageListTabIcons;
		private System.Windows.Forms.OpenFileDialog openFileDialogNpcShop;
		private System.Windows.Forms.SaveFileDialog saveFileDialogNpcShop;
		private System.Windows.Forms.OpenFileDialog openFileDialogLeveling;
		private System.Windows.Forms.SaveFileDialog saveFileDialogLeveling;
		private System.Windows.Forms.OpenFileDialog openFileDialogQuests;
		private System.Windows.Forms.SaveFileDialog saveFileDialogQuests;
		private System.Windows.Forms.OpenFileDialog openFileDialogInvasions;
		private System.Windows.Forms.SaveFileDialog saveFileDialogInvasions;
		private System.Windows.Forms.TabPage tabPageQuests;
		private Controls.ModelTreeEditor modelTreeEditor1;
		private System.Windows.Forms.TabPage tabPageNpcShops;
		private Controls.ModelTreeEditor modelTreeEditor2;
		private System.Windows.Forms.TabPage tabPageLeveling;
		private Controls.ModelTreeEditor modelTreeEditor3;
		private Controls.ModelTreeEditor modelTreeEditor4;
		private Controls.ModelTreeEditor modelTreeEditor5;
		private Controls.ModelTreeEditor modelTreeEditor6;
		private System.Windows.Forms.ImageList imageListInvasions;
		private System.Windows.Forms.ImageList imageListNpcs;
		private System.Windows.Forms.ImageList imageListProjectiles;
		private System.Windows.Forms.ImageList imageListQuests;
		private System.Windows.Forms.ImageList imageListNpcShops;
		private System.Windows.Forms.ImageList imageListLeveling;
		private System.Windows.Forms.TabPage tabPageBanking;
		private Controls.ModelTreeEditor modelTreeEditorBanking;
		private System.Windows.Forms.OpenFileDialog openFileDialogBanking;
		private System.Windows.Forms.SaveFileDialog saveFileDialogBanking;
		private System.Windows.Forms.ImageList imageListBanking;
	}
}

