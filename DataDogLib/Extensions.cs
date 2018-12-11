using System.IO;
using System.Text;

namespace DataDogLib
{
	/// <summary>
	/// Extensions for BinaryWriter.
	/// </summary>
	public static class BinaryWriterExtensions
	{
		/// <summary>
		/// Writes UTF8 string to binary writer.
		/// </summary>
		/// <param name="bw"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void WriteString(this BinaryWriter bw, string format, params object[] args)
		{
			if (args.Length != 0)
				bw.Write(Encoding.GetEncoding("EUC-KR").GetBytes(string.Format(format, args)));
			else
				bw.Write(Encoding.GetEncoding("EUC-KR").GetBytes(format));
		}

		/// <summary>
		/// Writes null-terminated UTF8 string to binary writer.
		/// </summary>
		/// <param name="bw"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void WriteNTString(this BinaryWriter bw, string format, params object[] args)
		{
			bw.WriteString(format, args);

			if (!format.EndsWith("\0"))
				bw.Write('\0');
		}
	}
}
