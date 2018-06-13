namespace CustomNpcsEdit.Controls
{
	partial class ProjectileEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectileEditor));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.listBoxItems = new System.Windows.Forms.ListBox();
			this.toolStripListControl = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonAddItem = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDeleteItem = new System.Windows.Forms.ToolStripButton();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.propertyGridItemEditor = new System.Windows.Forms.PropertyGrid();
			this.toolStripButtonCopy = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.toolStripListControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.listBoxItems);
			this.splitContainer1.Panel1.Controls.Add(this.toolStripListControl);
			this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.propertyGridItemEditor);
			this.splitContainer1.Size = new System.Drawing.Size(498, 413);
			this.splitContainer1.SplitterDistance = 244;
			this.splitContainer1.TabIndex = 0;
			// 
			// listBoxItems
			// 
			this.listBoxItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxItems.FormattingEnabled = true;
			this.listBoxItems.Location = new System.Drawing.Point(32, 25);
			this.listBoxItems.Name = "listBoxItems";
			this.listBoxItems.Size = new System.Drawing.Size(212, 388);
			this.listBoxItems.TabIndex = 0;
			this.listBoxItems.SelectedValueChanged += new System.EventHandler(this.listBoxItems_SelectedValueChanged);
			// 
			// toolStripListControl
			// 
			this.toolStripListControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.toolStripListControl.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripListControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAddItem,
            this.toolStripButtonCopy,
            this.toolStripButtonDeleteItem});
			this.toolStripListControl.Location = new System.Drawing.Point(0, 25);
			this.toolStripListControl.Name = "toolStripListControl";
			this.toolStripListControl.Size = new System.Drawing.Size(32, 388);
			this.toolStripListControl.TabIndex = 2;
			this.toolStripListControl.Text = "toolStrip2";
			// 
			// toolStripButtonAddItem
			// 
			this.toolStripButtonAddItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonAddItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAddItem.Image")));
			this.toolStripButtonAddItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonAddItem.Name = "toolStripButtonAddItem";
			this.toolStripButtonAddItem.Size = new System.Drawing.Size(29, 20);
			this.toolStripButtonAddItem.Text = "Add New Projectile";
			this.toolStripButtonAddItem.Click += new System.EventHandler(this.toolStripButtonAddItem_Click);
			// 
			// toolStripButtonDeleteItem
			// 
			this.toolStripButtonDeleteItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonDeleteItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonDeleteItem.Image")));
			this.toolStripButtonDeleteItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDeleteItem.Name = "toolStripButtonDeleteItem";
			this.toolStripButtonDeleteItem.Size = new System.Drawing.Size(29, 20);
			this.toolStripButtonDeleteItem.Text = "toolStripButton1";
			this.toolStripButtonDeleteItem.ToolTipText = "Delete Projectile";
			this.toolStripButtonDeleteItem.Click += new System.EventHandler(this.toolStripButtonDeleteItem_Click);
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(244, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// propertyGridItemEditor
			// 
			this.propertyGridItemEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGridItemEditor.Location = new System.Drawing.Point(0, 0);
			this.propertyGridItemEditor.Name = "propertyGridItemEditor";
			this.propertyGridItemEditor.Size = new System.Drawing.Size(250, 413);
			this.propertyGridItemEditor.TabIndex = 0;
			// 
			// toolStripButtonCopy
			// 
			this.toolStripButtonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonCopy.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonCopy.Image")));
			this.toolStripButtonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCopy.Name = "toolStripButtonCopy";
			this.toolStripButtonCopy.Size = new System.Drawing.Size(29, 20);
			this.toolStripButtonCopy.Text = "toolStripButton1";
			this.toolStripButtonCopy.ToolTipText = "Copy Projectile";
			this.toolStripButtonCopy.Click += new System.EventHandler(this.toolStripButtonCopy_Click);
			// 
			// ProjectileEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ProjectileEditor";
			this.Size = new System.Drawing.Size(498, 413);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.toolStripListControl.ResumeLayout(false);
			this.toolStripListControl.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListBox listBoxItems;
		private System.Windows.Forms.PropertyGrid propertyGridItemEditor;
		private System.Windows.Forms.ToolStrip toolStripListControl;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButtonAddItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonDeleteItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonCopy;
	}
}
