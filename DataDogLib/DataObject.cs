using System;
using System.Collections.Generic;
using System.Linq;

namespace DataDogLib
{
	/// <summary>
	/// Represents an object.
	/// </summary>
	[Serializable]
	public class DataObject
	{
		/// <summary>
		/// Gets or set's the object's name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Returns the object's type.
		/// </summary>
		public DataTypeDefinition Type { get; private set; }

		/// <summary>
		/// Returns the the object's fields.
		/// </summary>
		public Dictionary<string, DataField> Fields { get; } = new Dictionary<string, DataField>();

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public DataObject(string name, DataTypeDefinition type)
		{
			this.Name = name;
			this.Type = type;
		}

		/// <summary>
		/// Creates a new object based on given type, initializes
		/// the fields, and returns the object.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static DataObject New(string name, DataTypeDefinition type)
		{
			var obj = new DataObject(name, type);

			foreach (var field in type.Fields.Values.OrderBy(a => a.Offset))
			{
				var newField = new DataField(field.Name, field.VarType);
				obj.Fields.Add(field.Name, newField);
			}

			return obj;
		}
	}
}
