﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataDogLib;
using DataObject = DataDogLib.DataObject;

namespace DataDog
{
	/// <summary>
	/// Main Form.
	/// </summary>
	public partial class FrmMain : Form
	{
		private readonly string Title;

		private string _openFilePath;
		private DataDogFile _openFile;

		private DataObject _validatedObj;
		private bool _editingCell;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public FrmMain()
		{
			this.InitializeComponent();
			this.Title = this.Text;
			this.LstObjects.SetDoubleBuffered(true);

			CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture =
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture =
				CultureInfo.InvariantCulture;
		}

		/// <summary>
		/// Called when the form is loaded, opens files via arguments.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FrmMain_Load(object sender, EventArgs e)
		{
			this.ToolBar.Renderer = new ToolStripRendererNL();

			var args = Environment.GetCommandLineArgs();
			if (args.Length > 1 && File.Exists(args[1]))
				this.OpenFile(args[1]);
		}

		/// <summary>
		/// Called when the Open button is clicked, opens selected file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnOpen_Click(object sender, EventArgs e)
		{
			if (this.OfdDataDog.ShowDialog() != DialogResult.OK)
				return;

			var filePath = this.OfdDataDog.FileName;
			this.OpenFile(filePath);
		}

		/// <summary>
		/// Opens file from given path.
		/// </summary>
		/// <param name="filePath"></param>
		private void OpenFile(string filePath)
		{
			try
			{
				_openFile = DataDogFile.Read(filePath);
				_openFilePath = filePath;

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

		/// <summary>
		/// Called when the XML Export button is clicked, exports file
		/// in XML format to selected path.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Called when a new list is selected, reloads object list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CboList_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.LstObjects.BeginUpdate();

			try
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

				var rows = new DataGridViewRow[list.Objects.Count];
				var i = 0;

				foreach (var obj in list.Objects)
				{
					var row = this.GetRowFromObject(obj);
					//this.LstObjects.Rows.Add(row);
					rows[i++] = row;
				}

				this.LstObjects.Rows.AddRange(rows);
				this.LblObjectCount.Text = "Objects: " + rows.Length;
			}
			finally
			{
				this.LstObjects.EndUpdate();
			}
		}

		/// <summary>
		/// Called when the About button is clicked, opens About form.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnAbout_Click(object sender, EventArgs e)
		{
			new FrmAbout().ShowDialog();
		}

		/// <summary>
		/// Called when something is dragged onto this form, enables drag
		/// if it's a file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FrmMain_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = (e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None);
		}

		/// <summary>
		/// Called when something is dropped onto form, opens files.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FrmMain_DragDrop(object sender, DragEventArgs e)
		{
			var filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (filePaths.Length == 0)
				return;

			this.OpenFile(filePaths[0]);
		}

