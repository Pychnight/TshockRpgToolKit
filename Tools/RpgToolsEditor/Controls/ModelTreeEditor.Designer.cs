﻿namespace RpgToolsEditor.Controls
{
	partial class ModelTreeEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelTreeEditor));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.toolStripContainerItems = new System.Windows.Forms.ToolStripContainer();
			this.treeViewItems = new System.Windows.Forms.TreeView();
			this.imageListModelTreeEditorDefaultItems = new System.Windows.Forms.ImageList(this.components);
			this.toolStripItemControls = new System.Windows.Forms.ToolStrip();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonAddItem = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonCopy = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.propertyGridItemEditor = new System.Windows.Forms.PropertyGrid();
			this.toolStripFileControls = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonNewFile = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonFileOpen = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonFileSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonFileSaveAs = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabelFileName = new System.Windows.Forms.ToolStripLabel();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.toolStripContainerItems.ContentPanel.SuspendLayout();
			this.toolStripContainerItems.LeftToolStripPanel.SuspendLayout();
			this.toolStripContainerItems.SuspendLayout();
			this.toolStripItemControls.SuspendLayout();
			this.toolStripFileControls.SuspendLayout();
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
			this.splitContainer1.Panel1.Controls.Add(this.toolStripContainerItems);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.propertyGridItemEditor);
			this.splitContainer1.Size = new System.Drawing.Size(500, 391);
			this.splitContainer1.SplitterDistance = 244;
			this.splitContainer1.TabIndex = 0;
			// 
			// toolStripContainerItems
			// 
			this.toolStripContainerItems.BottomToolStripPanelVisible = false;
			// 
			// toolStripContainerItems.ContentPanel
			// 
			this.toolStripContainerItems.ContentPanel.Controls.Add(this.treeViewItems);
			this.toolStripContainerItems.ContentPanel.Size = new System.Drawing.Size(220, 391);
			this.toolStripContainerItems.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// toolStripContainerItems.LeftToolStripPanel
			// 
			this.toolStripContainerItems.LeftToolStripPanel.Controls.Add(this.toolStripItemControls);
			this.toolStripContainerItems.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainerItems.Name = "toolStripContainerItems";
			this.toolStripContainerItems.RightToolStripPanelVisible = false;
			this.toolStripContainerItems.Size = new System.Drawing.Size(244, 391);
			this.toolStripContainerItems.TabIndex = 4;
			this.toolStripContainerItems.Text = "toolStripContainer1";
			this.toolStripContainerItems.TopToolStripPanelVisible = false;
			// 
			// treeViewItems
			// 
			this.treeViewItems.AllowDrop = true;
			this.treeViewItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewItems.ImageIndex = 0;
			this.treeViewItems.ImageList = this.imageListModelTreeEditorDefaultItems;
			this.treeViewItems.Location = new System.Drawing.Point(0, 0);
			this.treeViewItems.Name = "treeViewItems";
			this.treeViewItems.SelectedImageIndex = 0;
			this.treeViewItems.Size = new System.Drawing.Size(220, 391);
			this.treeViewItems.TabIndex = 3;
			this.treeViewItems.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewItems_ItemDrag);
			this.treeViewItems.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewItems_AfterSelect);
			this.treeViewItems.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewItems_DragDrop);
			this.treeViewItems.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewItems_DragEnter);
			this.treeViewItems.Leave += new System.EventHandler(this.treeViewItems_Leave);
			this.treeViewItems.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeViewItems_MouseUp);
			// 
			// imageListModelTreeEditorDefaultItems
			// 
			this.imageListModelTreeEditorDefaultItems.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListModelTreeEditorDefaultItems.ImageStream")));
			this.imageListModelTreeEditorDefaultItems.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListModelTreeEditorDefaultItems.Images.SetKeyName(0, "ellipse_blue.png");
			this.imageListModelTreeEditorDefaultItems.Images.SetKeyName(1, "triangle_red.png");
			this.imageListModelTreeEditorDefaultItems.Images.SetKeyName(2, "rectangle_red.png");
			// 
			// toolStripItemControls
			// 
			this.toolStripItemControls.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripItemControls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripItemControls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator5,
            this.toolStripButtonAddItem,
            this.toolStripButtonCopy,
            this.toolStripButtonDelete,
            this.toolStripSeparator1});
			this.toolStripItemControls.Location = new System.Drawing.Point(0, 3);
			this.toolStripItemControls.Name = "toolStripItemControls";
			this.toolStripItemControls.Size = new System.Drawing.Size(24, 83);
			this.toolStripItemControls.TabIndex = 2;
			this.toolStripItemControls.Text = "toolStrip2";
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(22, 6);
			// 
			// toolStripButtonAddItem
			// 
			this.toolStripButtonAddItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonAddItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAddItem.Image")));
			this.toolStripButtonAddItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonAddItem.Name = "toolStripButtonAddItem";
			this.toolStripButtonAddItem.Size = new System.Drawing.Size(22, 20);
			this.toolStripButtonAddItem.Text = "Add New Projectile";
			this.toolStripButtonAddItem.ToolTipText = "Add New Item";
			this.toolStripButtonAddItem.Click += new System.EventHandler(this.toolStripButtonAddItem_Click);
			// 
			// toolStripButtonCopy
			// 
			this.toolStripButtonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonCopy.Enabled = false;
			this.toolStripButtonCopy.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonCopy.Image")));
			this.toolStripButtonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCopy.Name = "toolStripButtonCopy";
			this.toolStripButtonCopy.Size = new System.Drawing.Size(22, 20);
			this.toolStripButtonCopy.Text = "toolStripButton1";
			this.toolStripButtonCopy.ToolTipText = "Copy Item";
			this.toolStripButtonCopy.Click += new System.EventHandler(this.toolStripButtonCopy_Click);
			// 
			// toolStripButtonDelete
			// 
			this.toolStripButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonDelete.Enabled = false;
			this.toolStripButtonDelete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonDelete.Image")));
			this.toolStripButtonDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDelete.Name = "toolStripButtonDelete";
			this.toolStripButtonDelete.Size = new System.Drawing.Size(22, 20);
			this.toolStripButtonDelete.Text = "toolStripButton1";
			this.toolStripButtonDelete.ToolTipText = "Delete Item";
			this.toolStripButtonDelete.Click += new System.EventHandler(this.toolStripButtonDeleteItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(22, 6);
			// 
			// propertyGridItemEditor
			// 
			this.propertyGridItemEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGridItemEditor.Location = new System.Drawing.Point(0, 0);
			this.propertyGridItemEditor.Name = "propertyGridItemEditor";
			this.propertyGridItemEditor.Size = new System.Drawing.Size(252, 391);
			this.propertyGridItemEditor.TabIndex = 0;
			this.propertyGridItemEditor.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridItemEditor_PropertyValueChanged);
			// 
			// toolStripFileControls
			// 
			this.toolStripFileControls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripFileControls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNewFile,
            this.toolStripSeparator2,
            this.toolStripButtonFileOpen,
            this.toolStripSeparator3,
            this.toolStripButtonFileSave,
            this.toolStripButtonFileSaveAs,
            this.toolStripSeparator4,
            this.toolStripLabelFileName});
			this.toolStripFileControls.Location = new System.Drawing.Point(0, 0);
			this.toolStripFileControls.Name = "toolStripFileControls";
			this.toolStripFileControls.Size = new System.Drawing.Size(500, 25);
			this.toolStripFileControls.TabIndex = 1;
			this.toolStripFileControls.Text = "toolStrip1";
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
			// toolStripButtonFileSave
			// 
			this.toolStripButtonFileSave.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFileSave.Image")));
			this.toolStripButtonFileSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFileSave.Name = "toolStripButtonFileSave";
			this.toolStripButtonFileSave.Size = new System.Drawing.Size(51, 22);
			this.toolStripButtonFileSave.Text = "Save";
			this.toolStripButtonFileSave.Click += new System.EventHandler(this.toolStripButtonFileSave_Click);
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
			// toolStripLabelFileName
			// 
			this.toolStripLabelFileName.Name = "toolStripLabelFileName";
			this.toolStripLabelFileName.Size = new System.Drawing.Size(0, 22);
			// 
			// ModelTreeEditor
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.toolStripFileControls);
			this.Name = "ModelTreeEditor";
			this.Size = new System.Drawing.Size(500, 416);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.toolStripContainerItems.ContentPanel.ResumeLayout(false);
			this.toolStripContainerItems.LeftToolStripPanel.ResumeLayout(false);
			this.toolStripContainerItems.LeftToolStripPanel.PerformLayout();
			this.toolStripContainerItems.ResumeLayout(false);
			this.toolStripContainerItems.PerformLayout();
			this.toolStripItemControls.ResumeLayout(false);
			this.toolStripItemControls.PerformLayout();
			this.toolStripFileControls.ResumeLayout(false);
			this.toolStripFileControls.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.PropertyGrid propertyGridItemEditor;
		private System.Windows.Forms.ToolStrip toolStripItemControls;
		private System.Windows.Forms.ToolStripButton toolStripButtonAddItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
		private System.Windows.Forms.ToolStripButton toolStripButtonCopy;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStrip toolStripFileControls;
		private System.Windows.Forms.ToolStripButton toolStripButtonFileOpen;
		private System.Windows.Forms.ToolStripButton toolStripButtonFileSave;
		private System.Windows.Forms.ToolStripButton toolStripButtonFileSaveAs;
		private System.Windows.Forms.ToolStripButton toolStripButtonNewFile;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripLabel toolStripLabelFileName;
		private System.Windows.Forms.TreeView treeViewItems;
		private System.Windows.Forms.ToolStripContainer toolStripContainerItems;
		public System.Windows.Forms.ImageList imageListModelTreeEditorDefaultItems;
	}
}
