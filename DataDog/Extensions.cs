using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DataDog
{
	public static class DataGridViewExtensions
	{
		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);
		private const int WM_SETREDRAW = 11;

		public static void BeginUpdate(this DataGridView dataGridView)
		{
			SendMessage(dataGridView.Handle, WM_SETREDRAW, false, 0);
		}

		public static void EndUpdate(this DataGridView dataGridView)
		{
			SendMessage(dataGridView.Handle, WM_SETREDRAW, true, 0);
			dataGridView.Refresh();
		}

		public static void SetDoubleBuffering(this DataGridView dataGridView, bool enabled)
		{
			// Double buffering can make DGV slow in remote desktop
			if (!SystemInformation.TerminalServerSession)
			{
				var type = dataGridView.GetType();
				var property = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);

				property.SetValue(dataGridView, enabled, null);
			}
		}
	}
}
