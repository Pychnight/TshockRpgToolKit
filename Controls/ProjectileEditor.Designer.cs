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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.propertyGridItemEditor = new System.Windows.Forms.PropertyGrid();
			this.listBoxItems = new System.Windows.Forms.ListBox();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
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
			this.splitContainer1.Panel1.Controls.Add(this.toolStrip2);
			this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.propertyGridItemEditor);
			this.splitContainer1.Size = new System.Drawing.Size(498, 413);
			this.splitContainer1.SplitterDistance = 244;
			this.splitContainer1.TabIndex = 0;
			// 
			// propertyGridItemEditor
			// 
			this.propertyGridItemEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGridItemEditor.Location = new System.Drawing.Point(0, 0);
			this.propertyGridItemEditor.Name = "propertyGridItemEditor";
			this.propertyGridItemEditor.Size = new System.Drawing.Size(250, 413);
			this.propertyGridItemEditor.TabIndex = 0;
			// 
			// listBoxItems
			// 
			this.listBoxItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxItems.FormattingEnabled = true;
			this.listBoxItems.Location = new System.Drawing.Point(26, 25);
			this.listBoxItems.Name = "listBoxItems";
			this.listBoxItems.Size = new System.Drawing.Size(218, 388);
			this.listBoxItems.TabIndex = 0;
			this.listBoxItems.SelectedValueChanged += new System.EventHandler(this.listBoxItems_SelectedValueChanged);
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
			// toolStrip2
			// 
			this.toolStrip2.Dock = System.Windows.Forms.DockStyle.Left;
			this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip2.Location = new System.Drawing.Point(0, 25);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(26, 388);
			this.toolStrip2.TabIndex = 2;
			this.toolStrip2.Text = "toolStrip2";
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
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListBox listBoxItems;
		private System.Windows.Forms.PropertyGrid propertyGridItemEditor;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStrip toolStrip1;
	}
}