		/// <summary>
		/// Called when the Save button is clicked, saves file to its
		/// current path.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnSave_Click(object sender, EventArgs e)
		{
			if (_openFile == null)
			{
				MessageBox.Show("No open file.", this.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			try
			{
				//var path = Path.ChangeExtension(_openFilePath, ".test.data");
				var path = _openFilePath;

				using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
					_openFile.Write(fs);

				MessageBox.Show("Saved file.", this.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to save file. Error: " + ex, this.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Called when a user starts editing a cell.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LstObjects_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
		{
			_editingCell = true;
		}

		/// <summary>
		/// Called when the user ends editing a cell, updates object
		/// in list and open file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LstObjects_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			_editingCell = false;

			if (_validatedObj == null)
				return;

			var row = this.LstObjects.Rows[e.RowIndex];
			var col = this.LstObjects.Columns[e.ColumnIndex];
			var cell = this.LstObjects.Rows[e.RowIndex].Cells[e.ColumnIndex];
			var listName = this.CboList.Text;

			//var objName = row.Cells[0].Value.ToString();
			//var obj = _openFile.Lists[listName].Objects.FirstOrDefault(a => a.Name == objName);
			var obj = _validatedObj;

			var fieldName = col.Name;
			if (fieldName == "_ObjName")
			{
				obj.Name = cell.Value.ToString();
				return;
			}

			var field = obj.Fields[fieldName];

			var newValueStr = cell.Value.ToString();
			object newValue = null;

			switch (field.VarType)
			{
				case DataVarType.Bool:
					newValue = string.Compare(newValueStr, "true", true) == 0;
					break;

				case DataVarType.Byte:
					newValue = byte.Parse(newValueStr);
					break;

				case DataVarType.Color:
					newValue = uint.Parse(newValueStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					break;

				case DataVarType.Float:
					newValue = float.Parse(newValueStr, NumberStyles.Float, CultureInfo.InvariantCulture);
					break;

				case DataVarType.Integer:
					newValue = int.Parse(newValueStr);
					break;

				case DataVarType.Reference:
				case DataVarType.String:
					newValue = newValueStr;
					break;

				default:
					throw new Exception($"Unknown type '{field.VarType}'.");
			}

			field.Value = newValue;
		}

		/// <summary>
		/// Called to validate the value of a cell, cancels editing if
		/// a value that was entered was invalid, such as an empty object
		/// name or a non-numeric string for an integer field.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LstObjects_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (!_editingCell)
				return;

			var row = this.LstObjects.Rows[e.RowIndex];
			var col = this.LstObjects.Columns[e.ColumnIndex];
			var cell = this.LstObjects.Rows[e.RowIndex].Cells[e.ColumnIndex];
			var listName = this.CboList.Text;

			var objName = row.Cells[0].Value.ToString();
			var list = _openFile.Lists[listName];
			var obj = list.Objects.FirstOrDefault(a => a.Name == objName);

			if (obj == null)
			{
				MessageBox.Show($"Object '{objName}' was not found, something went wrong here. Please report.", this.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			_validatedObj = obj;
			var newValue = e.FormattedValue.ToString();

			var fieldName = col.Name;
			if (fieldName == "_ObjName")
			{
				if (newValue == "")
				{
					e.Cancel = true;
					this.LstObjects.CancelEdit();
					this.LstObjects.EndEdit();
					MessageBox.Show($"Object Name can't be empty.", this.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else if (list.Objects.Any(a => a.Name == newValue))
				{
					e.Cancel = true;
					this.LstObjects.CancelEdit();
					this.LstObjects.EndEdit();
					MessageBox.Show($"Object names must be unique.", this.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			else
			{
				var field = obj.Fields[fieldName];

				if (!field.ValidateStringValue(newValue))
				{
					e.Cancel = true;
					this.LstObjects.CancelEdit();
					this.LstObjects.EndEdit();
					MessageBox.Show($"Invalid '{field.VarType}' value.", this.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Returns new row to use in list from object's data.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		private DataGridViewRow GetRowFromObject(DataObject obj)
		{
			var row = new DataGridViewRow();
			row.CreateCells(this.LstObjects);

			row.Cells[0].Value = obj.Name;

			var i = 1;
			foreach (var fieldDef in obj.Type.Fields.Values.OrderBy(a => a.Offset))
			{
				var field = obj.Fields[fieldDef.Name];

				switch (fieldDef.VarType)
				{
					case DataVarType.Color: row.Cells[i].Value = ((uint)field.Value).ToString("X8"); break;
					default: row.Cells[i].Value = field.Value; break;
				}

				//row.Cells[i].ToolTipText = fieldDef.VarType.ToString();
				i++;
			}

			return row;
		}

		/// <summary>
		/// Called when the Add Object button is clicked, adds an object
		/// with default values to list and file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnAddObject_Click(object sender, EventArgs e)
		{
			var listName = this.CboList.Text;
			var list = _openFile.Lists[listName];
			var type = list.Type;

			var name = "NewObject";
			for (var i = 1; ; ++i)
			{
				if (!list.Objects.Any(a => a.Name == name + i))
				{
					name = name + i;
					break;
				}
			}

			var obj = DataObject.New(name, type);
			var row = this.GetRowFromObject(obj);

			this.LstObjects.Rows.Add(row);
			list.Objects.Add(obj);

			this.LstObjects.ClearSelection();
			this.LstObjects.CurrentCell = this.LstObjects[0, this.LstObjects.RowCount - 1];
		}

		/// <summary>
		/// Called when the Remove Selected Objects button is clicked,
		/// removes all currently selected objects from list and file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnRemoveObject_Click(object sender, EventArgs e)
		{
			if (this.LstObjects.SelectedRows.Count == 0)
			{
				MessageBox.Show("No objects selected.", this.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			var listName = this.CboList.Text;
			var list = _openFile.Lists[listName];

			var toRemove = new List<DataGridViewRow>();

			foreach (DataGridViewRow row in this.LstObjects.SelectedRows)
			{
				var objName = row.Cells[0].Value.ToString();
				list.Objects.RemoveAll(a => a.Name == objName);

				toRemove.Add(row);
			}

			foreach (var row in toRemove)
				this.LstObjects.Rows.Remove(row);
		}

		/// <summary>
		/// Called when a key is let go on the list, inserts and removes
		/// objects.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LstObjects_KeyUp(object sender, KeyEventArgs e)
		{
			if (this.LstObjects.IsCurrentCellInEditMode)
				return;

			if (e.KeyCode == Keys.Delete)
			{
				this.BtnRemoveObject_Click(null, null);
			}
			else if (e.KeyCode == Keys.Insert)
			{
				this.BtnAddObject_Click(null, null);
			}
			else if (e.KeyCode == Keys.C && e.Control)
			{
				this.CopySelectedRowsToClipboard();
			}
			else if (e.KeyCode == Keys.V && e.Control)
			{
				this.PasteRowsFromClipboard();
			}
		}

		/// <summary>
		/// Copies all currently selected row's objects to the clipboard.
		/// </summary>
		private void CopySelectedRowsToClipboard()
		{
			if (this.LstObjects.SelectedRows.Count == 0)
				return;

			var listName = this.CboList.Text;
			var list = _openFile.Lists[listName];

			var toCopy = new List<DataObject>();

			foreach (DataGridViewRow row in this.LstObjects.SelectedRows)
			{
				var objName = row.Cells[0].Value.ToString();
				var obj = list.Objects.FirstOrDefault(a => a.Name == objName);

				if (obj != null)
					toCopy.Add(obj);
			}

			Clipboard.SetData("DataDog.DataObjectArray", toCopy);

			MessageBox.Show(string.Format("Copied {0} object(s) to clipboard.", toCopy.Count), this.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Adds all objects found in the clipboard.
		/// </summary>
		private void PasteRowsFromClipboard()
		{
			if (!(Clipboard.GetData("DataDog.DataObjectArray") is List<DataObject> copiedObjects) || copiedObjects.Count == 0)
				return;

			var listName = this.CboList.Text;
			var list = _openFile.Lists[listName];
			var type = list.Type;

			foreach (var obj in copiedObjects)
			{
				if (list.Objects.Any(a => a.Name == obj.Name))
				{
					for (var i = 1; ; ++i)
					{
						if (!list.Objects.Any(a => a.Name == obj.Name + i))
						{
							obj.Name = obj.Name + i;
							break;
						}
					}
				}

				var row = this.GetRowFromObject(obj);

				this.LstObjects.Rows.Add(row);
				list.Objects.Add(obj);
			}

			this.LstObjects.ClearSelection();
			this.LstObjects.CurrentCell = this.LstObjects[0, this.LstObjects.RowCount - 1];

			MessageBox.Show(string.Format("Pasted {0} object(s) from clipboard.", copiedObjects.Count), this.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Called when a key is let go off, used for global hotkeys.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FrmMain_KeyUp(object sender, KeyEventArgs e)
		{
			if (this.LstObjects.IsCurrentCellInEditMode)
				return;

			if (e.Control)
			{
				// Save
				if (e.KeyCode == Keys.S)
				{
					this.BtnSave_Click(null, null);
				}
				// Open
				else if (e.KeyCode == Keys.O)
				{
					this.BtnOpen_Click(null, null);
				}
			}
		}
	}
}
