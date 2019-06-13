using System;
using System.Collections.Generic;

namespace DataDogLib
{
	/// <summary>
	/// Represents a type.
	/// </summary>
	[Serializable]
	public class DataTypeDefinition
	{
		/// <summary>
		/// Returns the type's name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Returns the struct size of objects of this type.
		/// </summary>
		public int Size { get; private set; }

		/// <summary>
		/// Returns the type's fields.
		/// </summary>
		public Dictionary<string, DataFieldDefinition> Fields { get; } = new Dictionary<string, DataFieldDefinition>();

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="size"></param>
		public DataTypeDefinition(string name, int size)
		{
			this.Name = name;
			this.Size = size;
		}

		/// <summary>
		/// Returns string representation of this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Name + "%" + this.Size;
		}
	}
}
