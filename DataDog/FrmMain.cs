using DataDogLib;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DataDog
{
	public partial class FrmMain : Form
	{
		private readonly string Title;

		private DataDogFile _openFile;

		public FrmMain()
		{
			this.InitializeComponent();
			this.Title = this.Text;

			CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture =
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture =
				CultureInfo.InvariantCulture;
		}

		private void FrmMain_Load(object sender, EventArgs e)
		{
			this.ToolBar.Renderer = new ToolStripRendererNL();

			var args = Environment.GetCommandLineArgs();
			if (args.Length > 1 && File.Exists(args[1]))
				this.OpenFile(args[1]);
		}

		private void BtnOpen_Click(object sender, EventArgs e)
		{
			if (this.OfdDataDog.ShowDialog() != DialogResult.OK)
				return;

			var filePath = this.OfdDataDog.FileName;
			this.OpenFile(filePath);
		}

		private void OpenFile(string filePath)
		{
			try
			{
				_openFile = DataDogFile.Read(filePath);

				this.Text = filePath + " - " + this.Title;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Failed to open file. Error: " + ex.Message, this.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.CboList.Items.Clear();
			this.CboList.Items.Add("All lists");
			foreach (var listName in _openFile.Lists.Keys)
				this.CboList.Items.Add(listName);

			this.CboList.SelectedIndex = 0;
			this.CboList.Enabled = true;
			this.BtnExportXml.Enabled = true;

			if (_openFile.Lists.Count == 1)
				this.CboList.SelectedIndex = 1;
		}

		private void BtnExportXml_Click(object sender, EventArgs e)
		{
			if (this.SfdXml.ShowDialog() != DialogResult.OK)
				return;

			try
			{
				var filePath = this.SfdXml.FileName;

				if (this.CboList.SelectedIndex == 0)
					_openFile.ExportXml(filePath);
				else
					_openFile.ExportXml(filePath, this.CboList.Text);

				if (MessageBox.Show(this, "Export successful, do you want to open the new file?", this.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					Process.Start(filePath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Failed to export to XML. Error: " + ex.Message, this.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

		}

		private void CboList_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.LstObjects.Columns.Clear();
			this.LstObjects.Rows.Clear();

			if (this.CboList.SelectedIndex == 0)
				return;

			var listName = this.CboList.Text;
			var list = _openFile.Lists[listName];
			var type = list.Type;

			this.LstObjects.Columns.Add("_ObjName", "_ObjName");
			foreach (var field in type.Fields.Values.OrderBy(a => a.Offset))
			{
				this.LstObjects.Columns.Add(field.Name, field.Name);
			}

			foreach (var obj in list.Objects)
			{
				var row = new DataGridViewRow();
				row.CreateCells(this.LstObjects);

				row.Cells[0].Value = obj.Name;

				var i = 1;
				foreach (var fieldDef in type.Fields.Values.OrderBy(a => a.Offset))
				{
					var field = obj.Fields[fieldDef.Name];
					string value;

					switch (fieldDef.VarType)
					{
						case DataVarType.Color: value = ("0x" + ((uint)field.Value).ToString("X8")); break;
						default: row.Cells[i++].Value = field.Value; break;
					}
				}

				this.LstObjects.Rows.Add(row);
			}
		}

		private void BtnAbout_Click(object sender, EventArgs e)
		{
			new FrmAbout().ShowDialog();
		}

		private void FrmMain_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = (e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None);
		}

		private void FrmMain_DragDrop(object sender, DragEventArgs e)
		{
			var filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (filePaths.Length == 0)
				return;

			this.OpenFile(filePaths[0]);
		}
	}
}
