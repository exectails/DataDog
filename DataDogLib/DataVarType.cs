namespace DataDogLib
{
	/// <summary>
	/// A field's type.
	/// </summary>
	public enum DataVarType
	{
		/// <summary>
		/// 1 byte integer value.
		/// </summary>
		Byte,

		/// <summary>
		/// 1 byte value used to store either 0 or 1.
		/// </summary>
		Bool,

		/// <summary>
		/// 4 byte integer value.
		/// </summary>
		Integer,

		/// <summary>
		/// 4 byte unsigned integer value.
		/// </summary>
		Color,

		/// <summary>
		/// 4 byte floating point value.
		/// </summary>
		Float,

		/// <summary>
		/// 4 byte pointer into string area.
		/// </summary>
		String,

		/// <summary>
		/// 4 byte pointer into string area.
		/// </summary>
		Reference,
	}
}
