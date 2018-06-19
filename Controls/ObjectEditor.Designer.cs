namespace CustomNpcsEdit.Controls
{
	partial class ObjectEditor
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("SuperSlime");
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("EvilBunny");
			System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Skeleton Warrior");
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Skeletal Mage");
			System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Skeleton King");
			System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Skeleton King Omega");
			System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Category - Quest Npcs", 1, 1, new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6});
			System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Wimpy Demon");
			System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Rabid Vulture");
			System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Npcs.json", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode7,
            treeNode8,
            treeNode9});
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectEditor));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.listBoxItems = new System.Windows.Forms.ListBox();
			this.toolStripListControl = new System.Windows.Forms.ToolStrip();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonAddItem = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonCopy = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDeleteItem = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonMoveUp = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonMoveDown = new System.Windows.Forms.ToolStripButton();
			this.propertyGridItemEditor = new System.Windows.Forms.PropertyGrid();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonNewFile = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonFileOpen = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonFileSaveAs = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripLabelFileName = new System.Windows.Forms.ToolStripLabel();
			this.treeViewItems = new System.Windows.Forms.TreeView();
			this.imageListTreeView = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.toolStripListControl.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 25);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeViewItems);
			this.splitContainer1.Panel1.Controls.Add(this.listBoxItems);
			this.splitContainer1.Panel1.Controls.Add(this.toolStripListControl);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.propertyGridItemEditor);
			this.splitContainer1.Size = new System.Drawing.Size(500, 391);
			this.splitContainer1.SplitterDistance = 244;
			this.splitContainer1.TabIndex = 0;
			// 
			// listBoxItems
			// 
			this.listBoxItems.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.listBoxItems.FormattingEnabled = true;
			this.listBoxItems.Location = new System.Drawing.Point(24, 322);
			this.listBoxItems.Name = "listBoxItems";
			this.listBoxItems.Size = new System.Drawing.Size(220, 69);
			this.listBoxItems.TabIndex = 0;
			this.listBoxItems.SelectedValueChanged += new System.EventHandler(this.listBoxItems_SelectedValueChanged);
			// 
			// toolStripListControl
			// 
			this.toolStripListControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.toolStripListControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripListControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator5,
            this.toolStripButtonAddItem,
            this.toolStripButtonCopy,
            this.toolStripButtonDeleteItem,
            this.toolStripSeparator1,
            this.toolStripButtonMoveUp,
            this.toolStripButtonMoveDown});
			this.toolStripListControl.Location = new System.Drawing.Point(0, 0);
			this.toolStripListControl.Name = "toolStripListControl";
			this.toolStripListControl.Size = new System.Drawing.Size(24, 391);
			this.toolStripListControl.TabIndex = 2;
			this.toolStripListControl.Text = "toolStrip2";
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(21, 6);
			// 
			// toolStripButtonAddItem
			// 
			this.toolStripButtonAddItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonAddItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAddItem.Image")));
			this.toolStripButtonAddItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonAddItem.Name = "toolStripButtonAddItem";
			this.toolStripButtonAddItem.Size = new System.Drawing.Size(21, 20);
			this.toolStripButtonAddItem.Text = "Add New Projectile";
			this.toolStripButtonAddItem.ToolTipText = "Add New Item";
			this.toolStripButtonAddItem.Click += new System.EventHandler(this.toolStripButtonAddItem_Click);
			// 
			// toolStripButtonCopy
			// 
			this.toolStripButtonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonCopy.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonCopy.Image")));
			this.toolStripButtonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCopy.Name = "toolStripButtonCopy";
			this.toolStripButtonCopy.Size = new System.Drawing.Size(21, 20);
			this.toolStripButtonCopy.Text = "toolStripButton1";
			this.toolStripButtonCopy.ToolTipText = "Copy Item";
			this.toolStripButtonCopy.Click += new System.EventHandler(this.toolStripButtonCopy_Click);
			// 
			// toolStripButtonDeleteItem
			// 
			this.toolStripButtonDeleteItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonDeleteItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonDeleteItem.Image")));
			this.toolStripButtonDeleteItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDeleteItem.Name = "toolStripButtonDeleteItem";
			this.toolStripButtonDeleteItem.Size = new System.Drawing.Size(21, 20);
			this.toolStripButtonDeleteItem.Text = "toolStripButton1";
			this.toolStripButtonDeleteItem.ToolTipText = "Delete Item";
			this.toolStripButtonDeleteItem.Click += new System.EventHandler(this.toolStripButtonDeleteItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(21, 6);
			// 
			// toolStripButtonMoveUp
			// 
			this.toolStripButtonMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMoveUp.Image")));
			this.toolStripButtonMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMoveUp.Name = "toolStripButtonMoveUp";
			this.toolStripButtonMoveUp.Size = new System.Drawing.Size(21, 20);
			this.toolStripButtonMoveUp.Text = "Move Up";
			this.toolStripButtonMoveUp.Click += new System.EventHandler(this.toolStripButtonMoveUp_Click);
			// 
			// toolStripButtonMoveDown
			// 
			this.toolStripButtonMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMoveDown.Image")));
			this.toolStripButtonMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMoveDown.Name = "toolStripButtonMoveDown";
			this.toolStripButtonMoveDown.Size = new System.Drawing.Size(21, 20);
			this.toolStripButtonMoveDown.Text = "Move Down";
			this.toolStripButtonMoveDown.Click += new System.EventHandler(this.toolStripButtonMoveDown_Click);
			// 
			// propertyGridItemEditor
			// 
			this.propertyGridItemEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGridItemEditor.Location = new System.Drawing.Point(0, 0);
			this.propertyGridItemEditor.Name = "propertyGridItemEditor";
			this.propertyGridItemEditor.Size = new System.Drawing.Size(252, 391);
			this.propertyGridItemEditor.TabIndex = 0;
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNewFile,
            this.toolStripSeparator2,
            this.toolStripButtonFileOpen,
            this.toolStripSeparator3,
            this.toolStripButton2,
            this.toolStripButtonFileSaveAs,
            this.toolStripSeparator4,
            this.toolStripLabel1,
            this.toolStripLabelFileName});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(500, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripButtonNewFile
			// 
			this.toolStripButtonNewFile.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonNewFile.Image")));
			this.toolStripButtonNewFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonNewFile.Name = "toolStripButtonNewFile";
			this.toolStripButtonNewFile.Size = new System.Drawing.Size(51, 22);
			this.toolStripButtonNewFile.Text = "New";
			this.toolStripButtonNewFile.Click += new System.EventHandler(this.toolStripButtonNewFile_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonFileOpen
			// 
			this.toolStripButtonFileOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFileOpen.Image")));
			this.toolStripButtonFileOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFileOpen.Name = "toolStripButtonFileOpen";
			this.toolStripButtonFileOpen.Size = new System.Drawing.Size(62, 22);
			this.toolStripButtonFileOpen.Text = "Open..";
			this.toolStripButtonFileOpen.Click += new System.EventHandler(this.toolStripButtonFileOpen_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.Enabled = false;
			this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(51, 22);
			this.toolStripButton2.Text = "Save";
			// 
			// toolStripButtonFileSaveAs
			// 
			this.toolStripButtonFileSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFileSaveAs.Image")));
			this.toolStripButtonFileSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFileSaveAs.Name = "toolStripButtonFileSaveAs";
			this.toolStripButtonFileSaveAs.Size = new System.Drawing.Size(73, 22);
			this.toolStripButtonFileSaveAs.Text = "Save As..";
			this.toolStripButtonFileSaveAs.Click += new System.EventHandler(this.toolStripButtonFileSaveAs_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(28, 22);
			this.toolStripLabel1.Text = "File:";
			// 
			// toolStripLabelFileName
			// 
			this.toolStripLabelFileName.Name = "toolStripLabelFileName";
			this.toolStripLabelFileName.Size = new System.Drawing.Size(0, 22);
			// 
			// treeViewItems
			// 
			this.treeViewItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewItems.ImageIndex = 0;
			this.treeViewItems.ImageList = this.imageListTreeView;
			this.treeViewItems.Location = new System.Drawing.Point(24, 0);
			this.treeViewItems.Name = "treeViewItems";
			treeNode1.Name = "Node1";
			treeNode1.Text = "SuperSlime";
			treeNode2.Name = "Node2";
			treeNode2.Text = "EvilBunny";
			treeNode3.Name = "Node6";
			treeNode3.Text = "Skeleton Warrior";
			treeNode4.Name = "Node7";
			treeNode4.Text = "Skeletal Mage";
			treeNode5.Name = "Node8";
			treeNode5.Text = "Skeleton King";
			treeNode6.Name = "Node9";
			treeNode6.Text = "Skeleton King Omega";
			treeNode7.ImageIndex = 1;
			treeNode7.Name = "Node3";
			treeNode7.SelectedImageIndex = 1;
			treeNode7.Text = "Category - Quest Npcs";
			treeNode7.ToolTipText = "C://User//Blah/Dungeon/dungeon-npcs.json";
			treeNode8.Name = "Node4";
			treeNode8.Text = "Wimpy Demon";
			treeNode9.Name = "Node5";
			treeNode9.Text = "Rabid Vulture";
			treeNode10.Name = "Node0";
			treeNode10.Text = "Npcs.json";
			treeNode10.ToolTipText = "C://User//Blah/npcs.json";
			this.treeViewItems.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode10});
			this.treeViewItems.SelectedImageIndex = 0;
			this.treeViewItems.Size = new System.Drawing.Size(220, 322);
			this.treeViewItems.TabIndex = 3;
			this.treeViewItems.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewItems_AfterSelect);
			// 
			// imageListTreeView
			// 
			this.imageListTreeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTreeView.ImageStream")));
			this.imageListTreeView.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListTreeView.Images.SetKeyName(0, "ellipse_blue.png");
			this.imageListTreeView.Images.SetKeyName(1, "triangle_red.png");
			this.imageListTreeView.Images.SetKeyName(2, "rectangle_red.png");
			// 
			// ObjectEditor
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.toolStrip1);
			this.Name = "ObjectEditor";
			this.Size = new System.Drawing.Size(500, 416);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.toolStripListControl.ResumeLayout(false);
			this.toolStripListControl.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListBox listBoxItems;
		private System.Windows.Forms.PropertyGrid propertyGridItemEditor;
		private System.Windows.Forms.ToolStrip toolStripListControl;
		private System.Windows.Forms.ToolStripButton toolStripButtonAddItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonDeleteItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonCopy;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton toolStripButtonMoveUp;
		private System.Windows.Forms.ToolStripButton toolStripButtonMoveDown;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButtonFileOpen;
		private System.Windows.Forms.ToolStripButton toolStripButton2;
		private System.Windows.Forms.ToolStripButton toolStripButtonFileSaveAs;
		private System.Windows.Forms.ToolStripButton toolStripButtonNewFile;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripLabel toolStripLabelFileName;
		private System.Windows.Forms.TreeView treeViewItems;
		private System.Windows.Forms.ImageList imageListTreeView;
	}
}
