using System.Collections.Generic;

namespace DataDogLib
{
	public class DataObjectList
	{
		public string Name { get; private set; }
		public DataTypeDefinition Type { get; private set; }
		public List<DataObject> Objects { get; } = new List<DataObject>();

		public DataObjectList(string name, DataTypeDefinition type)
		{
			this.Name = name;
			this.Type = type;
		}
	}
}
