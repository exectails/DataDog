using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DataDog
{
	/// <summary>
	/// About Form.
	/// </summary>
	public partial class FrmAbout : Form
	{
		/// <summary>
		/// Creates new instance.
		/// </summary>
		public FrmAbout()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// Called when OK is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnOK_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Called when the link is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ImgLink_Click(object sender, EventArgs e)
		{
			Process.Start((sender as Control).Tag as string);
		}
	}
}
