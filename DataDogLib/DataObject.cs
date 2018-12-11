using System.Collections.Generic;
using System.Linq;

namespace DataDogLib
{
	public class DataObject
	{
		public string Name { get; set; }
		public DataTypeDefinition Type { get; private set; }
		public Dictionary<string, DataField> Fields { get; } = new Dictionary<string, DataField>();

		public DataObject(string name, DataTypeDefinition type)
		{
			this.Name = name;
			this.Type = type;
		}

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
