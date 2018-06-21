using System.Collections.Generic;

namespace DataDogLib
{
	public class DataTypeDefinition
	{
		public string Name { get; private set; }
		public int Size { get; private set; }
		public Dictionary<string, DataFieldDefinition> Fields { get; } = new Dictionary<string, DataFieldDefinition>();

		public DataTypeDefinition(string name, int size)
		{
			this.Name = name;
			this.Size = size;
		}

		public override string ToString()
		{
			return this.Name + "%" + this.Size;
		}
	}
}
