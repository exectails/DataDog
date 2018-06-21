using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DataDog
{
	public partial class FrmAbout : Form
	{
		public FrmAbout()
		{
			this.InitializeComponent();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void ImgLink_Click(object sender, EventArgs e)
		{
			Process.Start((sender as Control).Tag as string);
		}
	}
}
