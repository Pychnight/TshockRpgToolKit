namespace CustomNpcsEdit
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
			this.tabPageNpcs = new System.Windows.Forms.TabPage();
			this.tabPageProjectiles = new System.Windows.Forms.TabPage();
			this.imageListTabIcons = new System.Windows.Forms.ImageList(this.components);
			this.openFileDialogProjectiles = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogProjectiles = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialogNpcs = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogNpcs = new System.Windows.Forms.SaveFileDialog();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.invasionEditor1 = new CustomNpcsEdit.Controls.InvasionEditor();
			this.npcEditor1 = new CustomNpcsEdit.Controls.NpcEditor();
			this.projectileEditor1 = new CustomNpcsEdit.Controls.ProjectileEditor();
			this.npcShopsEditor1 = new CustomNpcsEdit.Controls.NpcShopsEditor();
			this.menuStrip1.SuspendLayout();
			this.tabControlMain.SuspendLayout();
			this.tabPageInvasions.SuspendLayout();
			this.tabPageNpcs.SuspendLayout();
			this.tabPageProjectiles.SuspendLayout();
			this.tabPage1.SuspendLayout();
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
			this.saveToolStripMenuItem.Enabled = false;
			this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
			this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.saveToolStripMenuItem.Text = "&Save";
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
			this.tabControlMain.Controls.Add(this.tabPage1);
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
			this.tabPageInvasions.Controls.Add(this.invasionEditor1);
			this.tabPageInvasions.ImageIndex = 0;
			this.tabPageInvasions.Location = new System.Drawing.Point(4, 23);
			this.tabPageInvasions.Name = "tabPageInvasions";
			this.tabPageInvasions.Size = new System.Drawing.Size(1000, 608);
			this.tabPageInvasions.TabIndex = 0;
			this.tabPageInvasions.Text = "Invasions";
			this.tabPageInvasions.UseVisualStyleBackColor = true;
			// 
			// tabPageNpcs
			// 
			this.tabPageNpcs.Controls.Add(this.npcEditor1);
			this.tabPageNpcs.ImageIndex = 1;
			this.tabPageNpcs.Location = new System.Drawing.Point(4, 23);
			this.tabPageNpcs.Name = "tabPageNpcs";
			this.tabPageNpcs.Size = new System.Drawing.Size(1000, 608);
			this.tabPageNpcs.TabIndex = 1;
			this.tabPageNpcs.Text = "NPCs";
			this.tabPageNpcs.UseVisualStyleBackColor = true;
			// 
			// tabPageProjectiles
			// 
			this.tabPageProjectiles.Controls.Add(this.projectileEditor1);
			this.tabPageProjectiles.ImageIndex = 2;
			this.tabPageProjectiles.Location = new System.Drawing.Point(4, 23);
			this.tabPageProjectiles.Name = "tabPageProjectiles";
			this.tabPageProjectiles.Size = new System.Drawing.Size(1000, 608);
			this.tabPageProjectiles.TabIndex = 2;
			this.tabPageProjectiles.Text = "Projectiles";
			this.tabPageProjectiles.UseVisualStyleBackColor = true;
			// 
			// imageListTabIcons
			// 
			this.imageListTabIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTabIcons.ImageStream")));
			this.imageListTabIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListTabIcons.Images.SetKeyName(0, "Goblin-16.png");
			this.imageListTabIcons.Images.SetKeyName(1, "Green_Slime-16.png");
			this.imageListTabIcons.Images.SetKeyName(2, "Shuriken.png");
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
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.npcShopsEditor1);
			this.tabPage1.Location = new System.Drawing.Point(4, 23);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(1000, 608);
			this.tabPage1.TabIndex = 3;
			this.tabPage1.Text = "NpcShops";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// invasionEditor1
			// 
			this.invasionEditor1.CanAddCategory = true;
			this.invasionEditor1.CurrentFilePath = "";
			this.invasionEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.invasionEditor1.IsTreeDirty = false;
			this.invasionEditor1.Location = new System.Drawing.Point(0, 0);
			this.invasionEditor1.Name = "invasionEditor1";
			this.invasionEditor1.OpenFileDialog = null;
			this.invasionEditor1.SaveFileDialog = null;
			this.invasionEditor1.Size = new System.Drawing.Size(1000, 608);
			this.invasionEditor1.TabIndex = 0;
			// 
			// npcEditor1
			// 
			this.npcEditor1.CanAddCategory = true;
			this.npcEditor1.CurrentFilePath = "";
			this.npcEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.npcEditor1.IsTreeDirty = false;
			this.npcEditor1.Location = new System.Drawing.Point(0, 0);
			this.npcEditor1.Name = "npcEditor1";
			this.npcEditor1.OpenFileDialog = null;
			this.npcEditor1.SaveFileDialog = null;
			this.npcEditor1.Size = new System.Drawing.Size(1000, 608);
			this.npcEditor1.TabIndex = 0;
			// 
			// projectileEditor1
			// 
			this.projectileEditor1.CanAddCategory = true;
			this.projectileEditor1.CurrentFilePath = "";
			this.projectileEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.projectileEditor1.IsTreeDirty = false;
			this.projectileEditor1.Location = new System.Drawing.Point(0, 0);
			this.projectileEditor1.Name = "projectileEditor1";
			this.projectileEditor1.OpenFileDialog = null;
			this.projectileEditor1.SaveFileDialog = null;
			this.projectileEditor1.Size = new System.Drawing.Size(1000, 608);
			this.projectileEditor1.TabIndex = 0;
			// 
			// npcShopsEditor1
			// 
			this.npcShopsEditor1.CanAddCategory = true;
			this.npcShopsEditor1.CurrentFilePath = "";
			this.npcShopsEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.npcShopsEditor1.IsTreeDirty = false;
			this.npcShopsEditor1.Location = new System.Drawing.Point(3, 3);
			this.npcShopsEditor1.Name = "npcShopsEditor1";
			this.npcShopsEditor1.OpenFileDialog = null;
			this.npcShopsEditor1.SaveFileDialog = null;
			this.npcShopsEditor1.Size = new System.Drawing.Size(994, 602);
			this.npcShopsEditor1.TabIndex = 0;
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
			this.Text = "CustomNpcsEdit";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditorForm_FormClosing);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.tabControlMain.ResumeLayout(false);
			this.tabPageInvasions.ResumeLayout(false);
			this.tabPageNpcs.ResumeLayout(false);
			this.tabPageProjectiles.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
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
		private Controls.ProjectileEditor projectileEditor1;
		private Controls.NpcEditor npcEditor1;
		private System.Windows.Forms.OpenFileDialog openFileDialogNpcs;
		private System.Windows.Forms.SaveFileDialog saveFileDialogNpcs;
		private Controls.InvasionEditor invasionEditor1;
		private System.Windows.Forms.ImageList imageListTabIcons;
		private System.Windows.Forms.TabPage tabPage1;
		private Controls.NpcShopsEditor npcShopsEditor1;
	}
}

