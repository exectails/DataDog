using System.Collections.Generic;

namespace DataDogLib
{
	public class DataObject
	{
		public string Name { get; private set; }
		public DataTypeDefinition Type { get; private set; }
		public Dictionary<string, DataField> Fields { get; } = new Dictionary<string, DataField>();

		public DataObject(string name, DataTypeDefinition type)
		{
			this.Name = name;
			this.Type = type;
		}
	}
}
