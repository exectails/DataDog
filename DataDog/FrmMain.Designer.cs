namespace DataDog
{
	partial class FrmMain
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
			this.LstObjects = new System.Windows.Forms.DataGridView();
			this.ToolBar = new System.Windows.Forms.ToolStrip();
			this.BtnOpen = new System.Windows.Forms.ToolStripButton();
			this.BtnSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.CboList = new System.Windows.Forms.ToolStripComboBox();
			this.BtnExportXml = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.BtnAddObject = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.BtnAbout = new System.Windows.Forms.ToolStripButton();
			this.OfdDataDog = new System.Windows.Forms.OpenFileDialog();
			this.SfdXml = new System.Windows.Forms.SaveFileDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.BtnRemoveObject = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.LstObjects)).BeginInit();
			this.ToolBar.SuspendLayout();
			this.SuspendLayout();
			// 
			// LstObjects
			// 
			this.LstObjects.AllowUserToAddRows = false;
			this.LstObjects.AllowUserToDeleteRows = false;
			this.LstObjects.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LstObjects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.LstObjects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LstObjects.Location = new System.Drawing.Point(0, 25);
			this.LstObjects.Name = "LstObjects";
			this.LstObjects.Size = new System.Drawing.Size(800, 425);
			this.LstObjects.TabIndex = 0;
			this.LstObjects.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.LstObjects_CellBeginEdit);
			this.LstObjects.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.LstObjects_CellEndEdit);
			this.LstObjects.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.LstObjects_CellValidating);
			// 
			// ToolBar
			// 
			this.ToolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BtnOpen,
            this.BtnSave,
            this.toolStripSeparator1,
            this.CboList,
            this.BtnExportXml,
            this.toolStripSeparator2,
            this.BtnAddObject,
            this.BtnRemoveObject,
            this.toolStripSeparator3,
            this.BtnAbout});
			this.ToolBar.Location = new System.Drawing.Point(0, 0);
			this.ToolBar.Name = "ToolBar";
			this.ToolBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.ToolBar.Size = new System.Drawing.Size(800, 25);
			this.ToolBar.TabIndex = 1;
			this.ToolBar.Text = "toolStrip1";
			// 
			// BtnOpen
			// 
			this.BtnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.BtnOpen.Image = ((System.Drawing.Image)(resources.GetObject("BtnOpen.Image")));
			this.BtnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnOpen.Name = "BtnOpen";
			this.BtnOpen.Size = new System.Drawing.Size(23, 22);
			this.BtnOpen.Text = "Open...";
			this.BtnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
			// 
			// BtnSave
			// 
			this.BtnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.BtnSave.Image = ((System.Drawing.Image)(resources.GetObject("BtnSave.Image")));
			this.BtnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.Size = new System.Drawing.Size(23, 22);
			this.BtnSave.Text = "Save";
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// CboList
			// 
			this.CboList.AutoSize = false;
			this.CboList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CboList.Enabled = false;
			this.CboList.Name = "CboList";
			this.CboList.Size = new System.Drawing.Size(150, 23);
			this.CboList.SelectedIndexChanged += new System.EventHandler(this.CboList_SelectedIndexChanged);
			// 
			// BtnExportXml
			// 
			this.BtnExportXml.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.BtnExportXml.Enabled = false;
			this.BtnExportXml.Image = ((System.Drawing.Image)(resources.GetObject("BtnExportXml.Image")));
			this.BtnExportXml.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnExportXml.Name = "BtnExportXml";
			this.BtnExportXml.Size = new System.Drawing.Size(23, 22);
			this.BtnExportXml.Text = "Export to XML...";
			this.BtnExportXml.Click += new System.EventHandler(this.BtnExportXml_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// BtnAddObject
			// 
			this.BtnAddObject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.BtnAddObject.Image = ((System.Drawing.Image)(resources.GetObject("BtnAddObject.Image")));
			this.BtnAddObject.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnAddObject.Name = "BtnAddObject";
			this.BtnAddObject.Size = new System.Drawing.Size(23, 22);
			this.BtnAddObject.Text = "Add Object";
			this.BtnAddObject.Click += new System.EventHandler(this.BtnAddObject_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// BtnAbout
			// 
			this.BtnAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.BtnAbout.Image = ((System.Drawing.Image)(resources.GetObject("BtnAbout.Image")));
			this.BtnAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnAbout.Name = "BtnAbout";
			this.BtnAbout.Size = new System.Drawing.Size(23, 22);
			this.BtnAbout.Text = "About";
			this.BtnAbout.Click += new System.EventHandler(this.BtnAbout_Click);
			// 
			// OfdDataDog
			// 
			this.OfdDataDog.Filter = "DataDog File|*.data";
			// 
			// SfdXml
			// 
			this.SfdXml.Filter = "XML File|*.xml";
			// 
			// BtnRemoveObject
			// 
			this.BtnRemoveObject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.BtnRemoveObject.Image = ((System.Drawing.Image)(resources.GetObject("BtnRemoveObject.Image")));
			this.BtnRemoveObject.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnRemoveObject.Name = "BtnRemoveObject";
			this.BtnRemoveObject.Size = new System.Drawing.Size(23, 22);
			this.BtnRemoveObject.Text = "Remove selected objects";
			this.BtnRemoveObject.Click += new System.EventHandler(this.BtnRemoveObject_Click);
			// 
			// FrmMain
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.LstObjects);
			this.Controls.Add(this.ToolBar);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FrmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "DataDog";
			this.Load += new System.EventHandler(this.FrmMain_Load);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FrmMain_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FrmMain_DragEnter);
			((System.ComponentModel.ISupportInitialize)(this.LstObjects)).EndInit();
			this.ToolBar.ResumeLayout(false);
			this.ToolBar.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView LstObjects;
		private System.Windows.Forms.ToolStrip ToolBar;
		private System.Windows.Forms.ToolStripButton BtnOpen;
		private System.Windows.Forms.ToolStripButton BtnExportXml;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripComboBox CboList;
		private System.Windows.Forms.OpenFileDialog OfdDataDog;
		private System.Windows.Forms.SaveFileDialog SfdXml;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton BtnAbout;
		private System.Windows.Forms.ToolStripButton BtnSave;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.ToolStripButton BtnAddObject;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton BtnRemoveObject;
	}
}

