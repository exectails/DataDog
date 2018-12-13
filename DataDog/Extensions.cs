using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DataDog
{
	/// <summary>
	/// Extenions for DataGridView.
	/// </summary>
	public static class DataGridViewExtensions
	{
		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);
		private const int WM_SETREDRAW = 11;

		/// <summary>
		/// Suspends redraws until EndUpdate is called.
		/// </summary>
		/// <param name="dataGridView"></param>
		public static void BeginUpdate(this DataGridView dataGridView)
		{
			SendMessage(dataGridView.Handle, WM_SETREDRAW, false, 0);
		}

		/// <summary>
		/// Restarts redraws and immediately refreshes control.
		/// </summary>
		/// <param name="dataGridView"></param>
		public static void EndUpdate(this DataGridView dataGridView)
		{
			SendMessage(dataGridView.Handle, WM_SETREDRAW, true, 0);
			dataGridView.Refresh();
		}

		/// <summary>
		/// Sets grid's DoubleBuffered property.
		/// </summary>
		/// <param name="dataGridView"></param>
		/// <param name="enabled"></param>
		public static void SetDoubleBuffered(this DataGridView dataGridView, bool enabled)
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
