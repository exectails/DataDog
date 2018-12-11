namespace DataDogLib
{
	/// <summary>
	/// Used to specify how a field's value is stored.
	/// </summary>
	public enum DataFieldReadType
	{
		/// <summary>
		/// Stored directly in the data.
		/// </summary>
		Bin,

		/// <summary>
		/// Stored separately in the string area.
		/// </summary>
		String,
	}
}
