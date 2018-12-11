using System.Collections.Generic;

namespace DataDogLib
{
	/// <summary>
	/// Represents a list of objects.
	/// </summary>
	public class DataObjectList
	{
		/// <summary>
		/// Returns the list's name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Returns the type of the list's objects.
		/// </summary>
		public DataTypeDefinition Type { get; private set; }

		/// <summary>
		/// Returns a list of all objects.
		/// </summary>
		public List<DataObject> Objects { get; } = new List<DataObject>();

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public DataObjectList(string name, DataTypeDefinition type)
		{
			this.Name = name;
			this.Type = type;
		}
	}
}
